using System.Buffers;
using System.IO.Compression;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

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

    JsonSerializerSettings JSONSettings = new() {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        Converters =
        [

        ]
    };

	protected override int Execute(CommandContext context, SeedfindingCommandSettings settings, CancellationToken cancellationToken) {
        CompiledSeedfindingConfig cfg = new CompiledSeedfindingConfig() {
            StartSeed = Seedfinder.MIN_SEED, EndSeed = Seedfinder.MAX_SEED  
        };

        List<PhoebeMoonInfo> AllMoonInfos = new();
        string filePath = "Phoebe.CLI/data/v81.json";
        Console.WriteLine("Loading existing file");
        try {
            AllMoonInfos = JsonConvert.DeserializeObject<List<PhoebeMoonInfo>>(File.ReadAllText(filePath), JSONSettings)!;
        } catch(Exception e) {
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error happened while trying to load Phoebe Exported Data ({Path.GetFileName(filePath)}):\n{e}");
            Console.ForegroundColor = prevColor;
        }

        PhoebeMoonInfo info = AllMoonInfos.First(it => it.MoonName == "220 Assurance");
        cfg.Moons.Add(info);
        cfg.ScrapSetupFilters.Add(new SpecificSIDFilter("Zed Dog"));

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
                        }
                    }
                }
            });

        foreach(SeedfindingTask task in tasks) {
            AnsiConsole.MarkupLine($"Found [green]{task.FoundSeeds.Count}[/] seeds on [yellow]{task.Moon.MoonName}[/]");
        }
        if(tasks.Count > 1) {
            AnsiConsole.MarkupLine($"Found [green]{tasks.Sum(it => it.FoundSeeds.Count)}[/] total seeds");
        }
        AnsiConsole.MarkupLine($"Saved results to [yellow]{settings.OutputPath}[/]");

		return 0;
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
                        HasPhoebeFixMod = cfg.PhoebeFix
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
            });
        }

        return tasks;
    }
}