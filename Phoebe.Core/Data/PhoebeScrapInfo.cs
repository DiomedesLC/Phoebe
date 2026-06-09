namespace Phoebe;

public record PhoebeScrapInfo(
	string ItemName,
	bool IsTwoHanded,
	int MinValue, int MaxValue,
	List<string> MeshVariants, List<string> MaterialVariants
);