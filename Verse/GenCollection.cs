using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class GenCollection
{
	private static class SortStableTempList<T>
	{
		public static List<Pair<T, int>> list = new List<Pair<T, int>>();

		public static bool working;
	}

	private class ComparerChain<T> : IComparer<T>
	{
		private readonly IComparer<T> first;

		private readonly IComparer<T> second;

		public ComparerChain(IComparer<T> first, IComparer<T> second)
		{
			this.first = first;
			this.second = second;
		}

		public int Compare(T x, T y)
		{
			int num = first.Compare(x, y);
			if (num != 0)
			{
				return num;
			}
			return second.Compare(x, y);
		}
	}

	private class DescendingComparer<T> : IComparer<T>
	{
		private readonly IComparer<T> cmp;

		public DescendingComparer(IComparer<T> cmp)
		{
			this.cmp = cmp;
		}

		public int Compare(T x, T y)
		{
			return -cmp.Compare(x, y);
		}
	}

	public class DictionarySlice<T, V> : IReadOnlyDictionary<T, V>, IEnumerable<KeyValuePair<T, V>>, IEnumerable, IReadOnlyCollection<KeyValuePair<T, V>>
	{
		private HashSet<T> keys;

		private IReadOnlyDictionary<T, V> underlying;

		public V this[T key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException();
				}
				if (!keys.Contains(key))
				{
					throw new KeyNotFoundException(key.ToStringSafe());
				}
				return underlying[key];
			}
		}

		public IEnumerable<V> Values
		{
			get
			{
				foreach (T key in keys)
				{
					if (key != null && underlying.TryGetValue(key, out var value))
					{
						yield return value;
					}
				}
			}
		}

		public IEnumerable<T> Keys => keys;

		public int Count => keys.Count;

		public bool ContainsKey(T key)
		{
			if (key != null)
			{
				return keys.Contains(key);
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public DictionarySlice(IReadOnlyDictionary<T, V> dict, IEnumerable<T> keys)
		{
			underlying = dict;
			this.keys = new HashSet<T>(keys);
		}

		public IEnumerator<KeyValuePair<T, V>> GetEnumerator()
		{
			foreach (T key in keys)
			{
				if (key != null && underlying.TryGetValue(key, out var value))
				{
					yield return new KeyValuePair<T, V>(key, value);
				}
			}
		}

		public bool TryGetValue(T key, out V value)
		{
			if (!keys.Contains(key))
			{
				value = default(V);
				return false;
			}
			return underlying.TryGetValue(key, out value);
		}
	}

	public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable)
		{
			hashSet.Add(item);
		}
	}

	public static bool NullOrEmpty<T>(this HashSet<T> hashSet)
	{
		if (hashSet != null)
		{
			return !hashSet.Any();
		}
		return true;
	}

	public static bool Empty<T>(this Queue<T> queue)
	{
		return queue.Count == 0;
	}

	public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dest, IDictionary<TKey, TValue> source)
	{
		foreach (KeyValuePair<TKey, TValue> item in source)
		{
			dest.Add(item.Key, item.Value);
		}
	}

	public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, ICollection<TKey> remove)
	{
		foreach (TKey item in remove)
		{
			if (dict.ContainsKey(item))
			{
				dict.Remove(item);
			}
		}
	}

	public static void SetOrAdd<K, V>(this Dictionary<K, V> dict, K key, V value)
	{
		dict[key] = value;
	}

	public static void AddDistinct<K, V>(this Dictionary<K, V> dict, K key, V value)
	{
		dict.TryAdd(key, value);
	}

	public static void Increment<K>(this Dictionary<K, int> dict, K key)
	{
		if (!dict.TryAdd(key, 1))
		{
			dict[key]++;
		}
	}

	public static bool SharesElementWith<T>(this IEnumerable<T> source, IEnumerable<T> other)
	{
		if (source is IList<T> list && other is IList<T> list2)
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					if (EqualityComparer<T>.Default.Equals(list[i], list2[j]))
					{
						return true;
					}
				}
			}
			return false;
		}
		return source.Any(other.Contains<T>);
	}

	public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source, IList<T> workingList = null)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (workingList == null)
		{
			workingList = source.ToList();
		}
		else
		{
			workingList.Clear();
			foreach (T item in source)
			{
				workingList.Add(item);
			}
		}
		for (int countUnChosen = workingList.Count; countUnChosen > 0; countUnChosen--)
		{
			int rand = Rand.Range(0, countUnChosen);
			yield return workingList[rand];
			IList<T> list = workingList;
			int index = rand;
			IList<T> list2 = workingList;
			int index2 = countUnChosen - 1;
			T value = workingList[countUnChosen - 1];
			T value2 = workingList[rand];
			list[index] = value;
			list2[index2] = value2;
		}
	}

	public static T RandomElement<T>(this HashSet<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Count == 0)
		{
			Log.Warning("Getting random element from empty hashset.");
			return default(T);
		}
		HashSet<T>.Enumerator enumerator = source.GetEnumerator();
		int num = Rand.Range(0, source.Count);
		T result = default(T);
		while (enumerator.MoveNext())
		{
			if (num == 0)
			{
				result = enumerator.Current;
				break;
			}
			num--;
		}
		enumerator.Dispose();
		return result;
	}

	public static T RandomElement<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				Log.Warning("Getting random element from empty collection.");
				return default(T);
			}
			return source.ElementAt(Rand.Range(0, collection.Count));
		}
		List<T> list = source.ToList();
		if (list.Count == 0)
		{
			Log.Warning("Getting random element from empty collection.");
			return default(T);
		}
		return list[Rand.Range(0, list.Count)];
	}

	public static T RandomElementWithFallback<T>(this IEnumerable<T> source, T fallback = default(T))
	{
		if (source.TryRandomElement(out var result))
		{
			return result;
		}
		return fallback;
	}

	public static bool TryRandomElement<T>(this IEnumerable<T> source, Predicate<T> predicate, out T result)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		IList<T> list = source as IList<T>;
		if (list != null)
		{
			if (list.Count == 0)
			{
				result = default(T);
				return false;
			}
		}
		else
		{
			list = source.ToList();
			if (!list.Any())
			{
				result = default(T);
				return false;
			}
		}
		if (!list.Any((T x) => predicate(x)))
		{
			result = default(T);
			return false;
		}
		result = list.Where((T x) => predicate(x)).RandomElement();
		return true;
	}

	public static bool TryRandomElement<T>(this HashSet<T> source, out T result)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Count == 0)
		{
			result = default(T);
			return false;
		}
		HashSet<T>.Enumerator enumerator = source.GetEnumerator();
		int num = Rand.Range(0, source.Count);
		result = default(T);
		while (enumerator.MoveNext())
		{
			if (num == 0)
			{
				result = enumerator.Current;
				break;
			}
			num--;
		}
		enumerator.Dispose();
		return true;
	}

	public static bool TryRandomElement<T>(this IEnumerable<T> source, out T result)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		ICollection<T> collection = source as ICollection<T>;
		if (collection != null)
		{
			if (collection.Count == 0)
			{
				result = default(T);
				return false;
			}
		}
		else
		{
			collection = source.ToList();
			if (!collection.Any())
			{
				result = default(T);
				return false;
			}
		}
		result = collection.RandomElement();
		return true;
	}

	public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
	{
		float num = 0f;
		IList<T> list = source as IList<T>;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				float num2 = weightSelector(list[i]);
				if (num2 < 0f)
				{
					Log.Error("Negative weight in selector: " + num2 + " from " + list[i]);
					num2 = 0f;
				}
				num += num2;
			}
			if (list.Count == 1 && num > 0f)
			{
				return list[0];
			}
		}
		else
		{
			int num3 = 0;
			foreach (T item in source)
			{
				num3++;
				float num4 = weightSelector(item);
				if (num4 < 0f)
				{
					string text = num4.ToString();
					T val = item;
					Log.Error("Negative weight in selector: " + text + " from " + val);
					num4 = 0f;
				}
				num += num4;
			}
			if (num3 == 1 && num > 0f)
			{
				return source.First();
			}
		}
		if (num <= 0f)
		{
			Log.Error("RandomElementByWeight with totalWeight=" + num + " - use TryRandomElementByWeight.");
			return default(T);
		}
		float num5 = Rand.Value * num;
		float num6 = 0f;
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				float num7 = weightSelector(list[j]);
				if (!(num7 <= 0f))
				{
					num6 += num7;
					if (num6 >= num5)
					{
						return list[j];
					}
				}
			}
		}
		else
		{
			foreach (T item2 in source)
			{
				float num8 = weightSelector(item2);
				if (!(num8 <= 0f))
				{
					num6 += num8;
					if (num6 >= num5)
					{
						return item2;
					}
				}
			}
		}
		return default(T);
	}

	public static T RandomElementByWeightWithFallback<T>(this IEnumerable<T> source, Func<T, float> weightSelector, T fallback = default(T))
	{
		if (source.TryRandomElementByWeight(weightSelector, out var result))
		{
			return result;
		}
		return fallback;
	}

	public static bool TryRandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector, out T result)
	{
		if (source is IList<T> list)
		{
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				float num2 = weightSelector(list[i]);
				if (num2 < 0f)
				{
					Log.Error("Negative weight in selector: " + num2 + " from " + list[i]);
					num2 = 0f;
				}
				num += num2;
			}
			if (list.Count == 1 && num > 0f)
			{
				result = list[0];
				return true;
			}
			if (num == 0f)
			{
				result = default(T);
				return false;
			}
			num *= Rand.Value;
			for (int j = 0; j < list.Count; j++)
			{
				float num3 = weightSelector(list[j]);
				if (!(num3 <= 0f))
				{
					num -= num3;
					if (num <= 0f)
					{
						result = list[j];
						return true;
					}
				}
			}
		}
		IEnumerator<T> enumerator = source.GetEnumerator();
		result = default(T);
		float num4 = 0f;
		while (num4 == 0f && enumerator.MoveNext())
		{
			result = enumerator.Current;
			num4 = weightSelector(result);
			if (num4 < 0f)
			{
				string text = num4.ToString();
				T val = result;
				Log.Error("Negative weight in selector: " + text + " from " + val);
				num4 = 0f;
			}
		}
		if (num4 == 0f)
		{
			result = default(T);
			return false;
		}
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			float num5 = weightSelector(current);
			if (num5 < 0f)
			{
				string text2 = num5.ToString();
				T val = current;
				Log.Error("Negative weight in selector: " + text2 + " from " + val);
				num5 = 0f;
			}
			if (Rand.Range(0f, num4 + num5) >= num4)
			{
				result = current;
			}
			num4 += num5;
		}
		return true;
	}

	public static T RandomElementByWeightWithDefault<T>(this IEnumerable<T> source, Func<T, float> weightSelector, float defaultValueWeight)
	{
		if (defaultValueWeight < 0f)
		{
			Log.Error("Negative default value weight.");
			defaultValueWeight = 0f;
		}
		float num = 0f;
		foreach (T item in source)
		{
			float num2 = weightSelector(item);
			if (num2 < 0f)
			{
				string text = num2.ToString();
				T val = item;
				Log.Error("Negative weight in selector: " + text + " from " + val);
				num2 = 0f;
			}
			num += num2;
		}
		float num3 = defaultValueWeight + num;
		if (num3 <= 0f)
		{
			Log.Error("RandomElementByWeightWithDefault with totalWeight=" + num3);
			return default(T);
		}
		if (Rand.Value < defaultValueWeight / num3 || num == 0f)
		{
			return default(T);
		}
		return source.RandomElementByWeight(weightSelector);
	}

	public static T FirstOrFallback<T>(this IEnumerable<T> source, T fallback = default(T))
	{
		using IEnumerator<T> enumerator = source.GetEnumerator();
		if (enumerator.MoveNext())
		{
			return enumerator.Current;
		}
		return fallback;
	}

	public static T FirstOrFallback<T>(this IEnumerable<T> source, Func<T, bool> predicate, T fallback = default(T))
	{
		return source.Where(predicate).FirstOrFallback(fallback);
	}

	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		return source.MaxBy(selector, Comparer<TKey>.Default);
	}

	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) > 0)
			{
				val = current;
				y = val2;
			}
		}
		return val;
	}

	public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource fallback = default(TSource))
	{
		return source.MaxByWithFallback(selector, Comparer<TKey>.Default, fallback);
	}

	public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource fallback = default(TSource))
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return fallback;
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) > 0)
			{
				val = current;
				y = val2;
			}
		}
		return val;
	}

	public static bool TryMaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, out TSource value)
	{
		return source.TryMaxBy(selector, Comparer<TKey>.Default, out value);
	}

	public static bool TryMaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, out TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			value = default(TSource);
			return false;
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) > 0)
			{
				val = current;
				y = val2;
			}
		}
		value = val;
		return true;
	}

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		return source.MinBy(selector, Comparer<TKey>.Default);
	}

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) < 0)
			{
				val = current;
				y = val2;
			}
		}
		return val;
	}

	public static bool TryMinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, out TSource value)
	{
		return source.TryMinBy(selector, Comparer<TKey>.Default, out value);
	}

	public static bool TryMinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, out TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			value = default(TSource);
			return false;
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) < 0)
			{
				val = current;
				y = val2;
			}
		}
		value = val;
		return true;
	}

	public static void SortBy<T, TSortBy>(this List<T> list, Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
	{
		if (list.Count > 1)
		{
			list.Sort((T a, T b) => selector(a).CompareTo(selector(b)));
		}
	}

	public static void SortBy<T, TSortBy, TThenBy>(this List<T> list, Func<T, TSortBy> selector, Func<T, TThenBy> thenBySelector) where TSortBy : IComparable<TSortBy>, IEquatable<TSortBy> where TThenBy : IComparable<TThenBy>
	{
		if (list.Count > 1)
		{
			list.Sort(delegate(T a, T b)
			{
				TSortBy val = selector(a);
				TSortBy other = selector(b);
				return (!val.Equals(other)) ? val.CompareTo(other) : thenBySelector(a).CompareTo(thenBySelector(b));
			});
		}
	}

	public static void SortBy<T, TSortBy, TThenBy, TThenBy2>(this List<T> list, Func<T, TSortBy> selector, Func<T, TThenBy> thenBySelector, Func<T, TThenBy2> thenBy2Selector) where TSortBy : IComparable<TSortBy>, IEquatable<TSortBy> where TThenBy : IComparable<TThenBy>, IEquatable<TThenBy> where TThenBy2 : IComparable<TThenBy2>
	{
		if (list.Count <= 1)
		{
			return;
		}
		list.Sort(delegate(T a, T b)
		{
			TSortBy val = selector(a);
			TSortBy other = selector(b);
			if (!val.Equals(other))
			{
				return val.CompareTo(other);
			}
			TThenBy val2 = thenBySelector(a);
			TThenBy other2 = thenBySelector(b);
			return (!val2.Equals(other2)) ? val2.CompareTo(other2) : thenBy2Selector(a).CompareTo(thenBy2Selector(b));
		});
	}

	public static void SortByColor<T>(this List<T> colorDefs, Func<T, Color> getColor)
	{
		colorDefs.SortBy(delegate(T x)
		{
			Color.RGBToHSV(getColor(x), out var H, out var S, out var _);
			return (!Mathf.Approximately(S, 0f)) ? ((float)Mathf.RoundToInt(H * 100f)) : (-1f);
		}, delegate(T x)
		{
			Color.RGBToHSV(getColor(x), out var _, out var _, out var V);
			return Mathf.RoundToInt(V * 100f);
		});
	}

	public static void SortByDescending<T, TSortByDescending>(this List<T> list, Func<T, TSortByDescending> selector) where TSortByDescending : IComparable<TSortByDescending>
	{
		if (list.Count > 1)
		{
			list.Sort((T a, T b) => selector(b).CompareTo(selector(a)));
		}
	}

	public static void SortByDescending<T, TSortByDescending, TThenByDescending>(this List<T> list, Func<T, TSortByDescending> selector, Func<T, TThenByDescending> thenByDescendingSelector) where TSortByDescending : IComparable<TSortByDescending>, IEquatable<TSortByDescending> where TThenByDescending : IComparable<TThenByDescending>
	{
		if (list.Count > 1)
		{
			list.Sort(delegate(T a, T b)
			{
				TSortByDescending other = selector(a);
				TSortByDescending other2 = selector(b);
				return (!other.Equals(other2)) ? other2.CompareTo(other) : thenByDescendingSelector(b).CompareTo(thenByDescendingSelector(a));
			});
		}
	}

	public static void SortStable<T>(this IList<T> list, Func<T, T, int> comparator)
	{
		if (list.Count <= 1)
		{
			return;
		}
		List<Pair<T, int>> list2;
		bool flag;
		if (SortStableTempList<T>.working)
		{
			list2 = new List<Pair<T, int>>();
			flag = false;
		}
		else
		{
			list2 = SortStableTempList<T>.list;
			SortStableTempList<T>.working = true;
			flag = true;
		}
		try
		{
			list2.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(new Pair<T, int>(list[i], i));
			}
			list2.Sort(delegate(Pair<T, int> lhs, Pair<T, int> rhs)
			{
				int num2 = comparator(lhs.First, rhs.First);
				return (num2 != 0) ? num2 : lhs.Second.CompareTo(rhs.Second);
			});
			list.Clear();
			for (int num = 0; num < list2.Count; num++)
			{
				list.Add(list2[num].First);
			}
			list2.Clear();
		}
		finally
		{
			if (flag)
			{
				SortStableTempList<T>.working = false;
			}
		}
	}

	public static IComparer<T> ThenBy<T>(this IComparer<T> first, IComparer<T> second)
	{
		return new ComparerChain<T>(first, second);
	}

	public static IComparer<T> Descending<T>(this IComparer<T> cmp)
	{
		return new DescendingComparer<T>(cmp);
	}

	public static IComparer<T> CompareBy<T, TComparable>(Func<T, TComparable> selector) where TComparable : IComparable<TComparable>
	{
		return Comparer<T>.Create((T a, T b) => selector(a).CompareTo(selector(b)));
	}

	public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
	{
		List<TKey> list = null;
		try
		{
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				if (predicate(item))
				{
					if (list == null)
					{
						list = SimplePool<List<TKey>>.Get();
					}
					list.Add(item.Key);
				}
			}
			if (list != null)
			{
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					dictionary.Remove(list[i]);
				}
				return list.Count;
			}
			return 0;
		}
		finally
		{
			if (list != null)
			{
				list.Clear();
				SimplePool<List<TKey>>.Return(list);
			}
		}
	}

	public static void RemoveAll<T>(this List<T> list, Func<T, int, bool> predicate)
	{
		int i = 0;
		int count;
		for (count = list.Count; i < count && !predicate(list[i], i); i++)
		{
		}
		if (i >= count)
		{
			return;
		}
		int j = i + 1;
		while (j < count)
		{
			for (; j < count && predicate(list[j], j); j++)
			{
			}
			if (j < count)
			{
				list[i++] = list[j++];
			}
		}
	}

	public static void RemoveLast<T>(this List<T> list)
	{
		list.RemoveAt(list.Count - 1);
	}

	public static T Pop<T>(this List<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	public static T PopFront<T>(this List<T> list)
	{
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	public static bool Any<T>(this List<T> list, Predicate<T> predicate)
	{
		return list.FindIndex(predicate) != -1;
	}

	public static bool Any<T>(this List<T> list)
	{
		return list.Count != 0;
	}

	public static bool Empty<T>(this List<T> list)
	{
		return list.Count == 0;
	}

	public static bool Any<T>(this HashSet<T> list)
	{
		return list.Count != 0;
	}

	public static bool Any<T>(this Stack<T> list)
	{
		return list.Count != 0;
	}

	public static void AddRange<T>(this HashSet<T> set, List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			set.Add(list[i]);
		}
	}

	public static void AddRange<T>(this HashSet<T> set, HashSet<T> other)
	{
		foreach (T item in other)
		{
			set.Add(item);
		}
	}

	public static void AddRangeNoAlloc<T>(this List<T> self, IEnumerator<T> other) where T : class
	{
		while (other.MoveNext())
		{
			self.Add(other.Current);
		}
	}

	public static void AddRangeFast<T>(this List<T> self, IEnumerable<T> other) where T : class
	{
		using IEnumerator<T> other2 = other.GetEnumerator();
		self.AddRangeNoAlloc(other2);
	}

	public static void AddRangeWhereFast<T>(this List<T> self, IEnumerable<T> other, Predicate<T> condition) where T : class
	{
		foreach (T item in other)
		{
			if (condition(item))
			{
				self.Add(item);
			}
		}
	}

	public static int Count_EnumerableBase(IEnumerable e)
	{
		if (e == null)
		{
			return 0;
		}
		if (e is ICollection collection)
		{
			return collection.Count;
		}
		int num = 0;
		foreach (object item in e)
		{
			_ = item;
			num++;
		}
		return num;
	}

	public static T FirstOrDefault<T>(this List<T> list, Predicate<T> predicate)
	{
		foreach (T item in list)
		{
			if (predicate(item))
			{
				return item;
			}
		}
		return default(T);
	}

	public static object FirstOrDefault_EnumerableBase(IEnumerable e)
	{
		if (e == null)
		{
			return null;
		}
		if (e is IList list)
		{
			if (list.Count == 0)
			{
				return null;
			}
			return list[0];
		}
		IEnumerator enumerator = e.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return null;
	}

	public static float AverageWeighted<T>(this IEnumerable<T> list, Func<T, float> weight, Func<T, float> value)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (T item in list)
		{
			float num3 = weight(item);
			num += num3;
			num2 += value(item) * num3;
		}
		return num2 / num;
	}

	public static void ExecuteEnumerable(this IEnumerable enumerable)
	{
		foreach (object item in enumerable)
		{
			_ = item;
		}
	}

	public static IEnumerable<T> OrElseEmptyEnumerable<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			return Enumerable.Empty<T>();
		}
		return enumerable;
	}

	public static bool EnumerableNullOrEmpty<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			return true;
		}
		if (enumerable is ICollection collection)
		{
			return collection.Count == 0;
		}
		return !enumerable.Any();
	}

	public static int EnumerableCount(this IEnumerable enumerable)
	{
		if (enumerable == null)
		{
			return 0;
		}
		if (enumerable is ICollection collection)
		{
			return collection.Count;
		}
		int num = 0;
		foreach (object item in enumerable)
		{
			_ = item;
			num++;
		}
		return num;
	}

	public static int Count<T>(this List<T> list, Predicate<T> predicate)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (predicate(list[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static float Percent<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		int num = 0;
		int num2 = 0;
		foreach (T item in source)
		{
			num2++;
			if (predicate(item))
			{
				num++;
			}
		}
		if (num2 == 0)
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		return (float)num / (float)num2;
	}

	public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
	{
		int num = 0;
		foreach (T item in enumerable)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static V TryGetValue<T, V>(this IReadOnlyDictionary<T, V> dict, T key, V fallback = default(V))
	{
		if (key == null || !dict.TryGetValue(key, out var value))
		{
			return fallback;
		}
		return value;
	}

	public static IReadOnlyDictionary<T, V> Slice<T, V>(this IReadOnlyDictionary<T, V> dict, IEnumerable<T> keys)
	{
		return new DictionarySlice<T, V>(dict, keys);
	}

	public static IReadOnlyDictionary<T, V> Slice<T, V>(this IReadOnlyDictionary<T, V> dict, params T[] keys)
	{
		return new DictionarySlice<T, V>(dict, keys);
	}

	public static IEnumerable<Pair<T, V>> Cross<T, V>(this IEnumerable<T> lhs, IEnumerable<V> rhs)
	{
		T[] lhsv = lhs.ToArray();
		V[] rhsv = rhs.ToArray();
		int i = 0;
		while (i < lhsv.Length)
		{
			int num;
			for (int j = 0; j < rhsv.Length; j = num)
			{
				yield return new Pair<T, V>(lhsv[i], rhsv[j]);
				num = j + 1;
			}
			num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> lhs, T rhs)
	{
		foreach (T lh in lhs)
		{
			yield return lh;
		}
		yield return rhs;
	}

	public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (predicate(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public static bool ContainsAny<T>(this IList<T> list, Func<T, bool> predicate)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (predicate(list[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<TSource> ConcatIfNotNull<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			return first;
		}
		return first.Concat(second);
	}

	public static LocalTargetInfo FirstValid(this List<LocalTargetInfo> source)
	{
		if (source == null)
		{
			return LocalTargetInfo.Invalid;
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (source[i].IsValid)
			{
				return source[i];
			}
		}
		return LocalTargetInfo.Invalid;
	}

	public static IEnumerable<T> Except<T>(this IEnumerable<T> lhs, T rhs) where T : class
	{
		foreach (T lh in lhs)
		{
			if (lh != rhs)
			{
				yield return lh;
			}
		}
	}

	public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> enumerable) where T : class
	{
		IEnumerator<T> iterator = enumerable.GetEnumerator();
		while (iterator.MoveNext())
		{
			T current = iterator.Current;
			if (!iterator.MoveNext())
			{
				yield return ValueTuple.Create<T, T>(current, null);
			}
			else
			{
				yield return ValueTuple.Create(current, iterator.Current);
			}
		}
	}

	public static bool ListsEqual<T>(List<T> a, List<T> b) where T : class
	{
		if (a == b)
		{
			return true;
		}
		if (a.NullOrEmpty() && b.NullOrEmpty())
		{
			return true;
		}
		if (a.NullOrEmpty() || b.NullOrEmpty())
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < a.Count; i++)
		{
			if (!equalityComparer.Equals(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool DictsEqual<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> a, IReadOnlyDictionary<TKey, TValue> b)
	{
		int num = a?.Count ?? 0;
		int num2 = b?.Count ?? 0;
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
		foreach (KeyValuePair<TKey, TValue> item in a)
		{
			if (!b.TryGetValue(item.Key, out var value) || !equalityComparer.Equals(item.Value, value))
			{
				return false;
			}
		}
		foreach (KeyValuePair<TKey, TValue> item2 in b)
		{
			if (!a.TryGetValue(item2.Key, out var value2) || !equalityComparer.Equals(item2.Value, value2))
			{
				return false;
			}
		}
		return true;
	}

	public static int DictHashCode<Key, Value>(IReadOnlyDictionary<Key, Value> dict)
	{
		int num = 0;
		foreach (KeyValuePair<Key, Value> item in dict ?? Enumerable.Empty<KeyValuePair<Key, Value>>())
		{
			num ^= item.GetHashCode();
		}
		return num;
	}

	public static void Deconstruct<Key, Value>(this KeyValuePair<Key, Value> tuple, out Key key, out Value value)
	{
		key = tuple.Key;
		value = tuple.Value;
	}

	public static bool SetsEqual<T>(this List<T> a, List<T> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a.NullOrEmpty() && b.NullOrEmpty())
		{
			return true;
		}
		if (a.NullOrEmpty() || b.NullOrEmpty())
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (!b.Contains(a[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < b.Count; j++)
		{
			if (!a.Contains(b[j]))
			{
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<T> TakeRandom<T>(this List<T> list, int count)
	{
		if (!list.NullOrEmpty())
		{
			int i = 0;
			while (i < count)
			{
				yield return list[Rand.Range(0, list.Count)];
				int num = i + 1;
				i = num;
			}
		}
	}

	public static List<T> TakeRandomDistinct<T>(this List<T> list, int count)
	{
		List<T> list2 = new List<T>();
		foreach (T item in list.Distinct().InRandomOrder())
		{
			list2.Add(item);
			if (list2.Count == count)
			{
				return list2;
			}
		}
		return list2;
	}

	public static List<T> TakeRandomDistinct<T>(this IEnumerable<T> list, int count)
	{
		List<T> list2 = new List<T>();
		foreach (T item in list.Distinct().InRandomOrder())
		{
			list2.Add(item);
			if (list2.Count == count)
			{
				return list2;
			}
		}
		return list2;
	}

	public static void AddDistinct<T>(this List<T> list, T obj)
	{
		list.AddUnique(obj);
	}

	public static void AddUnique<T>(this List<T> list, T obj)
	{
		if (!list.Contains(obj))
		{
			list.Add(obj);
		}
	}

	public static void AddRangeUnique<T>(this List<T> list, List<T> other)
	{
		for (int i = 0; i < other.Count; i++)
		{
			if (!list.Contains(other[i]))
			{
				list.Add(other[i]);
			}
		}
	}

	public static void AddRangeUnique<T>(this List<T> list, IEnumerable<T> other)
	{
		foreach (T item in other)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
	}

	public static int Replace<T>(this IList<T> list, T replace, T with) where T : class
	{
		if (list == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == replace)
			{
				list[i] = with;
				num++;
			}
		}
		return num;
	}

	public static Pair<K, List<E>> ConvertIGroupingToPair<K, E>(IGrouping<K, E> g)
	{
		return new Pair<K, List<E>>(g.Key, g.ToList());
	}

	public static int GetCountGreaterOrEqualInSortedList(List<int> list, int val)
	{
		int num = list.BinarySearch(val);
		if (num >= 0)
		{
			return list.Count - num;
		}
		int num2 = ~num;
		return list.Count - num2;
	}

	public static void InsertIntoSortedList<T>(this List<T> list, T val, IComparer<T> cmp)
	{
		if (list.Count == 0)
		{
			list.Add(val);
			return;
		}
		int num = list.BinarySearch(val, cmp);
		if (num >= 0)
		{
			list.Insert(num, val);
		}
		else
		{
			list.Insert(~num, val);
		}
	}

	public static void RemoveBatchUnordered<T>(this List<T> list, List<int> indices)
	{
		if (indices.Count == 0)
		{
			return;
		}
		int num = list.Count - 1;
		foreach (int index in indices)
		{
			if (num == index)
			{
				num--;
				continue;
			}
			if (num <= 0)
			{
				break;
			}
			list[index] = list[num];
			num--;
		}
		for (int num2 = list.Count - 1; num2 > num; num2--)
		{
			list.RemoveAt(num2);
		}
	}

	public static ValueType GetWithFallback<KeyType, ValueType>(this Dictionary<KeyType, ValueType> dictionary, KeyType key, ValueType fallback = default(ValueType))
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return fallback;
	}

	public static void ClearNullAndDestroyed<T>(this List<T> list) where T : Thing
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].DestroyedOrNull())
			{
				list.RemoveAt(num);
			}
		}
	}

	public static bool TryPop<T>(this Stack<T> stack, out T element)
	{
		if (stack.Count == 0)
		{
			element = default(T);
			return false;
		}
		element = stack.Pop();
		return true;
	}

	public static bool TryDequeue<T>(this Queue<T> queue, out T element)
	{
		if (queue.Count == 0)
		{
			element = default(T);
			return false;
		}
		element = queue.Dequeue();
		return true;
	}

	public static void CopyToList<T>(this IEnumerable<T> source, List<T> target, bool clear = true)
	{
		if (clear)
		{
			target.Clear();
		}
		if (source is ICollection<T> collection)
		{
			int num = ((!clear) ? target.Count : 0) + collection.Count;
			if (target.Capacity < num)
			{
				target.Capacity = num;
			}
		}
		foreach (T item in source)
		{
			target.Add(item);
		}
	}
}
