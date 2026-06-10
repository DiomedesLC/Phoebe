using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Phoebe;

// idk the last time the scrap spawning code was changed
// this includes single item days, so it goes back at least as far as v60
// todo: add like a ChallengeMoonInfo? nullable parameter, as that includes an increasedScrapSpawnRateIndex and adds more scrap overall
public class V81ScrapCalculator : IScrapCalculator {
	Random _anomalyRandom;
	Random? _scrapVariantRandom = null;

	// +6 for mineshafts, i don't want to have to bring in a `PhoebeDungeonInfo` into the calculator if i dont have too, although i dont like the outside caller modifying this, this directly
	public int AdditionalDungeonScrap { get; set; } = 0;
	public bool CalculateScrapVariants { get; set; } = true;
	public bool CalculateScrapValue { get; set; } = true;
	public bool HasPhoebeFixMod { get; set; } = false;

	const float scrapAmountMultiplier = 1f;
	const float scrapValueMultiplier = 0.4f;
	private readonly PhoebeMoonInfo _selectedLevel;
	private readonly int _seed;

	public V81ScrapCalculator(
		PhoebeMoonInfo selectedLevel,
		int seed
	) {
		_selectedLevel = selectedLevel;
		_seed = seed;
		_anomalyRandom = ZeekerssRnd.Anomaly(seed);

		ItemCountToSpawn = (int) (_anomalyRandom.Next(_selectedLevel.MinScrap, _selectedLevel.MaxScrap) * scrapAmountMultiplier);
		ItemCountToSpawn += AdditionalDungeonScrap;

		SingleItemDay = CalculateSingleItemDay();
	}

	List<PhoebeWithRarity<PhoebeScrapInfo>> possibleScrap => _selectedLevel.SpawnableScrap;

	public PhoebeScrapInfo? SingleItemDay { get; private set; }
	public int ItemCountToSpawn { get; private set; }

	[MethodImpl(PhoebeUtils.Optimized)]
#if NET10_0
	[MemberNotNullWhen(true, nameof(SingleItemDay))]
#endif
	public bool IsSingleItemDay() => SingleItemDay != null;

	public void Calculate(ref ScrapSpawn[] items, out int totalValue) {
		PhoebeScrapInfo[] scrapToSpawn = ArrayPool<PhoebeScrapInfo>.Shared.Rent(ItemCountToSpawn);

		if(IsSingleItemDay()) {
			for(int i = 0; i < ItemCountToSpawn; i++) {
				scrapToSpawn[i] = SingleItemDay;
			}
		} else {
			for(int i = 0; i < ItemCountToSpawn; i++) {
				int index;
				if(HasPhoebeFixMod) {
					index = _selectedLevel.GetScrapRarityList().GetRandomIndexWeightedFixed(_anomalyRandom);
				} else {
					index = _selectedLevel.GetScrapRarityList().GetRandomIndexWeightedVanilla(_anomalyRandom);
				}
				scrapToSpawn[i] = possibleScrap[index].Item;
			}
		}

		if(CalculateScrapVariants) {
			_scrapVariantRandom = ZeekerssRnd.ScrapVariant(_seed);
		}

		totalValue = 0;
		for(int i = 0; i < ItemCountToSpawn; i++) {
			items[i] = SpawnScrap(scrapToSpawn[i], IsSingleItemDay());
			totalValue += items[i].Value;
		}

		if(CalculateScrapValue && IsSingleItemDay()) {
			int threshold = 600;
			if(SingleItemDay!.IsTwoHanded) {
				threshold = 1500;
			}
			if(totalValue > 4500) {
				totalValue = 0;
				for(int i = 0; i < ItemCountToSpawn; i++) {
					int adjustedValue = (int) (items[i].Value * 0.7f);
					totalValue += adjustedValue;
					items[i] = items[i] with {
						Value = adjustedValue
					};
				}
			} else if(totalValue < threshold) {
				totalValue = 0;
				for(int i = 0; i < ItemCountToSpawn; i++) {
					int adjustedValue = (int) (items[i].Value * 1.4f);
					totalValue += adjustedValue;
					items[i] = items[i] with {
						Value = adjustedValue
					};
				}
			}
		}

		ArrayPool<PhoebeScrapInfo>.Shared.Return(scrapToSpawn);
	}

	ScrapSpawn SpawnScrap(PhoebeScrapInfo scrap, bool isSid = false) {
		// todo
		// this is calculated using anomaly random, but that's also used to determine where it's going to spawn the scrap.
		// randomScrapSpawn = list4[this.AnomalyRandom.Next(0, list4.Count)];

		// this is so dumb and yet it works LMAO
		int scrapValue = 0;
		if(CalculateScrapValue) {
			if(!HasPhoebeFixMod) {
				_anomalyRandom.Next();
				ZeekerssFuncs.RandomNumberInRadius(10, _anomalyRandom);
				ZeekerssFuncs.RandomNumberInRadius(10, _anomalyRandom);
				ZeekerssFuncs.RandomNumberInRadius(10, _anomalyRandom);
			}

			scrapValue = (int) (_anomalyRandom.Next(scrap.MinValue, scrap.MaxValue) * scrapValueMultiplier);

			if(isSid) {
				scrapValue = Math.Clamp(scrapValue, 50, 170);
			}
		}

		string? meshVariant = null, matVariant = null;

		if(_scrapVariantRandom != null) {
			if(scrap.MeshVariants.Count != 0) {
				meshVariant = _scrapVariantRandom.NextItem(scrap.MeshVariants);
			}

			if(scrap.MaterialVariants.Count != 0) {
				matVariant = _scrapVariantRandom.NextItem(scrap.MaterialVariants);
			}
		}

		return new ScrapSpawn(
			scrap, scrapValue, meshVariant, matVariant
		);
	}

	PhoebeScrapInfo? CalculateSingleItemDay() {
		if(_anomalyRandom.Next(0, 500) > 20) {
			return null;
		}
		int sidItem = _anomalyRandom.Next(0, possibleScrap.Count);

		bool hasNormalChance = false;
		for(int i = 0; i < 2; i++) {
			(int rarity, PhoebeScrapInfo scrap) = possibleScrap[sidItem];
			if(rarity >= 5 && !scrap.IsTwoHanded) {
				hasNormalChance = true;
				break;
			}

			sidItem = _anomalyRandom.Next(0, possibleScrap.Count);
		}

		if(!hasNormalChance && _anomalyRandom.Next(0, 100) < 60) {
			return null;
		}

		return possibleScrap[sidItem].Item;
	}
}
