using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class JobDriver_GiveSpeech : JobDriver
	{
		private const TargetIndex ThroneIndex = TargetIndex.A;

		private const TargetIndex FacingIndex = TargetIndex.B;

		private static readonly IntRange MoteInterval = new IntRange(300, 500);

		public static readonly Texture2D moteIcon = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Speech");

		private Building_Throne Throne => (Building_Throne)base.TargetThingA;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Throne, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				job.SetTarget(TargetIndex.B, Throne.InteractionCell + Throne.Rotation.FacingCell);
			});
			Toil toil = new Toil();
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			toil.FailOn(() => Throne.AssignedPawn != pawn);
			toil.FailOn(() => RoomRoleWorker_ThroneRoom.Validate(Throne.GetRoom()) != null);
			toil.tickAction = delegate
			{
				pawn.GainComfortFromCellIfPossible();
				pawn.skills.Learn(SkillDefOf.Social, 0.3f);
				if (pawn.IsHashIntervalTick(MoteInterval.RandomInRange))
				{
					MoteMaker.MakeSpeechBubble(pawn, moteIcon);
				}
				rotateToFace = TargetIndex.B;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			yield return toil;
		}
	}
}
