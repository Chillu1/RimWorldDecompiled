using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ExtractSkull : JobDriver
{
	public const int ExtractionTimeTicks = 180;

	private const TargetIndex CorpseInd = TargetIndex.A;

	protected Corpse Corpse => (Corpse)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => Corpse.Destroyed || !Corpse.Spawned);
		this.FailOn(() => base.Map.designationManager.DesignationOn(base.TargetA.Thing, DesignationDefOf.ExtractSkull) == null);
		yield return Toils_Reserve.Reserve(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = Toils_General.Wait(180);
		toil.WithProgressBarToilDelay(TargetIndex.A);
		toil.PlaySustainerOrSound(SoundDefOf.Recipe_Surgery);
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (Corpse.CurRotDrawMode != RotDrawMode.Dessicated && Rand.Chance(1f / 60f * (float)delta))
			{
				IntVec3 randomCell = new CellRect(Corpse.PositionHeld.x - 1, Corpse.PositionHeld.z - 1, 3, 3).RandomCell;
				if (randomCell.InBounds(base.Map) && GenSight.LineOfSight(randomCell, Corpse.PositionHeld, base.Map))
				{
					FilthMaker.TryMakeFilth(randomCell, Corpse.MapHeld, Corpse.InnerPawn.RaceProps.BloodDef, Corpse.InnerPawn.LabelIndefinite());
				}
			}
		});
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			Corpse.InnerPawn.health.AddHediff(HediffDefOf.MissingBodyPart, Corpse.InnerPawn.health.hediffSet.GetNotMissingParts().First((BodyPartRecord p) => p.def == BodyPartDefOf.Head));
			Corpse.Map.designationManager.RemoveDesignation(Corpse.Map.designationManager.DesignationOn(Corpse, DesignationDefOf.ExtractSkull));
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Skull);
			thing.TryGetComp<CompHasSources>()?.AddSource(Corpse.InnerPawn.LabelShort);
			GenPlace.TryPlaceThing(thing, Corpse.PositionHeld, Corpse.Map, ThingPlaceMode.Near);
		});
	}
}
