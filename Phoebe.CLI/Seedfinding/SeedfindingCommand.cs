using System.Buffers;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using Tomlyn;

namespace Phoebe.CLI;

public class CompletedColumn : ProgressColumn {
    public override IRenderable Render(RenderOptions context, ProgressTask task, TimeSpan deltaTime) {
        return new Markup($"[green]{task.Value:N0}[/]/[dim]{task.MaxValue:N0}[/]", null);
    }
}

public class SeedfindingCommand : Command<SeedfindingCommandSettings> {
    enum NeededStep {
        Interior = 1,
        ScrapSetup = 2,
        ScrapCalc = 3
    }

    class SeedfindingTask(PhoebeMoonInfo moon, ProgressTask task) {
        public PhoebeMoonInfo Moon { get; } = moon;
        public ProgressTask ProgressBar { get; } = task;
        public int TargetTotal { get; set; } = 0;
        public IReadOnlyCollection<int> FoundSeeds;
        public bool Finished;
    }

    class CompiledSeedfindingConfig {
        public List<PhoebeMoonInfo> Moons { get; } = [];

        // todo: these should probably be immutable as to ensure thread safety
        public List<IDungeonFilter> DungeonFilters { get; } = [];
        public List<IScrapSetupFilter> ScrapSetupFilters { get; } = [];
        public List<IScrapFilter> ScrapFilters { get; } = [];

        public int StartSeed, EndSeed;
        public bool PhoebeFix;

        public NeededStep? GetNeededStep() {
            if(ScrapFilters.Count > 0) {
                return NeededStep.ScrapCalc;
            } else if(ScrapSetupFilters.Count > 0) {
                return NeededStep.ScrapSetup;
            } else if(DungeonFilters.Count > 0) {
                return NeededStep.Interior;
            } else {
                return null;
            }
        }
    }

    JsonSerializerSettings JSONSettings = new JsonSerializerSettings {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        Formatting = Formatting.Indented
    };

    TomlSerializerOptions TOMLOptions = new TomlSerializerOptions {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

	protected override int Execute(CommandContext context, SeedfindingCommandSettings settings, CancellationToken cancellationToken) {
        CompiledSeedfindingConfig? cfg = ReadConfig(settings.ConfigPath);
        if(cfg == null) {
            AnsiConsole.MarkupLine("[red]Aborting seed finding[/]");
            return 1;
        }

        NeededStep? step = cfg.GetNeededStep();
        if(step == null) {
            AnsiConsole.MarkupLine("[red]Need at least 1 filter![/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"Needed Step: [yellow]{step}[/] [dim]({(int)step})[/]");

        List<SeedfindingTask> tasks = null!;
        AnsiConsole.Progress()
            .Columns(
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new CompletedColumn(),
                new ElapsedTimeColumn()
            )
            .Start(ctx => {
                tasks = SetupAllTasks(ctx, cfg);

                while(!ctx.IsFinished) {
                    foreach(SeedfindingTask task in tasks) {
                        if(task.ProgressBar.Value < task.TargetTotal) {
                            task.ProgressBar.Increment(1);
                            continue;
                        }
                        if(task.Finished && !task.ProgressBar.IsFinished) {
                            task.ProgressBar.StopTask();
                        }
                    }
                }
            });

        Dictionary<string, IReadOnlyCollection<int>> result = [];
        foreach(SeedfindingTask task in tasks) {
            AnsiConsole.MarkupLine($"Found [green]{task.FoundSeeds.Count}[/] seeds on [yellow]{task.Moon.MoonName}[/]");
            result[task.Moon.MoonName] = task.FoundSeeds;
        }
        if(tasks.Count > 1) {
            AnsiConsole.MarkupLine($"Found [green]{tasks.Sum(it => it.FoundSeeds.Count)}[/] total seeds");
        }
        AnsiConsole.MarkupLine($"Saved results to [yellow]{settings.OutputPath}[/]");
        File.WriteAllText(settings.OutputPath, JsonConvert.SerializeObject(result, JSONSettings));

		return 0;
	}

    CompiledSeedfindingConfig? ReadConfig(string configPath) {
        if(!File.Exists(configPath)) {
            AnsiConsole.MarkupLine($"[red]{configPath}[/] does not exist!");
            return null;
        }

        SeedfindingConfig tomlConfig = TomlSerializer.Deserialize<SeedfindingConfig>(File.ReadAllText(configPath), TOMLOptions)!;
        CompiledSeedfindingConfig cfg = new CompiledSeedfindingConfig() {
            StartSeed = tomlConfig.StartSeed, EndSeed = tomlConfig.EndSeed,
            PhoebeFix = tomlConfig.PhoebeFix  
        };

        List<PhoebeMoonInfo> AllMoonInfos = [];
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Phoebe.CLI.data.v81.json")) {
            if (stream == null) {
                throw new FileNotFoundException($"Could not find v81 resources");
            }

            using (StreamReader reader = new StreamReader(stream)) {
                string fileContent = reader.ReadToEnd();
                AllMoonInfos = JsonConvert.DeserializeObject<List<PhoebeMoonInfo>>(fileContent, JSONSettings)!;
            }
        }

        cfg.Moons.AddRange(AllMoonInfos.Where(it => tomlConfig.Moons.Contains(it.MoonName)));

        SeedfindingConfig.ScrapConfig? scrapConfig = tomlConfig.Scrap;
        if(scrapConfig != null) {
            if(scrapConfig.SingleItemDay == true) { // == true because it could be null
                if(scrapConfig.Items != null) {
                    if(scrapConfig.Items.Count == 0) {
                        AnsiConsole.MarkupLine("[red]'scrap.items' can not be empty! If you do not want to filter by items, remove the property[/]");
                        return null;
                    }
                    cfg.ScrapSetupFilters.Add(new SpecificSIDFilter(scrapConfig.Items.ToHashSet()));
                } else {
                    cfg.ScrapSetupFilters.Add(new SIDFilter());
                }
            } else {
                if(scrapConfig.Items != null) {
                    if(scrapConfig.Items.Count == 0) {
                        AnsiConsole.MarkupLine("[red]'scrap.items' can not be empty! If you do not want to filter by items, remove the property[/]");
                        return null;
                    }
                    cfg.ScrapFilters.Add(new ContainsAllFilter(scrapConfig.Items.ToHashSet()));
                }
            }

            if(scrapConfig.MinScrap != null) {
                cfg.ScrapSetupFilters.Add(new MinItemCountFilter(scrapConfig.MinScrap.Value));
            }
            if(scrapConfig.MaxScrap != null) {
                cfg.ScrapSetupFilters.Add(new MinItemCountFilter(scrapConfig.MaxScrap.Value));
            }
            if(scrapConfig.MinTotalValue != null) {
                if(!cfg.PhoebeFix) {
                    AnsiConsole.MarkupLine("[yellow]Without the PhoebeFix Mod scrap value is calculated incorrectly, it can be somewhat accurate on small facility layouts [dim]('scrap.min_total_value' enabled)[/]");
                }
                cfg.ScrapFilters.Add(new MinTotalValueFilter(scrapConfig.MinTotalValue.Value));
            }
            if(scrapConfig.MaxTotalValue != null) {
                if(!cfg.PhoebeFix) {
                    AnsiConsole.MarkupLine("[yellow]Without the PhoebeFix Mod scrap value is calculated incorrectly, it can be somewhat accurate on small facility layouts [dim]('scrap.max_total_value' enabled)[/]");
                }
                cfg.ScrapFilters.Add(new MaxTotalValueFilter(scrapConfig.MaxTotalValue.Value));
            }
        }

        SeedfindingConfig.DungeonConfig? dungeonConfig = tomlConfig.Interior;
        if(dungeonConfig != null) {
            if(dungeonConfig.Flows != null) {
                if(dungeonConfig.Flows.Count == 0) {
                    AnsiConsole.MarkupLine("[red]'interior.flows' can not be empty! If you do not want to filter by interiors, remove the property[/]");
                    return null;
                }
                cfg.DungeonFilters.Add(new DungeonFlowNameFilter(dungeonConfig.Flows.ToHashSet()));
            }
        }

        return cfg;
    }

    List<SeedfindingTask> SetupAllTasks(ProgressContext ctx, CompiledSeedfindingConfig cfg) {
        List<SeedfindingTask> tasks = [];
        NeededStep step = cfg.GetNeededStep()!.Value; // i love c# nullable

        foreach(PhoebeMoonInfo moon in cfg.Moons) {
            Seedfinder seedfinder = new Seedfinder() {
                StartSeed = cfg.StartSeed,
                EndSeed = cfg.EndSeed,
                CustomChunkSize = 10_000
            };
            ProgressTask task = ctx.AddTask(moon.MoonName, maxValue: seedfinder.SearchedSeeds());
            SeedfindingTask seedfindingTask = new SeedfindingTask(moon, task);
            seedfinder.ProgressCallback = new Progress<int>((total) => {
                seedfindingTask.TargetTotal = total;
            });
            tasks.Add(seedfindingTask);

            Task.Run(() => {
                seedfindingTask.FoundSeeds = seedfinder.Where(seed => {
                    V81DungeonCalculator dungeonCalculator = new V81DungeonCalculator(moon, seed) {
                        HasPhoebeFixMod = cfg.PhoebeFix
                    };
                    PhoebeDungeonInfo dungeonInfo = dungeonCalculator.Calculate();

                    if(cfg.DungeonFilters.Any(it => !it.Evaluate(dungeonInfo))) {
                        return false;
                    }

                    if(step == NeededStep.Interior) { // we don't need anything past here, so we can early return
                        return true;
                    }

                    V81ScrapCalculator scrapCalculator = new V81ScrapCalculator(moon, seed) {
                        HasPhoebeFixMod = cfg.PhoebeFix,
                        AdditionalDungeonScrap = dungeonInfo.AdditionalScrap
                    };
                    scrapCalculator.Setup();

                    if(cfg.ScrapSetupFilters.Any(it => !it.Evaluate(scrapCalculator))) {
                        return false;
                    }
                    if(step == NeededStep.ScrapSetup) { // we don't need anything past here, so we can early return
                        return true;
                    }

                    ScrapSpawn[] spawned = ArrayPool<ScrapSpawn>.Shared.Rent(scrapCalculator.ItemCountToSpawn);
                    scrapCalculator.Calculate(ref spawned, out int totalValue);

                    if(cfg.ScrapFilters.Any(it => !it.Evaluate(scrapCalculator, totalValue, spawned))) {
                        ArrayPool<ScrapSpawn>.Shared.Return(spawned);
                        return false;
                    }

                    ArrayPool<ScrapSpawn>.Shared.Return(spawned);
                    return true;
                });

                seedfindingTask.Finished = true;
            });
        }

        return tasks;
    }
}