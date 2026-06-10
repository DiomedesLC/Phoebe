
namespace Phoebe.CLI;

public class SIDFilter : IScrapSetupFilter {
	public bool Evaluate(IScrapCalculator calculator) {
		return calculator.SingleItemDay != null;
	}
}