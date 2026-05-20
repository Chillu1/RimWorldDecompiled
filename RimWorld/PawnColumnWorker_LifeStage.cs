using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_LifeStage : PawnColumnWorker_Icon
	{
		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.ageTracker.CurLifeStageRace.GetIcon(pawn);
		}

		protected override string GetIconTip(Pawn pawn)
		{
			int num = Mathf.FloorToInt(pawn.ageTracker.AgeBiologicalYearsFloat);
			int num2 = Mathf.FloorToInt(pawn.ageTracker.AgeBiologicalYearsFloat * 60f);
			num2 -= num * 60;
			string text = "";
			if (num > 0)
			{
				text = ((num != 1) ? ((string)(text + "PeriodYears".Translate(num))) : ((string)(text + "Period1Year".Translate())));
				text += ", ";
			}
			if (num2 > 0)
			{
				text = ((num2 != 1) ? ((string)(text + "PeriodDays".Translate(num2))) : ((string)(text + "Period1Day".Translate())));
			}
			else if (num <= 0)
			{
				text += "PeriodHours".Translate(((float)pawn.ageTracker.AgeBiologicalTicks / 2500f).ToString("0.0"));
			}
			return pawn.ageTracker.CurLifeStage.LabelCap + " (" + text.TrimEnd(',', ' ') + ")";
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return a.ageTracker.AgeBiologicalTicks.CompareTo(b.ageTracker.AgeBiologicalTicks);
		}
	}
}
