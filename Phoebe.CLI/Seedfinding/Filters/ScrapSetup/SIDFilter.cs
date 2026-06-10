
namespace Phoebe.CLI;

public class SIDFilter : IScrapSetupFilter {
	public bool Evaluate(V81ScrapCalculator calculator) {
		return calculator.IsSingleItemDay();
	}
}