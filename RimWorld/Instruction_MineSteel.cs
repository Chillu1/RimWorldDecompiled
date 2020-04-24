using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Instruction_MineSteel : Lesson_Instruction
	{
		private List<IntVec3> mineCells;

		protected override float ProgressPercent
		{
			get
			{
				int num = 0;
				for (int i = 0; i < mineCells.Count; i++)
				{
					IntVec3 c = mineCells[i];
					if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.Mine) != null || c.GetEdifice(base.Map) == null || c.GetEdifice(base.Map).def != ThingDefOf.MineableSteel)
					{
						num++;
					}
				}
				return (float)num / (float)mineCells.Count;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref mineCells, "mineCells", LookMode.Undefined);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			CellRect cellRect = TutorUtility.FindUsableRect(10, 10, base.Map, 0f, noItems: true);
			GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
			genStep_ScatterLumpsMineable.forcedDefToScatter = ThingDefOf.MineableSteel;
			genStep_ScatterLumpsMineable.ForceScatterAt(cellRect.CenterCell, base.Map);
			mineCells = new List<IntVec3>();
			foreach (IntVec3 item in cellRect)
			{
				Building edifice = item.GetEdifice(base.Map);
				if (edifice != null && edifice.def == ThingDefOf.MineableSteel)
				{
					mineCells.Add(item);
				}
			}
		}

		public override void LessonOnGUI()
		{
			if (!mineCells.NullOrEmpty())
			{
				TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(mineCells), def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(Gen.AveragePosition(mineCells));
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Mine")
			{
				return TutorUtility.EventCellsAreWithin(ep, mineCells);
			}
			return base.AllowAction(ep);
		}

		public override void Notify_Event(EventPack ep)
		{
			if (ep.Tag == "Designate-Mine" && ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
