using BepInEx;
using BepInEx.Logging;
using Phoebe.ContentExporter.MoonContent;

namespace Phoebe.ContentExporter;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class PhoebeContentExporter : BaseUnityPlugin {
	internal new static ManualLogSource Logger { get; private set; } = null!;

	private void Awake() {
		Logger = base.Logger;
		On.StartOfRound.Start += (orig, self) => {
			orig(self);
			MoonContentExport.ExportAllMoonData();
		};
		/* todo: crashing
		On.RoundManager.SpawnScrapInLevel += (orig, self) => {
			orig(self);
			V81ScrapCalculator calculator = new V81ScrapCalculator(MoonContentExport.ExportMoon(self.currentLevel), self.playersManager.randomMapSeed);
			calculator.Setup();
			calculator.Setup();
			ScrapSpawn[] items = new ScrapSpawn[calculator.ItemCountToSpawn];
			calculator.Calculate(ref items, out int totalValue);
			foreach(ScrapSpawn spawn in items) {
				Logger.LogDebug((spawn.Scrap.ItemName));
			}
			Logger.LogInfo($"{items.Length} items worth {totalValue}");
			GrabbableObject[] grabbableObjects = FindObjectsOfType<GrabbableObject>();
			Logger.LogInfo($"actual items {grabbableObjects.Length} worht {grabbableObjects.Sum(it => it.scrapValue)}");
		};
		*/

		Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
	}
}