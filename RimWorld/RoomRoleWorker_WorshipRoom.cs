using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_WorshipRoom : RoomRoleWorker
{
	private const int MinScore = 2000;

	public override string PostProcessedLabel(string baseLabel, Room room)
	{
		Ideo ideo = DominatingIdeo(room);
		if (ideo == null || ideo.WorshipRoomLabel.NullOrEmpty())
		{
			return base.PostProcessedLabel(baseLabel, room);
		}
		return ideo.WorshipRoomLabel;
	}

	public override float GetScore(Room room)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return -1f;
		}
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		int num = 0;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i].def.isAltar && ((containedAndAdjacentThings[i] as ThingWithComps)?.compStyleable)?.SourcePrecept?.ideo?.StructureMeme != null)
			{
				num++;
			}
		}
		return (num != 0) ? Mathf.Max(2000, num * 75) : 0;
	}

	private Ideo DominatingIdeo(Room room)
	{
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i].def.isAltar)
			{
				CompStyleable compStyleable = (containedAndAdjacentThings[i] as ThingWithComps)?.compStyleable;
				if (compStyleable?.SourcePrecept?.ideo?.StructureMeme != null)
				{
					return compStyleable.SourcePrecept.ideo;
				}
			}
		}
		return null;
	}
}
