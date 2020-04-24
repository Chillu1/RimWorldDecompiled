using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_ChopWood : Lesson_Instruction
	{
		protected override float ProgressPercent => (float)(from d in base.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.HarvestPlant)
			where d.target.Thing.def.plant.IsTree
			select d).Count() / (float)def.targetCount;

		public override void LessonUpdate()
		{
			if (ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
