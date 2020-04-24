using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanMaker
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		public static Caravan MakeCaravan(IEnumerable<Pawn> pawns, Faction faction, int startingTile, bool addToWorldPawnsIfNotAlready)
		{
			if (startingTile < 0 && addToWorldPawnsIfNotAlready)
			{
				Log.Warning("Tried to create a caravan but chose not to spawn a caravan but pass pawns to world. This can cause bugs because pawns can be discarded.");
			}
			tmpPawns.Clear();
			tmpPawns.AddRange(pawns);
			Caravan caravan = (Caravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);
			if (startingTile >= 0)
			{
				caravan.Tile = startingTile;
			}
			caravan.SetFaction(faction);
			if (startingTile >= 0)
			{
				Find.WorldObjects.Add(caravan);
			}
			for (int i = 0; i < tmpPawns.Count; i++)
			{
				Pawn pawn = tmpPawns[i];
				if (pawn.Dead)
				{
					Log.Warning("Tried to form a caravan with a dead pawn " + pawn);
					continue;
				}
				caravan.AddPawn(pawn, addToWorldPawnsIfNotAlready);
				if (addToWorldPawnsIfNotAlready && !pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
			caravan.Name = CaravanNameGenerator.GenerateCaravanName(caravan);
			tmpPawns.Clear();
			caravan.SetUniqueId(Find.UniqueIDsManager.GetNextCaravanID());
			return caravan;
		}
	}
}
