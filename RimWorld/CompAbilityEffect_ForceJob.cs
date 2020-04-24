using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompAbilityEffect_ForceJob : CompAbilityEffect_WithDest
	{
		public new CompProperties_AbilityForceJob Props => (CompProperties_AbilityForceJob)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Pawn pawn = target.Thing as Pawn;
			if (pawn != null)
			{
				Job job = JobMaker.MakeJob(Props.jobDef, new LocalTargetInfo(GetDestination(target).Cell));
				float num = 1f;
				if (Props.durationMultiplier != null)
				{
					num = pawn.GetStatValue(Props.durationMultiplier);
				}
				job.expiryInterval = (parent.def.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 10f) * num).SecondsToTicks();
				job.mote = MoteMaker.MakeThoughtBubble(pawn, parent.def.iconPath, maintain: true);
				pawn.jobs.StopAll();
				pawn.jobs.StartJob(job, JobCondition.InterruptForced);
			}
		}
	}
}
