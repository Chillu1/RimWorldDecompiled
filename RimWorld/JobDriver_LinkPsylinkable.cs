using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_LinkPsylinkable : JobDriver
{
	public const int LinkTimeTicks = 15000;

	public const int EffectsTickInterval = 720;

	protected const TargetIndex PsylinkableInd = TargetIndex.A;

	protected const TargetIndex LinkSpotInd = TargetIndex.B;

	private Thing PsylinkableThing => base.TargetA.Thing;

	private CompPsylinkable Psylinkable => PsylinkableThing.TryGetComp<CompPsylinkable>();

	private LocalTargetInfo LinkSpot => job.targetB;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(PsylinkableThing, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(LinkSpot, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckRoyalty("Psylinkable"))
		{
			yield break;
		}
		AddFailCondition(() => !Psylinkable.CanPsylink(pawn, LinkSpot).Accepted);
		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
		Toil toil = Toils_General.Wait(15000);
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(PsylinkableThing);
			if (pawn.IsHashIntervalTick(720, delta))
			{
				Vector3 vector = pawn.TrueCenter();
				vector += (PsylinkableThing.TrueCenter() - vector) * Rand.Value;
				FleckMaker.Static(vector, pawn.Map, FleckDefOf.PsycastAreaEffect, 0.5f);
				Psylinkable.Props.linkSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(PsylinkableThing)));
			}
		};
		toil.handlingFacing = false;
		toil.socialMode = RandomSocialMode.Off;
		yield return toil;
	}
}
