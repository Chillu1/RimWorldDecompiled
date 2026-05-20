using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class Alert_Analyzable : Alert
{
	protected List<Thing> targets = new List<Thing>();

	protected abstract ThingDef Def { get; }

	public override AlertReport GetReport()
	{
		if (Def == null)
		{
			return AlertReport.Inactive;
		}
		targets.Clear();
		foreach (Map map in Find.Maps)
		{
			targets.AddRange(map.listerThings.ThingsOfDef(Def));
		}
		return AlertReport.CulpritsAre(targets);
	}
}
