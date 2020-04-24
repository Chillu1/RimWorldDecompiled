using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedFood : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.food == null)
			{
				return ThoughtState.Inactive;
			}
			switch (p.needs.food.CurCategory)
			{
			case HungerCategory.Fed:
				return ThoughtState.Inactive;
			case HungerCategory.Hungry:
				return ThoughtState.ActiveAtStage(0);
			case HungerCategory.UrgentlyHungry:
				return ThoughtState.ActiveAtStage(1);
			case HungerCategory.Starving:
			{
				int num = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition)?.CurStageIndex ?? 0;
				return ThoughtState.ActiveAtStage(2 + num);
			}
			default:
				throw new NotImplementedException();
			}
		}
	}
}
