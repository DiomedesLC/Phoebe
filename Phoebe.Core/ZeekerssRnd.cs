namespace Phoebe;

/// <summary>
/// Shortcuts to create new randoms with their correct offsets from the main seed
/// </summary>
internal static class ZeekerssRnd {
	public static Random Anomaly(int seed) => new Random(seed + 5);
	public static Random ScrapVariant(int seed) => new Random(seed + 210);
	public static Random EnemySpawn(int seed) => new Random(seed + 40);
	public static Random DaytimeEnemySpawn(int seed) => new Random(seed + 43);
	public static Random OutsideEnemySpawn(int seed) => new Random(seed + 41);
	public static Random Level(int seed) => new Random(seed);
}