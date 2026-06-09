namespace Phoebe.Utils.Extensions;

static class RandomExtensions {
	internal static T NextItem<T>(this Random random, IList<T> items) {
		return items[random.Next(0, items.Count)];
	}
}