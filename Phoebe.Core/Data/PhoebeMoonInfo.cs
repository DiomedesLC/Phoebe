using System.Diagnostics.CodeAnalysis;

namespace Phoebe;

public record PhoebeMoonInfo(
	string MoonName,
	int MinScrap,
	int MaxScrap,
	List<(int Rarity, PhoebeScrapInfo Scrap)> SpawnableScrap
) {
	[field: AllowNull, MaybeNull]
	public RarityList SpawnableScrapWeights {
		get {
			if(field != null) return field;
			field = new RarityList(SpawnableScrap.Select(it => it.Rarity).ToArray());
			return field;
		}
	} = null;
}