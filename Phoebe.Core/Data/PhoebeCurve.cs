using System.Collections;
using Phoebe.Utils.Extensions;

namespace Phoebe.Data;

public class PhoebeCurve : ICollection<KeyValuePair<float, float>>, IDictionary<float, float>, IEnumerable<KeyValuePair<float, float>>, IReadOnlyCollection<KeyValuePair<float, float>>, IReadOnlyDictionary<float, float>, IDictionary {
	public float this[float key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); } // No idea what to do here
	public object? this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); } // No idea what to do here

	public int Count => Keys.Count;

	public bool IsReadOnly => false;

	public ICollection<float> Keys => throw new NotImplementedException(); // No idea what to do here

	public ICollection<float> Values => throw new NotImplementedException(); // No idea what to do here

	public bool IsFixedSize => false;

	public bool IsSynchronized => throw new NotImplementedException(); // No idea what this means

	public object SyncRoot => throw new NotImplementedException(); // No idea what this means

	IEnumerable<float> IReadOnlyDictionary<float, float>.Keys => Keys;

	ICollection IDictionary.Keys => throw new NotImplementedException();

	IEnumerable<float> IReadOnlyDictionary<float, float>.Values => Values;

	ICollection IDictionary.Values => throw new NotImplementedException();

	public void Add(KeyValuePair<float, float> item) {
		Add(item.Key, item.Value);
	}

	public void Add(float key, float value) {
		for(int i = 0; i < Keys.Count; i++) {
			float existingKey = this[i];
			if(existingKey == key) {
				throw new ArgumentException("Key already exists");
			}

			if(existingKey > key) {
				// Insert before this key
				Keys.Insert(i, key);
				Values.Insert(i, value);
				return;
			}
		}
	}

	public void Add(object key, object? value) {
		if(key is float kFloat && value is float vFloat) {
			Add(kFloat, vFloat);
		} else {
			throw new ArgumentException("Key and value must be float");
		}
	}

	public void Clear() {
		Keys.Clear();
		Values.Clear();
	}

	public bool Contains(KeyValuePair<float, float> item) {
		foreach(KeyValuePair<float, float> pair in this) {
			if(pair.Key == item.Key && pair.Value == item.Value) {
				return true;
			}
		}
		return false;
	}

	public bool Contains(object key) {
		if(key is not float kFloat) {
			return false;
		}

		if(!ContainsKey(kFloat)) {
			return false;
		}
		return true;
	}

	public bool ContainsKey(float key) {
		foreach(KeyValuePair<float, float> pair in this) {
			if(pair.Key == key) {
				return true;
			}
		}
		return false;
	}

	public void CopyTo(KeyValuePair<float, float>[] array, int arrayIndex) {
		CopyTo((Array) array, arrayIndex);
	}

	public void CopyTo(Array array, int index) {
		if(array.Length - index < Count) {
			throw new ArgumentException("Not enough space in array with given index {Index}.", nameof(index));
		}

		foreach(KeyValuePair<float, float> pair in this) {
			array.SetValue(pair, index++);
		}
	}

	public IEnumerator<KeyValuePair<float, float>> GetEnumerator() {
		foreach(KeyValuePair<float, float> pair in this) {
			yield return pair;
		}
	}

	public bool Remove(KeyValuePair<float, float> item) {
		if(Contains(item)) {
			Keys.Remove(item.Key);
			Values.Remove(item.Value);
			return true;
		}
		return false;
	}

	public bool Remove(float key) {
		foreach(KeyValuePair<float, float> pair in this) {
			if(pair.Key == key) {
				Remove(pair);
				return true;
			}
		}
		return false;
	}

	public void Remove(object key) {
		if(key is float kFloat) {
			Remove(kFloat);
		}
	}

	public bool TryGetValue(float key, out float value) {
		value = 0f;
		foreach(KeyValuePair<float, float> pair in this) {
			if(pair.Key == key) {
				value = pair.Value;
				return true;
			}
		}
		return false;
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	IDictionaryEnumerator IDictionary.GetEnumerator() {
		throw new NotImplementedException(); // I have no idea how to do this :(
	}
}