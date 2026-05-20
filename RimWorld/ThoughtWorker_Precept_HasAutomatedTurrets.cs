using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_HasAutomatedTurrets : ThoughtWorker_Precept
{
	private static List<ThingDef> automatedTurretDefs = new List<ThingDef>();

	public static void ResetStaticData()
	{
		automatedTurretDefs.Clear();
		List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (allDefsListForReading[i].building != null && allDefsListForReading[i].building.IsTurret && !allDefsListForReading[i].HasComp(typeof(CompMannable)))
			{
				automatedTurretDefs.Add(allDefsListForReading[i]);
			}
		}
	}

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (p.Faction == null || p.IsSlave)
		{
			return false;
		}
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			for (int j = 0; j < automatedTurretDefs.Count; j++)
			{
				List<Thing> list = maps[i].listerThings.ThingsOfDef(automatedTurretDefs[j]);
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].Faction == p.Faction)
					{
						return true;
					}
				}
			}
			if (!ModsConfig.BiotechActive)
			{
				continue;
			}
			foreach (Pawn item in maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (item.IsColonyMechPlayerControlled)
				{
					return true;
				}
			}
		}
		return false;
	}
}
