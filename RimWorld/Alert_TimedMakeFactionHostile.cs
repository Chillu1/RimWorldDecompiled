using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_TimedMakeFactionHostile : Alert
{
	private List<GlobalTargetInfo> worldObjectsResult = new List<GlobalTargetInfo>();

	private StringBuilder sb = new StringBuilder();

	private List<GlobalTargetInfo> WorldObjects
	{
		get
		{
			worldObjectsResult.Clear();
			foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
			{
				TimedMakeFactionHostile component = allWorldObject.GetComponent<TimedMakeFactionHostile>();
				if (component != null && component.TicksLeft.HasValue)
				{
					worldObjectsResult.Add(allWorldObject);
				}
			}
			return worldObjectsResult;
		}
	}

	public Alert_TimedMakeFactionHostile()
	{
		defaultLabel = "FactionWillBecomeHostileIfNotLeavingWithin".Translate();
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (GlobalTargetInfo item in worldObjectsResult)
		{
			sb.Append("  - ");
			sb.Append(item.Label);
			sb.Append(" (");
			sb.Append(item.WorldObject.GetComponent<TimedMakeFactionHostile>().TicksLeft.Value.ToStringTicksToPeriodVerbose());
			sb.AppendLine(")");
		}
		return "FactionWillBecomeHostileIfNotLeavingWithinDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		List<GlobalTargetInfo> worldObjects = WorldObjects;
		Map currentMap = Find.CurrentMap;
		List<Pawn> culprits;
		if (!WorldRendererUtility.WorldSelected && currentMap != null && worldObjects.Contains(currentMap.Parent) && !(culprits = currentMap.mapPawns.FreeHumanlikesSpawnedOfFaction(currentMap.ParentFaction)).NullOrEmpty())
		{
			return AlertReport.CulpritsAre(culprits);
		}
		if (worldObjects.Count > 0)
		{
			return AlertReport.CulpritsAre(worldObjects);
		}
		return false;
	}
}
