using Phoebe.Data;

namespace Phoebe.ScrapCalculators;

public interface IScrapCalculator {
	int ItemCountToSpawn { get; }

	void Setup() { }
	void Calculate(ref ScrapSpawn[] items, out int totalValue);
}