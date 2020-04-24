using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildRoomDoor : Lesson_Instruction
	{
		private List<IntVec3> allowedPlaceCells;

		private CellRect RoomRect => Find.TutorialState.roomRect;

		public override void OnActivated()
		{
			base.OnActivated();
			allowedPlaceCells = RoomRect.EdgeCells.ToList();
			allowedPlaceCells.RemoveAll((IntVec3 c) => (c.x == RoomRect.minX && c.z == RoomRect.minZ) || (c.x == RoomRect.minX && c.z == RoomRect.maxZ) || (c.x == RoomRect.maxX && c.z == RoomRect.minZ) || (c.x == RoomRect.maxX && c.z == RoomRect.maxZ));
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(RoomRect, def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(RoomRect.CenterVector3);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Door")
			{
				return TutorUtility.EventCellsAreWithin(ep, allowedPlaceCells);
			}
			return base.AllowAction(ep);
		}

		public override void Notify_Event(EventPack ep)
		{
			if (ep.Tag == "Designate-Door")
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
