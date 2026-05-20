using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRequirement_ThingCount : RoomRequirement_Thing
{
	public int count;

	public override bool Met(Room r, Pawn p = null)
	{
		return Count(r) >= count;
	}

	public int Count(Room r)
	{
		return r.ThingCount(thingDef);
	}

	public override string Label(Room r = null)
	{
		bool flag = !labelKey.NullOrEmpty();
		string text = (flag ? ((string)labelKey.Translate()) : thingDef.label);
		if (r != null)
		{
			return text + " " + Count(r) + "/" + count;
		}
		if (!flag)
		{
			return GenLabel.ThingLabel(thingDef, null, count);
		}
		return text + " x" + count;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (count <= 0)
		{
			yield return "count must be larger than 0";
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref count, "count", 0);
	}
}
