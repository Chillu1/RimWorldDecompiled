using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_Scarify : JobDriver
{
	private const TargetIndex TargetPawnIndex = TargetIndex.A;

	public static readonly HealthTuning.PainCategoryWeighted[] InjuryPainCategories = new HealthTuning.PainCategoryWeighted[2]
	{
		new HealthTuning.PainCategoryWeighted(PainCategory.LowPain, 0.8f),
		new HealthTuning.PainCategoryWeighted(PainCategory.MediumPain, 0.2f)
	};

	protected Pawn Target => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public static bool AvailableOnNow(Pawn pawn, BodyPartRecord part = null)
	{
		if (!pawn.RaceProps.Humanlike || (Faction.OfPlayerSilentFail != null && !Faction.OfPlayer.ideos.AnyPreceptWithRequiredScars()))
		{
			return false;
		}
		if (part != null && (pawn.health.WouldDieAfterAddingHediff(HediffDefOf.Scarification, part, 1f) || pawn.health.WouldLosePartAfterAddingHediff(HediffDefOf.Scarification, part, 1f)))
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn)
	{
		foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
		{
			if (notMissingPart.def.canScarify)
			{
				yield return notMissingPart;
			}
		}
	}

	public static void Scarify(Pawn pawn, BodyPartRecord part)
	{
		if (ModLister.CheckIdeology("Scarification"))
		{
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob is LordJob_Ritual_Mutilation lordJob_Ritual_Mutilation)
			{
				lordJob_Ritual_Mutilation.mutilatedPawns.Add(pawn);
			}
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Scarification, pawn, part);
			HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
			hediffComp_GetsPermanent.IsPermanent = true;
			hediffComp_GetsPermanent.SetPainCategory(InjuryPainCategories.RandomElementByWeight((HealthTuning.PainCategoryWeighted e) => e.weight).category);
			pawn.health.AddHediff(hediff);
		}
	}

	public static void CreateHistoryEventDef(Pawn pawn)
	{
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.GotScarified, pawn.Named(HistoryEventArgsNames.Doer)));
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Scarify"))
		{
			yield break;
		}
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Pawn target = Target;
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
		yield return Toils_General.Wait(35);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (!(from part in GetPartsToApplyOn(target)
				where AvailableOnNow(target, part)
				select part).TryRandomElement(out var result) && !GetPartsToApplyOn(target).TryRandomElement(out result))
			{
				pawn.jobs.EndCurrentJob(JobCondition.Errored);
				Log.Error("Failed to find body part to scarify");
			}
			Scarify(target, result);
			CreateHistoryEventDef(target);
			SoundDefOf.Execute_Cut.PlayOneShot(target);
			if (target.RaceProps.BloodDef != null)
			{
				CellRect cellRect = new CellRect(target.PositionHeld.x - 1, target.PositionHeld.z - 1, 3, 3);
				for (int num = 0; num < 3; num++)
				{
					IntVec3 randomCell = cellRect.RandomCell;
					if (randomCell.InBounds(base.Map) && GenSight.LineOfSight(randomCell, target.PositionHeld, base.Map))
					{
						FilthMaker.TryMakeFilth(randomCell, target.MapHeld, target.RaceProps.BloodDef, target.LabelIndefinite());
					}
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}
}
