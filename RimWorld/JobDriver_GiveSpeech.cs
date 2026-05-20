using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

[StaticConstructorOnStartup]
public class JobDriver_GiveSpeech : JobDriver
{
	private int ticksTillSocialInteraction = 60;

	private int ticksTillFacingUpdate;

	private IntVec3 facingCellCached = IntVec3.Invalid;

	private const TargetIndex StandIndex = TargetIndex.A;

	private const TargetIndex FacingIndex = TargetIndex.B;

	private static readonly int SocialInteractionInterval = 480;

	private static readonly int FacingUpdateInterval = 20;

	public static readonly Texture2D moteIcon = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Speech");

	private Building_Throne Throne => (Building_Throne)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (Throne != null)
		{
			return pawn.Reserve(Throne, job, 1, -1, null, errorOnFailed);
		}
		return pawn.Reserve(base.TargetLocA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		yield return Toils_General.Do(delegate
		{
			LocalTargetInfo pack = ((Throne != null) ? ((LocalTargetInfo)(Throne.InteractionCell + Throne.Rotation.FacingCell)) : base.TargetB);
			job.SetTarget(TargetIndex.B, pack);
		});
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		if (Throne != null)
		{
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			toil.FailOn(() => Throne.AssignedPawn != pawn);
			toil.FailOn(() => RoomRoleWorker_ThroneRoom.Validate(Throne.GetRoom()) != null);
		}
		else
		{
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.OnCell);
		}
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.GainComfortFromCellIfPossible(delta);
			pawn.skills.Learn(SkillDefOf.Social, 0.3f * (float)delta);
			if (ticksTillSocialInteraction <= 0)
			{
				if (job.showSpeechBubbles)
				{
					MoteMaker.MakeSpeechBubble(pawn, moteIcon);
				}
				if (job.interaction != null && pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
				{
					SocialInteractionUtility.ImitateSocialInteractionWithManyPawns(pawn, lordJob_Ritual.lord.ownedPawns, job.interaction);
				}
				ticksTillSocialInteraction = SocialInteractionInterval;
			}
			if (ticksTillFacingUpdate == 0)
			{
				IntVec3 intVec = RitualUtility.RitualCrowdCenterFor(pawn);
				if (intVec.IsValid)
				{
					facingCellCached = intVec;
				}
				ticksTillFacingUpdate = FacingUpdateInterval;
			}
			if (facingCellCached.IsValid)
			{
				pawn.rotationTracker.FaceTarget(facingCellCached);
			}
			else
			{
				pawn.rotationTracker.FaceTarget(job.GetTarget(TargetIndex.B));
			}
			rotateToFace = TargetIndex.B;
			ticksTillSocialInteraction -= delta;
			ticksTillFacingUpdate -= delta;
		};
		if (ModsConfig.IdeologyActive)
		{
			toil.PlaySustainerOrSound(() => (pawn.gender != Gender.Female) ? job.speechSoundMale : job.speechSoundFemale, pawn.story.VoicePitchFactor);
		}
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.handlingFacing = true;
		yield return toil;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksTillSocialInteraction, "ticksTillSocialInteraction", 0);
		Scribe_Values.Look(ref ticksTillFacingUpdate, "ticksTillFacingUpdate", 0);
		Scribe_Values.Look(ref facingCellCached, "facingCellCached");
	}
}
