using Verse;

namespace RimWorld;

public class RoomContents_Orbital_Corridor : RoomContents_Checkpoint_Corridor
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		int count = room.rects.Count;
		RoomGenUtility.SpawnWallAttatchments(ThingDefOf.LifeSupportUnit, map, room, IntRange.Between(count, count));
	}

	protected override bool IsValidWallAttachmentCell(LayoutWallAttatchmentParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		if (parms.def != ThingDefOf.LifeSupportUnit)
		{
			return base.IsValidWallAttachmentCell(parms, cell, rot, room, map);
		}
		return false;
	}
}
