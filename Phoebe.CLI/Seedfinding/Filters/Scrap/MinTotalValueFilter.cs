
namespace Phoebe.CLI;

public class MinTotalValueFilter(int min) : IScrapFilter {
	public bool Evaluate(V81ScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns) {
		return totalValue >= min;
	}
}