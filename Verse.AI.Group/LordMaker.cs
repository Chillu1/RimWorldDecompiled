using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group
{
	public static class LordMaker
	{
		public static Lord MakeNewLord(Faction faction, LordJob lordJob, Map map, IEnumerable<Pawn> startingPawns = null)
		{
			if (map == null)
			{
				Log.Warning("Tried to create a lord with null map.");
				return null;
			}
			Lord lord = new Lord();
			lord.loadID = Find.UniqueIDsManager.GetNextLordID();
			lord.faction = faction;
			map.lordManager.AddLord(lord);
			lord.SetJob(lordJob);
			lord.GotoToil(lord.Graph.StartingToil);
			if (startingPawns != null)
			{
				foreach (Pawn startingPawn in startingPawns)
				{
					lord.AddPawn(startingPawn);
				}
				return lord;
			}
			return lord;
		}
	}
}
