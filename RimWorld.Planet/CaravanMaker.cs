using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class CaravanMaker
{
	private static readonly List<Pawn> tmpPawns = new List<Pawn>();

	public static Caravan MakeCaravan(IEnumerable<Pawn> pawns, Faction faction, PlanetTile startingTile, bool addToWorldPawnsIfNotAlready)
	{
		if (!startingTile.Valid && addToWorldPawnsIfNotAlready)
		{
			Log.Warning("Tried to create a caravan but chose not to spawn a caravan but pass pawns to world. This can cause bugs because pawns can be discarded.");
		}
		if (!startingTile.LayerDef.canFormCaravans)
		{
			Log.Warning("Tried to create a caravan on a tile which belongs to a layer which cannot form caravans.");
		}
		tmpPawns.Clear();
		tmpPawns.AddRange(pawns);
		Caravan caravan = (Caravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);
		if (startingTile.Valid)
		{
			caravan.Tile = startingTile;
		}
		caravan.SetFaction(faction);
		if (startingTile.Valid)
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
			if (!caravan.ContainsPawn(pawn))
			{
				caravan.AddPawn(pawn, addToWorldPawnsIfNotAlready);
			}
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
