using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Hibernatable : CompProperties
{
	public float startupDays = 14f;

	public IncidentTargetTagDef incidentTargetWhileStarting;

	public SoundDef sustainerActive;

	public CompProperties_Hibernatable()
	{
		compClass = typeof(CompHibernatable);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (parentDef.tickerType != TickerType.Normal)
		{
			yield return "CompHibernatable needs tickerType " + TickerType.Normal.ToString() + ", has " + parentDef.tickerType;
		}
	}
}
