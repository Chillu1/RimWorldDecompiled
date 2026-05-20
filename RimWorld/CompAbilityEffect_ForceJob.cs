using Verse;
using Verse.AI;

namespace RimWorld;

public class CompAbilityEffect_ForceJob : CompAbilityEffect_WithDest
{
	public new CompProperties_AbilityForceJob Props => (CompProperties_AbilityForceJob)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		if (target.Thing is Pawn pawn)
		{
			Job job = JobMaker.MakeJob(Props.jobDef, new LocalTargetInfo(GetDestination(target).Cell));
			float num = 1f;
			if (Props.durationMultiplier != null)
			{
				num = pawn.GetStatValue(Props.durationMultiplier);
			}
			job.expiryInterval = (parent.def.GetStatValueAbstract(StatDefOf.Ability_Duration, parent.pawn) * num).SecondsToTicks();
			job.mote = MoteMaker.MakeThoughtBubble(pawn, parent.def.iconPath, maintain: true);
			RestUtility.WakeUp(pawn);
			pawn.jobs.StopAll();
			pawn.jobs.StartJob(job, JobCondition.InterruptForced);
		}
	}
}
