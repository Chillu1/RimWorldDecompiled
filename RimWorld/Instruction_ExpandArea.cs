using Verse;

namespace RimWorld
{
	public abstract class Instruction_ExpandArea : Lesson_Instruction
	{
		private int startingAreaCount = -1;

		protected abstract Area MyArea
		{
			get;
		}

		protected override float ProgressPercent => (float)(MyArea.TrueCount - startingAreaCount) / (float)def.targetCount;

		public override void OnActivated()
		{
			base.OnActivated();
			startingAreaCount = MyArea.TrueCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startingAreaCount, "startingAreaCount", 0);
		}

		public override void LessonUpdate()
		{
			if (ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
