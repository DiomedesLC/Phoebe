
namespace Phoebe.CLI;

public class MinItemCountFilter(int min) : IScrapSetupFilter {
	public bool Evaluate(IScrapCalculator calculator) {
		return calculator.ItemCountToSpawn >= min;
	}
}