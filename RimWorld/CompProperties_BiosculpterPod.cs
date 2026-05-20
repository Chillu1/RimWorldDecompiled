using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_BiosculpterPod : CompProperties
{
	public SoundDef enterSound;

	public SoundDef exitSound;

	public EffecterDef operatingEffecter;

	public EffecterDef readyEffecter;

	public Color selectCycleColor;

	public float biotunedCycleSpeedFactor;

	public CompProperties_BiosculpterPod()
	{
		compClass = typeof(CompBiosculpterPod);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (parentDef.tickerType != TickerType.Normal)
		{
			yield return GetType().Name + " requires parent ticker type Normal";
		}
	}
}
