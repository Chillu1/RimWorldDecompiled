using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_PlaceStockpile : Lesson_Instruction
	{
		private CellRect stockpileRect;

		private List<IntVec3> cachedCells;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref stockpileRect, "stockpileRect");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				RecacheCells();
			}
		}

		private void RecacheCells()
		{
			cachedCells = stockpileRect.Cells.ToList();
		}

		public override void OnActivated()
		{
			base.OnActivated();
			stockpileRect = TutorUtility.FindUsableRect(6, 6, base.Map);
			RecacheCells();
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(stockpileRect, def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawFieldEdges(cachedCells);
			GenDraw.DrawArrowPointingAt(stockpileRect.CenterVector3);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-ZoneAddStockpile_Resources")
			{
				return TutorUtility.EventCellsMatchExactly(ep, cachedCells);
			}
			return base.AllowAction(ep);
		}
	}
}
