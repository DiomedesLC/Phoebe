using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Random = System.Random;

namespace Phoebe.PredictableScrapMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class PredictableScrapMod : BaseUnityPlugin {
	internal new static ManualLogSource Logger { get; private set; } = null!;

	List<int> _overwrittenValues = [];


	public static int GetRandomIndexWeightedFixed(IList<int> weights, Random randomSeed) {
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
		double num2 = randomSeed.NextDouble();
		double num3 = 0;
		for(int i = 0; i < weights.Count; i++) {
			if(weights[i] > 0) {
				num3 += weights[i] / (double) num;
				if(num3 >= num2) {
					return i;
				}
			}
		}

		Logger.LogError($"Still went onto fallback, oh no! Seed = {StartOfRound.Instance.randomMapSeed}");

		// failsafe!!
		return randomSeed.Next(0, weights.Count);
	}
	private void Awake() {
		Logger = base.Logger;

		IL.RoundManager.SpawnScrapInLevel += il => {
			ILCursor c = new ILCursor(il);
			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdstr("Number of scrap to spawn: {0}. minTotalScrapValue: {1}. Total value of items: {2}.")
			);
			c.Emit(OpCodes.Ldloc_0);
			c.Emit(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(RoundManager), "<>c__DisplayClass154_0"), "ScrapToSpawn"));
			c.Emit(OpCodes.Ldloc_2);
			c.EmitDelegate((List<Item> scrapToSpawn, int sidItemIndex) => {
				_overwrittenValues.Clear();
				foreach(Item item in scrapToSpawn) {
					int scrapValue = (int) (RoundManager.Instance.AnomalyRandom.Next(item.minValue, item.maxValue) * RoundManager.Instance.scrapValueMultiplier);

					if(sidItemIndex != -1) {
						scrapValue = Mathf.Clamp(scrapValue, 50, 170);
					}

					_overwrittenValues.Add(scrapValue);
				}
			});

			// Remove vanilla scrap calculation to avoid duplicating work.
			c.GotoNext(
				MoveType.After,
				i => i.MatchLdloc(18),
				i => i.MatchLdcR4(0f),
				i => i.MatchStfld<GrabbableObject>(nameof(GrabbableObject.fallTime))
			);
			int vanillaScrapCalcStart = c.Index;
			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdloc(4)
			);
			int instructionsToRemove = c.Index - vanillaScrapCalcStart;
			c.Index = vanillaScrapCalcStart;
			c.RemoveRange(instructionsToRemove);

			// Fill the list as normal
			c.Emit(OpCodes.Ldloc, 3); // load list of scrap values
			c.Emit(OpCodes.Ldloc, 17);
			c.Emit(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(RoundManager), "<>c__DisplayClass154_1"), "i"));
			c.EmitDelegate((List<int> scrapValues, int i) => {
				scrapValues.Add(_overwrittenValues[i]);
			});
		};

		On.RoundManager.GetRandomWeightedIndex += (orig, self, weights, randomSeed) => {
			if(randomSeed == null) randomSeed = self.AnomalyRandom;
			return GetRandomIndexWeightedFixed(weights, randomSeed);
		};
		On.RoundManager.GetRandomWeightedIndexList += (orig, self, weights, randomSeed) => {
			if(randomSeed == null) randomSeed = self.AnomalyRandom;
			return GetRandomIndexWeightedFixed(weights, randomSeed);
		};

		Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
	}
}
