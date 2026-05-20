using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_Rottable : CompProperties
{
	public float daysToRotStart = 2f;

	public bool rotDestroys;

	public float rotDamagePerDay = 40f;

	public float daysToDessicated = 999f;

	public float dessicatedDamagePerDay;

	public bool disableIfHatcher;

	public int TicksToRotStart => Mathf.RoundToInt(daysToRotStart * 60000f);

	public int TicksToDessicated => Mathf.RoundToInt(daysToDessicated * 60000f);

	public CompProperties_Rottable()
	{
		compClass = typeof(CompRottable);
	}

	public CompProperties_Rottable(float daysToRotStart)
	{
		this.daysToRotStart = daysToRotStart;
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (parentDef.tickerType != TickerType.Normal && parentDef.tickerType != TickerType.Rare)
		{
			yield return "CompRottable needs tickerType " + TickerType.Rare.ToString() + " or " + TickerType.Normal.ToString() + ", has " + parentDef.tickerType;
		}
	}
}
