namespace Phoebe.CLI;

public interface IScrapFilter {
    bool Evaluate(IScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns);
}