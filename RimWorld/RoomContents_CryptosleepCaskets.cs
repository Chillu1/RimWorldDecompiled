using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_CryptosleepCaskets : RoomContentsWorker
{
	private static readonly FloatRange LockersPer10EdgeCells = new FloatRange(1f, 3f);

	private static readonly IntRange LockerGroupSizeRange = new IntRange(2, 3);

	private static readonly IntRange CasketRange = new IntRange(2, 4);

	public virtual PodContentsType GetContentsType()
	{
		if (!Rand.Bool)
		{
			return PodContentsType.AncientFriendly;
		}
		return PodContentsType.AncientHostile;
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		int groupID = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientCryptosleepCasket, CasketRange.RandomInRange, room, map, Rot4.South, null, null, 2, null, alignWithRect: false, snapToGrid: false, SpawnAction);
		SpawnLockers(map, room);
		base.FillRoom(map, room, faction, threatPoints);
		Thing SpawnAction(IntVec3 cell, Rot4 rot, Map _)
		{
			PodContentsType contentsType = GetContentsType();
			return RoomGenUtility.SpawnCryptoCasket(cell, map, rot, groupID, contentsType, ThingSetMakerDefOf.MapGen_AncientPodContents);
		}
	}

	private static void SpawnLockers(Map map, LayoutRoom room)
	{
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(LockersPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.AncientLockerBank, count, LockerGroupSizeRange, room, map);
	}
}
