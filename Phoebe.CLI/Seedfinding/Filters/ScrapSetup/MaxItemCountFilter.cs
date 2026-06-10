
namespace Phoebe.CLI;

public class MaxItemCountFilter(int max) : IScrapSetupFilter {
	public bool Evaluate(V81ScrapCalculator calculator) {
		return calculator.ItemCountToSpawn <= max;
	}
}