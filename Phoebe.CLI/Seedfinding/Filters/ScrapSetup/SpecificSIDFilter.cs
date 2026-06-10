
namespace Phoebe.CLI;

public class SpecificSIDFilter(string ItemName) : IScrapSetupFilter {
	public bool Evaluate(V81ScrapCalculator calculator) {
		return calculator.IsSingleItemDay() && calculator.SingleItemDay.ItemName == ItemName;
	}
}