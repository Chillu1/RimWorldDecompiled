using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_BestowingCeremony : JobDriver
	{
		public const float WarmupSoundLength = 5.125f;

		public const int BestowTimeTicks = 471;

		public const int PlayWarmupSoundAfterTicks = 307;

		protected const TargetIndex BestowerInd = TargetIndex.A;

		protected const TargetIndex BestowSpotInd = TargetIndex.B;

		private Mote mote;

		private Sustainer sound;

		private Pawn Bestower => base.TargetA.Pawn;

		private LordJob_BestowingCeremony CeremonyJob => (LordJob_BestowingCeremony)Bestower.GetLord().LordJob;

		private LocalTargetInfo BestowSpot => job.targetB;

		public static bool AnalyzeThroneRoom(Pawn bestower, Pawn target)
		{
			RoyalTitleDef titleAwardedWhenUpdating = target.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, target.royalty.GetFavor(bestower.Faction));
			if (titleAwardedWhenUpdating != null && titleAwardedWhenUpdating.throneRoomRequirements != null)
			{
				foreach (RoomRequirement throneRoomRequirement in titleAwardedWhenUpdating.throneRoomRequirements)
				{
					if (!throneRoomRequirement.Met(bestower.GetRoom(), target))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(BestowSpot, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(Bestower, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Bestowing cermonies are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 5464564);
				yield break;
			}
			AddFailCondition(() => Bestower.GetLord() == null || Bestower.GetLord().CurLordToil == null || !(Bestower.GetLord().CurLordToil is LordToil_BestowingCeremony_Wait));
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil waitToil = Toils_General.Wait(471);
			waitToil.AddPreInitAction(delegate
			{
				Messages.Message("MessageBestowingCeremonyStarted".Translate(pawn.Named("PAWN")), Bestower, MessageTypeDefOf.PositiveEvent);
			});
			waitToil.AddPreInitAction(delegate
			{
				if (!AnalyzeThroneRoom(Bestower, pawn))
				{
					Messages.Message("BestowingCeremonyThroneroomRequirementsNotSatisfied".Translate(pawn.Named("PAWN"), pawn.royalty.GetTitleAwardedWhenUpdating(Bestower.Faction, pawn.royalty.GetFavor(Bestower.Faction)).label.Named("TITLE")), pawn, MessageTypeDefOf.NegativeEvent);
					((LordJob_BestowingCeremony)Bestower.GetLord().LordJob).MakeCeremonyFail();
				}
			});
			waitToil.AddPreInitAction(delegate
			{
				SoundDefOf.Bestowing_Start.PlayOneShot(pawn);
			});
			waitToil.tickAction = delegate
			{
				pawn.rotationTracker.FaceTarget(Bestower);
				if (mote == null || mote.Destroyed)
				{
					Vector3 loc = (pawn.TrueCenter() + Bestower.TrueCenter()) / 2f;
					mote = MoteMaker.MakeStaticMote(loc, pawn.Map, ThingDefOf.Mote_Bestow);
				}
				mote.Maintain();
				if ((sound == null || sound.Ended) && waitToil.actor.jobs.curDriver.ticksLeftThisToil <= 307)
				{
					sound = SoundDefOf.Bestowing_Warmup.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
				}
				if (sound != null)
				{
					sound.Maintain();
				}
			};
			waitToil.handlingFacing = false;
			waitToil.socialMode = RandomSocialMode.Off;
			waitToil.WithProgressBarToilDelay(TargetIndex.A);
			yield return waitToil;
			yield return Toils_General.Do(delegate
			{
				CeremonyJob.FinishCeremony(pawn);
				MoteMaker.MakeStaticMote((pawn.TrueCenter() + Bestower.TrueCenter()) / 2f, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, 2f);
				SoundDefOf.Bestowing_Finished.PlayOneShot(pawn);
			});
		}
	}
}
