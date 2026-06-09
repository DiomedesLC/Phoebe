using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Phoebe;

public record PhoebeMoonInfo(
	string MoonName,
	int MinScrap,
	int MaxScrap,
	List<(int Rarity, PhoebeScrapInfo Scrap)> SpawnableScrap,
	List<(int Rarity, PhoebeDungeonInfo Dungeon)> PossibleDungeons
) {
	RarityList? _cachedScrapWeights, _cachedDungeonWeights = null;

	public RarityList GetScrapRarityList() {
		_cachedScrapWeights ??= new RarityList(SpawnableScrap.Select(it => it.Rarity).ToArray());
		return _cachedScrapWeights;
	}

	public RarityList GetDungeonRarityList() {
		_cachedDungeonWeights ??= new RarityList(PossibleDungeons.Select(it => it.Rarity).ToArray());
		return _cachedDungeonWeights;
	}
}