using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ForceTargetWear : JobDriver
{
	private int duration;

	private int unequipBuffer;

	private const TargetIndex PawnInd = TargetIndex.A;

	private const TargetIndex ApparelInd = TargetIndex.B;

	private const TargetIndex ApparelSourceIndex = TargetIndex.C;

	private Pawn TargetPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.B).Thing;

	private bool TargetIsOnApparelSource
	{
		get
		{
			Apparel apparel = Apparel;
			if (apparel != null && !apparel.Spawned && apparel.ParentHolder is IApparelSource apparelSource)
			{
				return apparelSource is Thing;
			}
			return false;
		}
	}

	private IApparelSource ApparelSource => (IApparelSource)job.GetTarget(TargetIndex.C).Thing;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		job.count = 1;
		if (pawn == TargetPawn)
		{
			Log.Error($"Pawn {pawn} tried to do ForceTargetWear with self as target; this should not happen.");
			return false;
		}
		if (!pawn.Reserve(TargetPawn, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (TargetIsOnApparelSource)
		{
			return pawn.Reserve((Thing)Apparel.ParentHolder, job, 1, -1, null, errorOnFailed);
		}
		return pawn.Reserve(Apparel, job, 1, -1, null, errorOnFailed);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (TargetIsOnApparelSource)
		{
			job.targetC = (Thing)Apparel.ParentHolder;
		}
		duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = TargetPawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, TargetPawn.RaceProps.body))
			{
				duration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
			}
		}
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnBurningImmobile(TargetIndex.B);
		this.FailOnAggroMentalState(TargetIndex.A);
		bool usingApparelSource = TargetIsOnApparelSource;
		if (usingApparelSource)
		{
			yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.C);
		}
		else
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
		}
		if (usingApparelSource)
		{
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.initAction = delegate
			{
				IApparelSource apparelSource = (IApparelSource)Apparel.ParentHolder;
				if (apparelSource == null)
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else if (!apparelSource.RemoveApparel(Apparel))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else if (!pawn.carryTracker.TryStartCarry(Apparel))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					job.SetTarget(TargetIndex.B, Apparel);
					pawn.records.Increment(RecordDefOf.ThingsHauled);
				}
			};
			yield return toil;
		}
		else
		{
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, storageMode: true);
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			PawnUtility.ForceWait(TargetPawn, duration + 360, null, maintainPosture: true, maintainSleep: true);
		};
		toil2.tickIntervalAction = delegate(int delta)
		{
			unequipBuffer += delta;
			TryUnequipSomething();
			pawn.rotationTracker.FaceTarget(TargetPawn);
		};
		toil2.WithProgressBarToilDelay(TargetIndex.B);
		toil2.FailOnDespawnedNullOrForbidden(TargetIndex.B);
		toil2.defaultCompleteMode = ToilCompleteMode.Delay;
		toil2.defaultDuration = duration;
		toil2.handlingFacing = true;
		toil2.PlaySustainerOrSound(GetCurrentWearSound);
		yield return toil2;
		Toil toil3 = Toils_General.Do(delegate
		{
			Apparel apparel = Apparel;
			TargetPawn.apparel.Wear(apparel);
			if (TargetPawn.outfits != null && job.playerForced)
			{
				TargetPawn.outfits.forcedHandler.SetForced(apparel, forced: true);
			}
		});
		toil3.AddFinishAction(delegate
		{
			Pawn targetPawn = TargetPawn;
			if (targetPawn != null && targetPawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
			{
				targetPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			targetPawn?.Faction?.Notify_MemberStripped(targetPawn, Faction.OfPlayer);
		});
		yield return toil3;
	}

	private SoundDef GetCurrentWearSound()
	{
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = TargetPawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, TargetPawn.RaceProps.body))
			{
				if (unequipBuffer >= (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f))
				{
					break;
				}
				return wornApparel[num].def.apparel.soundRemove;
			}
		}
		return apparel.def.apparel.soundWear;
	}

	private void TryUnequipSomething()
	{
		Apparel apparel = Apparel;
		List<Apparel> wornApparel = TargetPawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, TargetPawn.RaceProps.body))
			{
				int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
				if (unequipBuffer >= num2)
				{
					bool forbid = TargetPawn.Faction != null && TargetPawn.Faction.HostileTo(Faction.OfPlayer);
					if (!TargetPawn.apparel.TryDrop(wornApparel[num], out var _, TargetPawn.PositionHeld, forbid))
					{
						Log.Error(TargetPawn?.ToString() + " could not drop " + wornApparel[num].ToStringSafe());
						EndJobWith(JobCondition.Errored);
					}
				}
				break;
			}
		}
	}
}
