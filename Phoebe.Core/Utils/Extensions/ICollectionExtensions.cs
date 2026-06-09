namespace Phoebe.Utils.Extensions;

public static class ICollectionExtensions {
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items) {
		foreach(T item in items) {
			collection.Add(item);
		}
	}

	public static void Insert<T>(this ICollection<T> collection, int index, T value) {
		if(index < 0 || index > collection.Count) {
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		int capacity = collection.Count + 1;
		List<T> temp = new(capacity);
		for(int i = 0; i < capacity; i++) {
			T collectionValue = collection.GetEnumerator().Current;
			if(i == index) {
				temp.Add(value);
			} else {
				temp.Add(collectionValue);
			}
		}
		collection.Clear();
		collection.AddRange(temp);
	}
}