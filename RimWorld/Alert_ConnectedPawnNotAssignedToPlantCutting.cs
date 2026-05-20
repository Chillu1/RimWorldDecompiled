using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_ConnectedPawnNotAssignedToPlantCutting : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_ConnectedPawnNotAssignedToPlantCutting()
	{
		defaultLabel = "AlertConnectedPawnNotAssignedToPlantCuttingLabel".Translate();
		requireIdeology = true;
	}

	public override AlertReport GetReport()
	{
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}

	private void GetTargets()
	{
		targets.Clear();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (!maps[i].IsPlayerHome)
			{
				continue;
			}
			List<Thing> list = maps[i].listerThings.ThingsInGroup(ThingRequestGroup.DryadSpawner);
			for (int j = 0; j < list.Count; j++)
			{
				CompTreeConnection compTreeConnection = list[j].TryGetComp<CompTreeConnection>();
				if (compTreeConnection != null && compTreeConnection.Connected && compTreeConnection.DesiredConnectionStrength > 0f && compTreeConnection.ConnectedPawn.workSettings.GetPriority(WorkTypeDefOf.PlantCutting) == 0)
				{
					targets.Add(compTreeConnection.ConnectedPawn);
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertConnectedPawnNotAssignedToPlantCuttingDesc".Translate() + ":\n" + targets.Select((GlobalTargetInfo x) => ((Pawn)(Thing)x).NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
	}
}
