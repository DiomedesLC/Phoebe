using System.Buffers;
using Newtonsoft.Json;
using Phoebe.Data;
using Phoebe.ScrapCalculators;
using Phoebe.Seedfinding;

JsonSerializerSettings JSONSettings = new() {
	ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
	PreserveReferencesHandling = PreserveReferencesHandling.None,
	TypeNameHandling = TypeNameHandling.All,
	Formatting = Formatting.Indented,
	Converters =
	[

	]
};

Console.WriteLine("Hello, World!");
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

PhoebeMoonInfo info = AllMoonInfos.First(it => it.MoonName == "41 Experimentation");
V81ScrapCalculator calculator = new V81ScrapCalculator(info, 6827935);
calculator.Setup();
ScrapSpawn[] spawned = new ScrapSpawn[calculator.ItemCountToSpawn];
calculator.Calculate(ref spawned, out int totalValue);

foreach(ScrapSpawn spawn in spawned) {
	Console.WriteLine($"{spawn.Scrap.ItemName}: {spawn.Value}");
}

Console.WriteLine($"{spawned.Length} items worth {spawned.Sum(it => it.Value)}");
