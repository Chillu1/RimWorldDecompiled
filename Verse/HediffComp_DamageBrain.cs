using RimWorld;

namespace Verse
{
	public class HediffComp_DamageBrain : HediffComp
	{
		public HediffCompProperties_DamageBrain Props => (HediffCompProperties_DamageBrain)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Props.mtbDaysPerStage[parent.CurStageIndex] > 0f && base.Pawn.IsHashIntervalTick(60) && Rand.MTBEventOccurs(Props.mtbDaysPerStage[parent.CurStageIndex], 60000f, 60f))
			{
				BodyPartRecord brain = base.Pawn.health.hediffSet.GetBrain();
				if (brain != null)
				{
					int randomInRange = Props.damageAmount.RandomInRange;
					base.Pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, randomInRange, 0f, -1f, null, brain));
					Messages.Message("MessageReceivedBrainDamageFromHediff".Translate(base.Pawn.Named("PAWN"), randomInRange, parent.Label), base.Pawn, MessageTypeDefOf.NegativeEvent);
				}
			}
		}
	}
}
