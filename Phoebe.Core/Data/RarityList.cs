using System.Collections;
using System.Runtime.CompilerServices;

namespace Phoebe;

public class RarityList(int[] weights) : IEnumerable<int> {
	public int TotalRarity { get; } = weights.Sum();

	[MethodImpl(PhoebeUtils.Optimized)]
	public int GetRandomIndexWeightedVanilla(Random randomSeed) {
		if(weights.Length == 1) return 0;
		if(TotalRarity <= 0) return randomSeed.Next(0, weights.Length);
		float target = (float) randomSeed.NextDouble();
		float current = 0;
		float total = TotalRarity;
		for(int i = 0; i < weights.Length; i++) {
			int rarity = weights[i];
			if(rarity <= 0) continue;

			current += rarity / total;
			if(current >= target) {
				return i;
			}
		}

		return randomSeed.Next(0, weights.Length);
	}

	[MethodImpl(PhoebeUtils.Optimized)]
	public int GetRandomIndexWeightedFixed(Random randomSeed) {
		if(weights.Length == 1) return 0;
		if(TotalRarity <= 0) return randomSeed.Next(0, weights.Length);
		double target = randomSeed.NextDouble();
		double current = 0;
		double total = TotalRarity;
		for(int i = 0; i < weights.Length; i++) {
			int rarity = weights[i];
			if(rarity <= 0) continue;

			current += rarity / total;
			if(current >= target) {
				return i;
			}
		}

		return randomSeed.Next(0, weights.Length);
	}

	public IEnumerator<int> GetEnumerator() {
		return weights.AsEnumerable().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
