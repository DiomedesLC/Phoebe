namespace Phoebe.CLI;

public interface IScrapFilter {
    bool Evaluate(V81ScrapCalculator calculator, int totalValue, ScrapSpawn[] spawns);
}