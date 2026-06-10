namespace Phoebe.CLI;

public interface IScrapSetupFilter {
    bool Evaluate(IScrapCalculator calculator);
}