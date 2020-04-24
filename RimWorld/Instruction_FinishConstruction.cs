using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_FinishConstruction : Lesson_Instruction
	{
		private int initialBlueprintsCount = -1;

		protected override float ProgressPercent
		{
			get
			{
				if (initialBlueprintsCount < 0)
				{
					initialBlueprintsCount = ConstructionNeeders().Count();
				}
				if (initialBlueprintsCount == 0)
				{
					return 1f;
				}
				return 1f - (float)ConstructionNeeders().Count() / (float)initialBlueprintsCount;
			}
		}

		private IEnumerable<Thing> ConstructionNeeders()
		{
			return from b in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).Concat(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame))
				where b.Faction == Faction.OfPlayer
				select b;
		}

		public override void LessonUpdate()
		{
			base.LessonUpdate();
			if (ConstructionNeeders().Count() < 3)
			{
				foreach (Thing item in ConstructionNeeders())
				{
					GenDraw.DrawArrowPointingAt(item.DrawPos);
				}
			}
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
