using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildRoomWalls : Lesson_Instruction
	{
		private List<IntVec3> cachedEdgeCells = new List<IntVec3>();

		private CellRect RoomRect
		{
			get
			{
				return Find.TutorialState.roomRect;
			}
			set
			{
				Find.TutorialState.roomRect = value;
			}
		}

		protected override float ProgressPercent
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 edgeCell in RoomRect.EdgeCells)
				{
					if (TutorUtility.BuildingOrBlueprintOrFrameCenterExists(edgeCell, base.Map, ThingDefOf.Wall))
					{
						num2++;
					}
					num++;
				}
				return (float)num2 / (float)num;
			}
		}

		public override void OnActivated()
		{
			base.OnActivated();
			RoomRect = TutorUtility.FindUsableRect(12, 8, base.Map);
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(RoomRect, def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			cachedEdgeCells.Clear();
			cachedEdgeCells.AddRange(RoomRect.EdgeCells.Where((IntVec3 c) => !TutorUtility.BuildingOrBlueprintOrFrameCenterExists(c, base.Map, ThingDefOf.Wall)).ToList());
			GenDraw.DrawFieldEdges(cachedEdgeCells.Where((IntVec3 c) => c.GetEdifice(base.Map) == null).ToList());
			GenDraw.DrawArrowPointingAt(RoomRect.CenterVector3);
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Wall")
			{
				return TutorUtility.EventCellsAreWithin(ep, cachedEdgeCells);
			}
			return base.AllowAction(ep);
		}
	}
}
