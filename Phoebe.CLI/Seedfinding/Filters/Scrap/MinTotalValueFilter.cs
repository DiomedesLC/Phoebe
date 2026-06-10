
namespace Phoebe.CLI;

public class MinTotalValueFilter(int min) : IScrapFilter {
	public bool Evaluate(IScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns) {
		return totalValue >= min;
	}
}