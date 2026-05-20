using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalTotalDamage : ThinkNode_Conditional
	{
		public float thresholdPercent = 0.25f;

		protected override bool Satisfied(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			float num = 0f;
			for (int i = 0; i < hediffSet.hediffs.Count; i++)
			{
				if (hediffSet.hediffs[i] is Hediff_Injury)
				{
					num += hediffSet.hediffs[i].Severity;
				}
			}
			return num / pawn.health.LethalDamageThreshold > thresholdPercent;
		}
	}
}
