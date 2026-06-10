
namespace Phoebe.CLI;

public class SpecificSIDFilter(HashSet<string> ItemNames) : IScrapSetupFilter {
	public bool Evaluate(V81ScrapCalculator calculator) {
		return calculator.IsSingleItemDay() && ItemNames.Contains(calculator.SingleItemDay.ItemName);
	}
}