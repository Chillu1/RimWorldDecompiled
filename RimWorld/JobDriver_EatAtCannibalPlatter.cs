using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_EatAtCannibalPlatter : JobDriver, IEatingDriver
{
	private Toil eating;

	private const TargetIndex PlatterIndex = TargetIndex.A;

	private const TargetIndex CellIndex = TargetIndex.B;

	private const int BloodFilthIntervalTick = 40;

	private const float ChanceToProduceBloodFilth = 0.25f;

	public bool GainingNutritionNow => base.CurToil == eating;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.ReserveSittableOrSpot(job.targetB.Cell, job, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Cannibal eat job"))
		{
			yield break;
		}
		this.EndOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
		float totalBuildingNutrition = base.TargetA.Thing.def.CostList.Sum((ThingDefCountClass x) => x.thingDef.GetStatValueAbstract(StatDefOf.Nutrition) * (float)x.count);
		eating = ToilMaker.MakeToil("MakeNewToils");
		eating.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
			pawn.GainComfortFromCellIfPossible(delta);
			if (pawn.needs.food != null)
			{
				pawn.needs.food.CurLevel += totalBuildingNutrition / (float)pawn.GetLord().ownedPawns.Count / (float)eating.defaultDuration * (float)delta;
			}
			if (pawn.IsHashIntervalTick(40, delta) && Rand.Value < 0.25f)
			{
				IntVec3 c = (Rand.Bool ? pawn.Position : pawn.RandomAdjacentCellCardinal());
				if (c.InBounds(pawn.Map))
				{
					FilthMaker.TryMakeFilth(c, pawn.Map, ThingDefOf.Human.race.BloodDef);
				}
			}
		};
		eating.AddFinishAction(delegate
		{
			if (pawn.mindState != null)
			{
				pawn.mindState.lastHumanMeatIngestedTick = Find.TickManager.TicksGame;
			}
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeat, pawn.Named(HistoryEventArgsNames.Doer)));
		});
		eating.WithEffect(EffecterDefOf.EatMeat, TargetIndex.A);
		eating.PlaySustainerOrSound(SoundDefOf.RawMeat_Eat);
		eating.handlingFacing = true;
		eating.defaultCompleteMode = ToilCompleteMode.Delay;
		eating.defaultDuration = (job.doUntilGatheringEnded ? job.expiryInterval : job.def.joyDuration);
		yield return eating;
	}
}
