
namespace Phoebe.CLI;

public class MinItemCountFilter(int min) : IScrapSetupFilter {
	public bool Evaluate(V81ScrapCalculator calculator) {
		return calculator.ItemCountToSpawn >= min;
	}
}