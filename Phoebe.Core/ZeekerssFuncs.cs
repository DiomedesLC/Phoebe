using System.Runtime.CompilerServices;

namespace Phoebe;

/// <summary>
/// These are functions copied straight from the decompiler with minimal changes.
/// </summary>
static class ZeekerssFuncs {
	// Copied from RoundManager
	// This in-game has two variations, one int[], one List<int>, but i combined them here because that was dumb.
	// It's also changed to be static and always require the randomSeed argument, alongside throwing an exception instead of returning -1
	// You should be using RarityList<T> instead! it's much more performant
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static int GetRandomIndexWeighted(IList<int> weights, Random randomSeed) {
		if(weights.Count == 0) {
			throw new ArgumentException("Weights cannot be empty!", nameof(weights));
		}

		int num = 0;
		foreach(int t in weights) {
			if(t >= 0) {
				num += t;
			}
		}
		if(weights.Count == 1) {
			return 0;
		}
		if(num <= 0) {
			return randomSeed.Next(0, weights.Count);
		}
		float num2 = (float) randomSeed.NextDouble();
		float num3 = 0;
		for(int i = 0; i < weights.Count; i++) {
			if((float) weights[i] > 0f) {
				num3 += (float) weights[i] / (float) num;
				if(num3 >= num2) {
					return i;
				}
			}
		}

		//throw new Exception($"Error while calculating random weighted index. num2 = {num2}, num3 = {num3} Weights given: {string.Join(", ", weights.Select(it => it.ToString()))}");
		Console.WriteLine("Error while calculating random weighted index. Choosing randomly. Weights given:");

		// failsafe!!
		return randomSeed.Next(0, weights.Count);
	}

	public static float RandomNumberInRadius(float radius, Random randomSeed) {
		return ((float) randomSeed.NextDouble() - 0.5f) * radius;
	}
}
