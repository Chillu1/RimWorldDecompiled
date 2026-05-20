using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_Blind : JobDriver
{
	private const TargetIndex TargetPawnIndex = TargetIndex.A;

	protected Pawn Target => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public static void Blind(Pawn pawn, Pawn doer)
	{
		Lord lord = pawn.GetLord();
		IEnumerable<BodyPartRecord> enumerable = from p in pawn.health.hediffSet.GetNotMissingParts()
			where p.def == BodyPartDefOf.Eye
			select p;
		if (lord != null && lord.LordJob is LordJob_Ritual_Mutilation lordJob_Ritual_Mutilation && enumerable.Count() == 1)
		{
			lordJob_Ritual_Mutilation.mutilatedPawns.Add(pawn);
		}
		foreach (BodyPartRecord item in enumerable)
		{
			if (item.def == BodyPartDefOf.Eye)
			{
				pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, item));
				break;
			}
		}
		if (pawn.Dead)
		{
			ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, doer, PawnExecutionKind.GenericBrutal);
		}
	}

	public static void CreateHistoryEventDef(Pawn pawn)
	{
		if (PawnUtility.IsBiologicallyBlind(pawn))
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.GotBlinded, pawn.Named(HistoryEventArgsNames.Doer)));
		}
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
		Toil scarify = ToilMaker.MakeToil("MakeNewToils");
		scarify.initAction = delegate
		{
			Blind(target, pawn);
			CreateHistoryEventDef(target);
			SoundDefOf.Execute_Cut.PlayOneShot(target);
			if (target.RaceProps.BloodDef != null)
			{
				CellRect cellRect = new CellRect(target.PositionHeld.x - 1, target.PositionHeld.z - 1, 3, 3);
				for (int i = 0; i < 3; i++)
				{
					IntVec3 randomCell = cellRect.RandomCell;
					if (randomCell.InBounds(base.Map) && GenSight.LineOfSight(randomCell, target.PositionHeld, base.Map))
					{
						FilthMaker.TryMakeFilth(randomCell, target.MapHeld, target.RaceProps.BloodDef, target.LabelIndefinite());
					}
				}
			}
		};
		scarify.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return Toils_General.Wait(120);
		yield return scarify;
	}
}
