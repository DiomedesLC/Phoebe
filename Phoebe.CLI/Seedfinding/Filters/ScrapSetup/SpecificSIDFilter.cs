
namespace Phoebe.CLI;

public class SpecificSIDFilter(HashSet<string> ItemNames) : IScrapSetupFilter {
	public bool Evaluate(IScrapCalculator calculator) {
		return calculator.SingleItemDay != null && ItemNames.Contains(calculator.SingleItemDay.ItemName);
	}
}