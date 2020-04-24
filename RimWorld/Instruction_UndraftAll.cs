using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_UndraftAll : Lesson_Instruction
	{
		protected override float ProgressPercent => 1f - (float)DraftedPawns().Count() / (float)base.Map.mapPawns.FreeColonistsSpawnedCount;

		private IEnumerable<Pawn> DraftedPawns()
		{
			return base.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p.Drafted);
		}

		public override void LessonUpdate()
		{
			foreach (Pawn item in DraftedPawns())
			{
				GenDraw.DrawArrowPointingAt(item.DrawPos);
			}
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
