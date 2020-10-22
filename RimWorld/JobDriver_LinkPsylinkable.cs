using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
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
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Psylinkables are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 5464564);
				yield break;
			}
			AddFailCondition(() => !Psylinkable.CanPsylink(pawn, LinkSpot).Accepted);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil toil = Toils_General.Wait(15000);
			toil.tickAction = delegate
			{
				pawn.rotationTracker.FaceTarget(PsylinkableThing);
				if (Find.TickManager.TicksGame % 720 == 0)
				{
					Vector3 vector = pawn.TrueCenter();
					vector += (PsylinkableThing.TrueCenter() - vector) * Rand.Value;
					MoteMaker.MakeStaticMote(vector, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, 0.5f);
					Psylinkable.Props.linkSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(PsylinkableThing)));
				}
			};
			toil.handlingFacing = false;
			toil.socialMode = RandomSocialMode.Off;
			toil.WithProgressBarToilDelay(TargetIndex.A);
			yield return toil;
			yield return Toils_General.Do(delegate
			{
				Psylinkable.FinishLinkingRitual(pawn);
			});
		}
	}
}
