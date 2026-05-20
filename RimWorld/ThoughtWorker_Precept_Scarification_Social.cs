using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Scarification_Social : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			int num = 0;
			foreach (Precept item in p.Ideo.PreceptsListForReading)
			{
				num = Mathf.Max(num, item.def.requiredScars);
			}
			if (num == 0)
			{
				return false;
			}
			if (CountScars(p) < num)
			{
				return false;
			}
			int num2 = CountScars(otherPawn);
			if (num2 >= num)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (num2 == 0)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (num2 < num)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return false;
			static int CountScars(Pawn pawn)
			{
				int num3 = 0;
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff.def == HediffDefOf.Scarification)
					{
						num3++;
					}
				}
				return num3;
			}
		}
	}
}
