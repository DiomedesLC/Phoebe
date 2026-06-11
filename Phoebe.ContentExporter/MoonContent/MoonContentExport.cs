using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

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

	public static PhoebeCurve UnityToPhoebeCurve(AnimationCurve animationCurve) {
		List<PhoebeKeyFrame> phoebeKeyFrames = new(100);
		for(int i = 0; i < 100; i++) {
			float time = i / 100f;
			float value = animationCurve.Evaluate(time);
			phoebeKeyFrames.Add(new PhoebeKeyFrame(time, value));
		}

		return new PhoebeCurve(phoebeKeyFrames);
	}

	public static PhoebeColor UnityToPhoebeColor(Color color) {
		return new(color.r, color.g, color.b, color.a);
	}

	public static PhoebeEnemyInfo EnemyTypeToPhoebeEnemyInfo(EnemyType enemyType) {
		return new PhoebeEnemyInfo(
			enemyType.enemyName,
			UnityToPhoebeCurve(enemyType.probabilityCurve),
			enemyType.useNumberSpawnedFalloff ? UnityToPhoebeCurve(enemyType.numberSpawnedFalloff) : null,
			enemyType.spawnInGroupsOf,
			enemyType.PowerLevel,
			enemyType.DiversityPowerLevel,
			enemyType.MaxCount,
			GetSpawnPoolFromEnemyType(enemyType),
			GetInteriorNameFromID(enemyType.increasedChanceInterior)
		);
	}

	public static SpawnPool GetSpawnPoolFromEnemyType(EnemyType enemyType) {
		if(enemyType.isDaytimeEnemy) {
			return SpawnPool.Daytime;
		}

		if(enemyType.isOutsideEnemy) {
			return SpawnPool.Outside;
		}

		if(enemyType.spawnFromWeeds) {
			return SpawnPool.Weed;
		}

		return SpawnPool.Inside;
	}

	public static string? GetInteriorNameFromID(int interiorID) {
		if(interiorID == -1) {
			return null;
		}

		return interiorID switch {
			0 => "Level1Flow",
			1 => "Level2Flow",
			2 => "Level1FlowExtraLarge",
			3 => "Level1Flow3Exits",
			4 => "Level3Flow",
			_ => null
		};
	}

	public static PhoebeMoonInfo ExportMoon(SelectableLevel level) {
		string planetName = level.PlanetName;

		int minScrap = level.minScrap;
		int maxScrap = level.maxScrap;
		List<PhoebeWithRarity<PhoebeScrapInfo>> spawnableScrap = new();
		foreach(SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap) {
			Item item = spawnableItemWithRarity.spawnableItem;
			spawnableScrap.Add(new PhoebeWithRarity<PhoebeScrapInfo>(spawnableItemWithRarity.rarity, new PhoebeScrapInfo(
				item.itemName, item.twoHanded,
				item.minValue, item.maxValue,
				item.meshVariants.Select(it => it.name).ToList(), item.materialVariants.Select(it => it.name).ToList()
			)));
		}

		List<PhoebeWithRarity<PhoebeDungeonInfo>> possibleDungeons = [];
		foreach(IntWithRarity intWithRarity in level.dungeonFlowTypes) {
			possibleDungeons.Add(new PhoebeWithRarity<PhoebeDungeonInfo>(intWithRarity.rarity, new PhoebeDungeonInfo(
				RoundManager.Instance.dungeonFlowTypes[intWithRarity.id].dungeonFlow.name,
				intWithRarity.id == 4 ? 6 : 0 // mineshaft +6
			)));
		}

		List<PhoebeWithRarity<PhoebeWeatherInfo>> possibleWeathers = [];
		foreach(RandomWeatherWithVariables randomWeatherWithVariables in level.randomWeathers) {
			possibleWeathers.Add(new PhoebeWithRarity<PhoebeWeatherInfo>(100, new PhoebeWeatherInfo(
				randomWeatherWithVariables.weatherType.ToString(),
				randomWeatherWithVariables.weatherVariable,
				randomWeatherWithVariables.weatherVariable2,
				UnityToPhoebeColor(randomWeatherWithVariables.weatherVariableColor)
			)));
		}

		PhoebeWeatherInfo? forcedWeather = null;
		if(level.overrideWeather) {
			RandomWeatherWithVariables? randomWeatherWithVariables = level.randomWeathers.FirstOrDefault(it => it.weatherType == level.overrideWeatherType);
			if(randomWeatherWithVariables == null) {
				goto skipOverrideWeather;
			}

			forcedWeather = new PhoebeWeatherInfo(
				randomWeatherWithVariables.weatherType.ToString(),
				randomWeatherWithVariables.weatherVariable,
				randomWeatherWithVariables.weatherVariable2,
				UnityToPhoebeColor(randomWeatherWithVariables.weatherVariableColor)
			);
		}

	skipOverrideWeather:

		List<PhoebeInsideMapObjectInfo> possibleInsideMapObjects = [];
		foreach(IndoorMapHazard indoorMapHazard in level.indoorMapHazards) {
			possibleInsideMapObjects.Add(new PhoebeInsideMapObjectInfo(
				indoorMapHazard.hazardType.name,
				UnityToPhoebeCurve(indoorMapHazard.numberToSpawn),
				indoorMapHazard.hazardType.allowInMineshaft ? new() : ["Level3Flow"]
			));
		}

		List<PhoebeOutsideMapObjectInfo> possibleOutsideMapObjects = [];
		foreach(SpawnableOutsideObjectWithRarity spawnableOutsideObjectWithRarity in level.spawnableOutsideObjects) {

		}

		int maxInsideEnemyPowerCount = level.maxEnemyPowerCount;
		int maxOutsideEnemyPowerCount = level.maxOutsideEnemyPowerCount;
		int maxDaytimeEnemyPowerCount = level.maxDaytimeEnemyPowerCount;
		int maxWeedEnemyPowerCount = 4;

		int maxInsideDiversityPowerCount = level.maxInsideDiversityPowerCount;
		int maxOutsideDiversityPowerCount = level.maxOutsideDiversityPowerCount;
		int maxDaytimeDiversityPowerCount = 100;
		int maxWeedDiversityPowerCount = 100;

		PhoebeSpecialEnemyInfo? specialInsideEnemy = null;
		if(level.specialEnemyRarity != null && level.specialEnemyRarity.overrideEnemy != null && level.specialEnemyRarity.percentageChance > 0) {
			specialInsideEnemy = new PhoebeSpecialEnemyInfo(EnemyTypeToPhoebeEnemyInfo(level.specialEnemyRarity.overrideEnemy), level.specialEnemyRarity.percentageChance);
		}

		List<PhoebeWithRarity<PhoebeEnemyInfo>> possibleInsideEnemies = [];
		foreach(SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies) {
			possibleInsideEnemies.Add(new PhoebeWithRarity<PhoebeEnemyInfo>(spawnableEnemyWithRarity.rarity, EnemyTypeToPhoebeEnemyInfo(spawnableEnemyWithRarity.enemyType)));
		}
		List<PhoebeWithRarity<PhoebeEnemyInfo>> possibleOutsideEnemies = [];
		foreach(SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.OutsideEnemies) {
			possibleOutsideEnemies.Add(new PhoebeWithRarity<PhoebeEnemyInfo>(spawnableEnemyWithRarity.rarity, EnemyTypeToPhoebeEnemyInfo(spawnableEnemyWithRarity.enemyType)));
		}
		List<PhoebeWithRarity<PhoebeEnemyInfo>> possibleDaytimeEnemies = [];
		foreach(SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.DaytimeEnemies) {
			possibleDaytimeEnemies.Add(new PhoebeWithRarity<PhoebeEnemyInfo>(spawnableEnemyWithRarity.rarity, EnemyTypeToPhoebeEnemyInfo(spawnableEnemyWithRarity.enemyType)));
		}
		List<PhoebeWithRarity<PhoebeEnemyInfo>> PossibleWeedEnemies = [];
		foreach(SpawnableEnemyWithRarity spawnableEnemyWithRarity in RoundManager.Instance.WeedEnemies) {
			PossibleWeedEnemies.Add(new PhoebeWithRarity<PhoebeEnemyInfo>(spawnableEnemyWithRarity.rarity, EnemyTypeToPhoebeEnemyInfo(spawnableEnemyWithRarity.enemyType)));
		}

		PhoebeCurve insideEnemySpawnChanceThroughoutDay = UnityToPhoebeCurve(level.enemySpawnChanceThroughoutDay);
		PhoebeCurve outsideEnemySpawnChanceThroughoutDay = UnityToPhoebeCurve(level.outsideEnemySpawnChanceThroughDay);
		PhoebeCurve daytimeEnemySpawnChanceThroughoutDay = UnityToPhoebeCurve(level.daytimeEnemySpawnChanceThroughDay);
		PhoebeCurve weedEnemySpawnChanceThroughoutDay = UnityToPhoebeCurve(AnimationCurve.Constant(0f, 1f, 2f));

		float insideSpawnProbabilityRange = level.spawnProbabilityRange;
		float outsideSpawnProbabilityRange = 3f;
		float daytimeSpawnProbabilityRange = level.daytimeEnemiesProbabilityRange;
		float weedSpawnProbabilityRange = 1f;

		return new PhoebeMoonInfo(
			planetName,

			minScrap,
			maxScrap,
			spawnableScrap,

			possibleDungeons,

			possibleWeathers,
			forcedWeather,

			possibleOutsideMapObjects,
			possibleInsideMapObjects,

			maxInsideEnemyPowerCount,
			maxOutsideEnemyPowerCount,
			maxDaytimeEnemyPowerCount,
			maxWeedEnemyPowerCount,

			maxInsideDiversityPowerCount,
			maxOutsideDiversityPowerCount,
			maxDaytimeDiversityPowerCount,
			maxWeedDiversityPowerCount,

			specialInsideEnemy,
			possibleInsideEnemies,
			possibleOutsideEnemies,
			possibleDaytimeEnemies,
			PossibleWeedEnemies,

			insideEnemySpawnChanceThroughoutDay,
			outsideEnemySpawnChanceThroughoutDay,
			daytimeEnemySpawnChanceThroughoutDay,
			weedEnemySpawnChanceThroughoutDay,

			insideSpawnProbabilityRange,
			outsideSpawnProbabilityRange,
			daytimeSpawnProbabilityRange,
			weedSpawnProbabilityRange
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