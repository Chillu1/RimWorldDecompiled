using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_PlaceGrowingZone : Lesson_Instruction
	{
		private CellRect growingZoneRect;

		private List<IntVec3> cachedCells;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref growingZoneRect, "growingZoneRect");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				RecacheCells();
			}
		}

		private void RecacheCells()
		{
			cachedCells = growingZoneRect.Cells.ToList();
		}

		public override void OnActivated()
		{
			base.OnActivated();
			growingZoneRect = TutorUtility.FindUsableRect(10, 8, base.Map, 0.5f);
			RecacheCells();
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(growingZoneRect, def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawFieldEdges(cachedCells);
			GenDraw.DrawArrowPointingAt(growingZoneRect.CenterVector3);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-ZoneAdd_Growing")
			{
				return TutorUtility.EventCellsMatchExactly(ep, cachedCells);
			}
			return base.AllowAction(ep);
		}
	}
}
