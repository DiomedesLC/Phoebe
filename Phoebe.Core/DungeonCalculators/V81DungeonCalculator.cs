namespace Phoebe;

public class V81DungeonCalculator(PhoebeMoonInfo moon, int seed) : IDungeonCalculator {
    Random _levelRandom = ZeekerssRnd.Level(seed);

    public bool HasPhoebeFixMod { get; set; }

	public PhoebeDungeonInfo Calculate() {
        int index;
        if(HasPhoebeFixMod) {
            index = moon.GetDungeonRarityList().GetRandomIndexWeightedFixed(_levelRandom);
        } else {
            index = moon.GetDungeonRarityList().GetRandomIndexWeightedVanilla(_levelRandom);
        }
        return moon.PossibleDungeons[index].Dungeon;
	}
}