using Phoebe;

namespace Phoebe;

public interface IScrapCalculator {
	int ItemCountToSpawn { get; }
	PhoebeScrapInfo? SingleItemDay { get; }

	void Calculate(ref ScrapSpawn[] items, out int totalValue);
}