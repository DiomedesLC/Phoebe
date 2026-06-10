
namespace Phoebe.CLI;

public class MaxItemCountFilter(int max) : IScrapSetupFilter {
	public bool Evaluate(IScrapCalculator calculator) {
		return calculator.ItemCountToSpawn <= max;
	}
}