using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasHediff : ThinkNode_Conditional
	{
		public HediffDef hediff;

		public FloatRange severityRange = FloatRange.Zero;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalHasHediff obj = (ThinkNode_ConditionalHasHediff)base.DeepCopy(resolve);
			obj.hediff = hediff;
			obj.severityRange = severityRange;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
			if (firstHediffOfDef != null)
			{
				return firstHediffOfDef.Severity >= severityRange.RandomInRange;
			}
			return false;
		}
	}
}
