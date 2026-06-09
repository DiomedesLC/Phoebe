namespace Phoebe;

public record ScrapSpawn(
	PhoebeScrapInfo Scrap,
	int Value,
	string? MeshVariant, string? MaterialVariant
);