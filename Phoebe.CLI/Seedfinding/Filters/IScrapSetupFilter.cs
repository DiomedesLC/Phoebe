namespace Phoebe.CLI;

public interface IScrapSetupFilter {
    bool Evaluate(V81ScrapCalculator calculator);
}