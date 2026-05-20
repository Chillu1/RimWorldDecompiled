using System;
using Verse;

namespace LudeonTK;

public class TableDataGetter<T>
{
	public string label;

	public Func<T, string> getter;

	public TableDataGetter(string label, Func<T, string> getter)
	{
		this.label = label;
		this.getter = getter;
	}

	public TableDataGetter(string label, Func<T, float> getter)
	{
		this.label = label;
		this.getter = (T t) => getter(t).ToString("0.#");
	}

	public TableDataGetter(string label, Func<T, int> getter)
	{
		this.label = label;
		this.getter = (T t) => getter(t).ToString("F0");
	}

	public TableDataGetter(string label, Func<T, ThingDef> getter)
	{
		this.label = label;
		this.getter = delegate(T t)
		{
			ThingDef thingDef = getter(t);
			return (thingDef == null) ? "" : thingDef.defName;
		};
	}

	public TableDataGetter(string label, Func<T, object> getter)
	{
		this.label = label;
		this.getter = (T t) => getter(t).ToString();
	}
}
