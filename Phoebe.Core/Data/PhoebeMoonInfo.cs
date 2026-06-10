namespace Phoebe;

public record PhoebeMoonInfo(
	string MoonName,

	int MinScrap,
	int MaxScrap,
	List<PhoebeWithRarity<PhoebeScrapInfo>> SpawnableScrap,

	List<PhoebeWithRarity<PhoebeDungeonInfo>> PossibleDungeons,

	List<PhoebeWithRarity<PhoebeWeatherInfo>> PossibleWeathers, // If we want to determine weather we'll likely need additional context related to RoundManager as the game does some logic before choosing weather to determine whether to even give a level a weather type.
	PhoebeWeatherInfo? ForcedWeatherType,

	List<PhoebeOutsideMapObjectInfo> PossibleOutsideMapObjects,
	List<PhoebeInsideMapObjectInfo> PossibleInsideMapObjects,

	int MaxInsideEnemiesPowerCount,
	int MaxOutsideEnemiesPowerCount,
	int MaxDaytimeEnemiesPowerCount,
	int MaxWeedEnemiesPowerCount,

	int MaxInsideDiversityPowerCount,
	int MaxOutsideDiversityPowerCount,
	int MaxDaytimeDiversityPowerCount,
	int MaxWeedDiversityPowerCount,

	PhoebeSpecialEnemyInfo? SpecialInsideEnemy,
	List<PhoebeWithRarity<PhoebeEnemyInfo>> PossibleInsideEnemies,
	List<PhoebeWithRarity<PhoebeEnemyInfo>> PossibleOutsideEnemies,
	List<PhoebeWithRarity<PhoebeEnemyInfo>> PossibleDaytimeEnemies,
	List<PhoebeWithRarity<PhoebeEnemyInfo>> PossibleWeedEnemies,

	PhoebeCurve InsideEnemySpawnChanceThroughoutDay,
	PhoebeCurve OutsideEnemySpawnChanceThroughoutDay,
	PhoebeCurve DaytimeEnemySpawnChanceThroughoutDay,
	PhoebeCurve WeedEnemySpawnChanceThroughoutDay,

	float InsideSpawnProbabilityRange,
	float OutsideSpawnProbabilityRange,
	float DaytimeSpawnProbabilityRange,
	float WeedSpawnProbabilityRange

) {
	RarityList? _cachedScrapWeights, _cachedDungeonWeights, _cachedWeatherWeights = null;
	RarityList? _cachedInsideEnemiesWeights, _cachedOutsideEnemiesWeights, _cachedDaytimeEnemiesWeights, _cachedWeedEnemiesWeights;

	public RarityList GetScrapRarityList() {
		_cachedScrapWeights ??= new RarityList(SpawnableScrap.Select(it => it.Rarity).ToArray());
		return _cachedScrapWeights;
	}

	public RarityList GetDungeonRarityList() {
		_cachedDungeonWeights ??= new RarityList(PossibleDungeons.Select(it => it.Rarity).ToArray());
		return _cachedDungeonWeights;
	}

	public RarityList GetWeatherRarityList() {
		_cachedWeatherWeights ??= new RarityList(PossibleWeathers.Select(it => it.Rarity).ToArray());
		return _cachedWeatherWeights;
	}

	public RarityList GetInsideEnemiesRarityList() {
		_cachedInsideEnemiesWeights ??= new RarityList(PossibleInsideEnemies.Select(it => it.Rarity).ToArray());
		return _cachedInsideEnemiesWeights;
	}

	public RarityList GetOutsideEnemiesRarityList() {
		_cachedOutsideEnemiesWeights ??= new RarityList(PossibleOutsideEnemies.Select(it => it.Rarity).ToArray());
		return _cachedOutsideEnemiesWeights;
	}

	public RarityList GetDaytimeEnemiesRarityList() {
		_cachedDaytimeEnemiesWeights ??= new RarityList(PossibleDaytimeEnemies.Select(it => it.Rarity).ToArray());
		return _cachedDaytimeEnemiesWeights;
	}

	public RarityList GetWeedEnemiesRarityList() {
		_cachedWeedEnemiesWeights ??= new RarityList(PossibleWeedEnemies.Select(it => it.Rarity).ToArray());
		return _cachedWeedEnemiesWeights;
	}
}