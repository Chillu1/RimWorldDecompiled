using Verse;

namespace RimWorld
{
	public abstract class Instruction_BuildAtRoom : Lesson_Instruction
	{
		protected abstract CellRect BuildableRect
		{
			get;
		}

		protected override float ProgressPercent
		{
			get
			{
				if (def.targetCount <= 1)
				{
					return -1f;
				}
				return (float)NumPlaced() / (float)def.targetCount;
			}
		}

		protected int NumPlaced()
		{
			int num = 0;
			foreach (IntVec3 item in BuildableRect)
			{
				if (TutorUtility.BuildingOrBlueprintOrFrameCenterExists(item, base.Map, def.thingDef))
				{
					num++;
				}
			}
			return num;
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(BuildableRect.ContractedBy(1), def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(BuildableRect.CenterVector3, offscreenOnly: true);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-" + def.thingDef.defName)
			{
				return AllowBuildAt(ep.Cell);
			}
			return base.AllowAction(ep);
		}

		protected virtual bool AllowBuildAt(IntVec3 c)
		{
			return BuildableRect.Contains(c);
		}

		public override void Notify_Event(EventPack ep)
		{
			if (NumPlaced() >= def.targetCount)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
