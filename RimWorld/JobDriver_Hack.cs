using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_Hack : JobDriver
{
	private Thing HackTarget => base.TargetThingA;

	private CompHackable CompHacking => HackTarget.TryGetComp<CompHackable>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(HackTarget, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => CompHacking.Props.intellectualSkillPrerequisite > 0 && pawn.skills.GetSkill(SkillDefOf.Intellectual).Level < CompHacking.Props.intellectualSkillPrerequisite);
		PathEndMode pathEndMode = (base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch);
		yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.handlingFacing = true;
		toil.tickAction = delegate
		{
			float statValue = pawn.GetStatValue(StatDefOf.HackingSpeed);
			CompHacking.Hack(statValue, pawn);
			pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f);
			pawn.rotationTracker.FaceTarget(HackTarget);
		};
		toil.WithEffect(EffecterDefOf.Hacking, TargetIndex.A);
		if (CompHacking.Props.effectHacking != null)
		{
			toil.WithEffect(() => CompHacking.Props.effectHacking, () => HackTarget.OccupiedRect().ClosestCellTo(pawn.Position));
		}
		toil.WithProgressBar(TargetIndex.A, () => CompHacking.ProgressPercent, interpolateBetweenActorAndTarget: false, -0.5f, alwaysShow: true);
		toil.PlaySoundAtStart(SoundDefOf.Hacking_Started);
		toil.PlaySustainerOrSound(SoundDefOf.Hacking_InProgress);
		toil.AddFinishAction(delegate
		{
			if (CompHacking.IsHacked)
			{
				SoundDefOf.Hacking_Completed.PlayOneShot(HackTarget);
				CompHacking.Props.hackingCompletedSound?.PlayOneShot(HackTarget);
			}
			else
			{
				SoundDefOf.Hacking_Suspended.PlayOneShot(HackTarget);
			}
		});
		toil.FailOnCannotTouch(TargetIndex.A, pathEndMode);
		toil.FailOn(() => CompHacking.IsHacked || CompHacking.LockedOut);
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.activeSkill = () => SkillDefOf.Intellectual;
		yield return toil;
	}
}
