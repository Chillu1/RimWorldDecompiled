using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Verse;

public class DefMap<D, V> : IExposable, IEnumerable<KeyValuePair<D, V>>, IEnumerable where D : Def, new() where V : new()
{
	private List<V> values;

	public int Count => values.Count;

	public V this[D def]
	{
		get
		{
			return values[def.index];
		}
		set
		{
			values[def.index] = value;
		}
	}

	public V this[int index]
	{
		get
		{
			return values[index];
		}
		set
		{
			values[index] = value;
		}
	}

	public DefMap()
	{
		int defCount = DefDatabase<D>.DefCount;
		if (defCount == 0)
		{
			throw new Exception("Constructed DefMap<" + typeof(D)?.ToString() + ", " + typeof(V)?.ToString() + "> without defs being initialized. Try constructing it in ResolveReferences instead of the constructor.");
		}
		values = new List<V>(defCount);
		for (int i = 0; i < defCount; i++)
		{
			values.Add(new V());
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref values, "vals", LookMode.Undefined);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			int defCount = DefDatabase<D>.DefCount;
			for (int i = values.Count; i < defCount; i++)
			{
				values.Add(new V());
			}
			while (values.Count > defCount)
			{
				values.RemoveLast();
			}
		}
	}

	public void SetAll(V val)
	{
		for (int i = 0; i < values.Count; i++)
		{
			values[i] = val;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<D, V>> GetEnumerator()
	{
		return DefDatabase<D>.AllDefsListForReading.Select((D d) => new KeyValuePair<D, V>(d, this[d])).GetEnumerator();
	}
}
