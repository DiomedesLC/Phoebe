namespace Phoebe.Data;

public record ScrapSpawn(
	PhoebeScrapInfo Scrap,
	int Value,
	string? MeshVariant, string? MaterialVariant
);