
namespace Phoebe.CLI;

public class ContainsAllFilter(HashSet<string> Items) : IScrapFilter {
	public bool Evaluate(V81ScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns) {
		if(spawns.Length < Items.Count) return false;
		
		HashSet<string> found = []; // todo: look at some hashset pool?
		foreach(ScrapSpawn spawn in spawns) {
			if(spawn != null && Items.Contains(spawn.Scrap.ItemName)) {
				found.Add(spawn.Scrap.ItemName);
			}
		}
		return found.Count == Items.Count;
	}
}