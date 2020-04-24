using RimWorld;
using Verse.AI;

namespace Verse
{
	public class HediffComp_ReactOnDamage : HediffComp
	{
		public HediffCompProperties_ReactOnDamage Props => (HediffCompProperties_ReactOnDamage)props;

		public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (Props.damageDefIncoming == dinfo.Def)
			{
				React();
			}
		}

		private void React()
		{
			if (Props.createHediff != null)
			{
				BodyPartRecord part = parent.Part;
				if (Props.createHediffOn != null)
				{
					part = parent.pawn.RaceProps.body.AllParts.FirstOrFallback((BodyPartRecord p) => p.def == Props.createHediffOn);
				}
				parent.pawn.health.AddHediff(Props.createHediff, part);
			}
			if (Props.vomit)
			{
				parent.pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
			}
		}
	}
}
