
namespace Phoebe.CLI;

public class MaxTotalValueFilter(int max) : IScrapFilter {
	public bool Evaluate(IScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns) {
		return totalValue <= max;
	}
}