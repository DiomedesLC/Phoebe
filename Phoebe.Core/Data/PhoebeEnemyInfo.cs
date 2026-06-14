namespace Phoebe;

public record PhoebeSpecialEnemyInfo(PhoebeEnemyInfo enemyInfo, float percentChance);

public record PhoebeEnemyInfo(
	string EnemyName,
	PhoebeCurve ProbabilityCurve,
	PhoebeCurve? NumberSpawnedFallOff,
	int SpawnInGroupsOf,
	float PowerLevel,
	int DiversityPowerLevel,
	int MaxCount,
	SpawnPool SpawnPool,
	string? IncreasedChanceInteriorName,
	bool CanDie
);

public enum SpawnPool {
	Outside,
	Inside,
	Daytime,
	Weed
}