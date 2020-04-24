using System.Collections.Generic;

namespace Verse
{
	public class HediffGiver_AddSeverity : HediffGiver
	{
		public float severityAmount = float.NaN;

		public float mtbHours = -1f;

		private static int mtbCheckInterval = 1200;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (pawn.IsNestedHashIntervalTick(60, mtbCheckInterval) && Rand.MTBEventOccurs(mtbHours, 2500f, mtbCheckInterval))
			{
				if (TryApply(pawn))
				{
					SendLetter(pawn, cause);
				}
				pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityAmount;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (float.IsNaN(severityAmount))
			{
				yield return "severityAmount is not defined";
			}
			if (mtbHours < 0f)
			{
				yield return "mtbHours is not defined";
			}
		}
	}
}
