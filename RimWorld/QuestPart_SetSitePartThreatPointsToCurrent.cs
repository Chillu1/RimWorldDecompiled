using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_SetSitePartThreatPointsToCurrent : QuestPart
	{
		public string inSignal;

		public SitePartDef sitePartDef;

		public Site site;

		public MapParent useMapParentThreatPoints;

		public float threatPointsFactor = 1f;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal) || useMapParentThreatPoints == null || !useMapParentThreatPoints.HasMap || site == null || sitePartDef == null)
			{
				return;
			}
			List<SitePart> parts = site.parts;
			for (int i = 0; i < parts.Count; i++)
			{
				if (parts[i].def == sitePartDef)
				{
					parts[i].parms.threatPoints = StorytellerUtility.DefaultThreatPointsNow(useMapParentThreatPoints.Map) * threatPointsFactor;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Defs.Look(ref sitePartDef, "sitePartDef");
			Scribe_References.Look(ref site, "site");
			Scribe_References.Look(ref useMapParentThreatPoints, "useMapParentThreatPoints");
			Scribe_Values.Look(ref threatPointsFactor, "threatPointsFactor", 0f);
		}
	}
}
