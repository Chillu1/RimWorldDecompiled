using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Fish : JobDriver
{
	private const TargetIndex SpotInd = TargetIndex.A;

	private const TargetIndex StandInd = TargetIndex.B;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed))
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (ModsConfig.OdysseyActive)
		{
			this.FailOn(() => !(job.GetTarget(TargetIndex.A).Cell.GetZone(pawn.Map) is Zone_Fishing zone_Fishing) || !zone_Fishing.Allowed);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			int ticks = Mathf.RoundToInt(7500f / pawn.GetStatValue(StatDefOf.FishingSpeed));
			Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true, maintainSleep: false, TargetIndex.A);
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				pawn.skills?.Learn(SkillDefOf.Animals, 0.025f);
				pawn.GainComfortFromCellIfPossible(1, chairsOnly: true);
			});
			toil.WithEffect(EffecterDefOf.Fishing, () => job.GetTarget(TargetIndex.A));
			toil.WithProgressBarToilDelay(TargetIndex.B);
			yield return toil;
			yield return CompleteFishingToil();
		}
	}

	private Toil CompleteFishingToil()
	{
		Toil toil = ToilMaker.MakeToil("CompleteFishingToil");
		toil.initAction = delegate
		{
			IntVec3 cell = job.GetTarget(TargetIndex.A).Cell;
			List<NegativeFishingOutcomeDef> negativeFishingOutcomes = FishingUtility.GetNegativeFishingOutcomes(pawn, cell);
			bool rare;
			List<Thing> catchesFor = FishingUtility.GetCatchesFor(pawn, cell, animalFishing: false, out rare);
			if (negativeFishingOutcomes.Any())
			{
				NegativeFishingOutcomeDef negativeFishingOutcomeDef = negativeFishingOutcomes.RandomElement();
				pawn.Map.waterBodyTracker.lastNegativeCatchTick = Find.TickManager.TicksGame;
				if (negativeFishingOutcomeDef.damageDef != null)
				{
					DamageInfo dinfo = new DamageInfo(negativeFishingOutcomeDef.damageDef, negativeFishingOutcomeDef.damageAmountRange.RandomInRange);
					dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
					pawn.TakeDamage(dinfo);
				}
				if (negativeFishingOutcomeDef.addsHediff != null)
				{
					Hediff hediff = pawn.health.AddHediff(negativeFishingOutcomeDef.addsHediff);
					if (negativeFishingOutcomeDef.hediffSeverity > 0f)
					{
						hediff.Severity = negativeFishingOutcomeDef.hediffSeverity;
					}
				}
				Find.LetterStack.ReceiveLetter(negativeFishingOutcomeDef.letterLabel, negativeFishingOutcomeDef.letterText.Formatted(pawn.Named("PAWN")), negativeFishingOutcomeDef.letterDef, pawn);
			}
			else if (catchesFor.Any())
			{
				bool flag = false;
				int num = catchesFor.Sum((Thing x) => x.stackCount);
				foreach (Thing item in catchesFor)
				{
					flag |= GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
				}
				if (flag)
				{
					if (rare)
					{
						pawn.Map.waterBodyTracker.lastRareCatchTick = Find.TickManager.TicksGame;
						Find.LetterStack.ReceiveLetter("LetterLabelRareCatch".Translate(), "LetterTextRareCatch".Translate(pawn.Named("PAWN")) + ":\n" + catchesFor.Select((Thing x) => x.LabelCap).ToLineList("  - "), LetterDefOf.PositiveEvent, catchesFor);
					}
					else
					{
						pawn.Map.waterBodyTracker.Notify_Fished(cell, num);
						Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SlaughteredFish, pawn.Named(HistoryEventArgsNames.Doer)));
					}
				}
			}
			(cell.GetZone(pawn.Map) as Zone_Fishing)?.Notify_Fished();
		};
		toil.PlaySoundAtStart(SoundDefOf.Interact_CatchFish);
		return toil;
	}
}
