using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Phoebe.ContentExporter.MoonContent;

public static class MoonContentExport {
	internal static readonly JsonSerializerSettings JSONSettings = new() {
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		TypeNameHandling = TypeNameHandling.All,
		Formatting = Formatting.Indented,
		Converters =
		[

		]
	};

	public static PhoebeMoonInfo ExportMoon(SelectableLevel level) {
		List<(int Rarity, PhoebeScrapInfo Scrap)> spawnableScrap = new();
		foreach(SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap) {
			Item item = spawnableItemWithRarity.spawnableItem;
			spawnableScrap.Add((spawnableItemWithRarity.rarity, new PhoebeScrapInfo(
				item.itemName, item.twoHanded,
				item.minValue, item.maxValue,
				item.meshVariants.Select(it => it.name).ToList(), item.materialVariants.Select(it => it.name).ToList()
			)));
		}

		List<(int Rarity, PhoebeDungeonInfo Dungeon)> possibleDungeons = [];
		foreach(IntWithRarity intWithRarity in level.dungeonFlowTypes) {
			possibleDungeons.Add((intWithRarity.rarity, new PhoebeDungeonInfo(
				RoundManager.Instance.dungeonFlowTypes[intWithRarity.id].dungeonFlow.name,
				intWithRarity.id == 4 ? 6 : 0 // mineshaft +6
			)));
		}

		return new PhoebeMoonInfo(
			level.PlanetName,
			level.minScrap,
			level.maxScrap,
			spawnableScrap,
			possibleDungeons
		);
	}

	public static void ExportAllMoonData() {
		List<PhoebeMoonInfo> AllMoonInfos = new();

		foreach(SelectableLevel selectableLevel in StartOfRound.Instance.levels) {
			AllMoonInfos.Add(ExportMoon(selectableLevel));
		}

		Directory.CreateDirectory(GetFolder());
		string filePath = Path.Combine(GetFolder(), "mooninfo.json");
		try {
			PhoebeContentExporter.Logger.LogDebug($"saving onto: ({Path.GetFileName(filePath)})");

			string allText = JsonConvert.SerializeObject(AllMoonInfos, JSONSettings);
			File.WriteAllText(filePath, allText);

			PhoebeContentExporter.Logger.LogDebug("saved");
		} catch(Exception e) {
			PhoebeContentExporter.Logger.LogError($"Error happened while trying to save Phoebe Exported Data ({Path.GetFileName(filePath)}):\n{e}");
		}
	}

	private static string GetFolder() {
		return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "phoebe-export-data");
	}
}