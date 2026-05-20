using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_MonumentMarkerMissingBlueprints : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<GlobalTargetInfo> Targets
	{
		get
		{
			targets.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.MonumentMarker);
				for (int j = 0; j < list.Count; j++)
				{
					MonumentMarker monumentMarker = (MonumentMarker)list[j];
					if (!monumentMarker.complete)
					{
						SketchEntity firstEntityWithMissingBlueprint = monumentMarker.FirstEntityWithMissingBlueprint;
						if (firstEntityWithMissingBlueprint != null)
						{
							targets.Add(new GlobalTargetInfo(firstEntityWithMissingBlueprint.pos + monumentMarker.Position, maps[i]));
						}
					}
				}
			}
			return targets;
		}
	}

	public Alert_MonumentMarkerMissingBlueprints()
	{
		defaultLabel = "MonumentMarkerMissingBlueprints".Translate();
		defaultExplanation = "MonumentMarkerMissingBlueprintsDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}
}
