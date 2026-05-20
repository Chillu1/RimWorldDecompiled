using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedRoomSize : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.roomsize == null)
		{
			return ThoughtState.Inactive;
		}
		if (!p.Awake())
		{
			return ThoughtState.Inactive;
		}
		Room room = p.GetRoom();
		if (room == null || room.PsychologicallyOutdoors)
		{
			return ThoughtState.Inactive;
		}
		RoomSizeCategory curCategory = p.needs.roomsize.CurCategory;
		if (p.Ideo != null && (int)curCategory < 2 && p.needs.PrefersIndoors)
		{
			return ThoughtState.Inactive;
		}
		return curCategory switch
		{
			RoomSizeCategory.VeryCramped => ThoughtState.ActiveAtStage(0), 
			RoomSizeCategory.Cramped => ThoughtState.ActiveAtStage(1), 
			RoomSizeCategory.Normal => ThoughtState.Inactive, 
			RoomSizeCategory.Spacious => ThoughtState.ActiveAtStage(2), 
			_ => throw new InvalidOperationException("Unknown RoomSizeCategory"), 
		};
	}
}
