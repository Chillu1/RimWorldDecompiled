using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_InstallImplant : JobDriver
{
	private const TargetIndex TargetPawnInd = TargetIndex.A;

	private const TargetIndex ItemInd = TargetIndex.B;

	private const int DurationTicks = 600;

	private Pawn TargetPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	private Thing Item => job.GetTarget(TargetIndex.B).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(TargetPawn, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, 600, useProgressBar: false, maintainPosture: true, maintainSleep: false, TargetIndex.A);
		toil.WithProgressBarToilDelay(TargetIndex.A);
		toil.FailOnDespawnedOrNull(TargetIndex.A);
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		yield return toil;
		yield return Toils_General.Do(Install);
	}

	private void Install()
	{
		CompTargetEffect_InstallImplantInOtherPawn compTargetEffect_InstallImplantInOtherPawn = Item.TryGetComp<CompTargetEffect_InstallImplantInOtherPawn>();
		if (compTargetEffect_InstallImplantInOtherPawn == null)
		{
			return;
		}
		BodyPartRecord bodyPartRecord = TargetPawn.RaceProps.body.GetPartsWithDef(compTargetEffect_InstallImplantInOtherPawn.Props.bodyPart).FirstOrFallback();
		if (bodyPartRecord != null)
		{
			Hediff firstHediffOfDef = TargetPawn.health.hediffSet.GetFirstHediffOfDef(compTargetEffect_InstallImplantInOtherPawn.Props.hediffDef);
			if (firstHediffOfDef == null && !compTargetEffect_InstallImplantInOtherPawn.Props.requiresExistingHediff)
			{
				TargetPawn.health.AddHediff(compTargetEffect_InstallImplantInOtherPawn.Props.hediffDef, bodyPartRecord);
			}
			else if (compTargetEffect_InstallImplantInOtherPawn.Props.canUpgrade)
			{
				((Hediff_Level)firstHediffOfDef).ChangeLevel(1);
			}
			if (TargetPawn.Map == Find.CurrentMap && compTargetEffect_InstallImplantInOtherPawn.Props.soundOnUsed != null)
			{
				compTargetEffect_InstallImplantInOtherPawn.Props.soundOnUsed.PlayOneShot(SoundInfo.InMap(TargetPawn));
			}
			Item.SplitOff(1).Destroy();
		}
	}
}
