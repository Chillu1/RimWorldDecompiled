using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedRoomSize : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.roomsize == null)
			{
				return ThoughtState.Inactive;
			}
			Room room = p.GetRoom();
			if (room == null || room.PsychologicallyOutdoors)
			{
				return ThoughtState.Inactive;
			}
			return p.needs.roomsize.CurCategory switch
			{
				RoomSizeCategory.VeryCramped => ThoughtState.ActiveAtStage(0), 
				RoomSizeCategory.Cramped => ThoughtState.ActiveAtStage(1), 
				RoomSizeCategory.Normal => ThoughtState.Inactive, 
				RoomSizeCategory.Spacious => ThoughtState.ActiveAtStage(2), 
				_ => throw new InvalidOperationException("Unknown RoomSizeCategory"), 
			};
		}
	}
}
