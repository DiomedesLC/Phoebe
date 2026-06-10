namespace Phoebe.CLI;

public record SeedfindingConfig(
    List<string> Moons,
    int StartSeed = Seedfinder.MIN_SEED,
    int EndSeed = Seedfinder.MAX_SEED,
    bool PhoebeFix = false,

    SeedfindingConfig.ScrapConfig? Scrap = null,
    SeedfindingConfig.DungeonConfig? Interior = null
) { 
    public record ScrapConfig(
        List<string>? Items = null,
        int? MinTotalValue = null,
        int? MaxTotalValue = null,
        int? MinScrap = null,
        int? MaxScrap = null,
        bool? SingleItemDay = null
    );

    public record DungeonConfig(
        List<string>? Flows = null
    );
}