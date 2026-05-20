using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RemoveApparel : JobDriver
{
	private int duration;

	private const TargetIndex ApparelInd = TargetIndex.A;

	private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref duration, "duration", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		yield return Toils_General.Wait(duration).PlaySustainerOrSound(Apparel.def.apparel.soundRemove).WithProgressBarToilDelay(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			Apparel resultingAp;
			if (!pawn.apparel.WornApparel.Contains(Apparel))
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else if (!pawn.apparel.TryDrop(Apparel, out resultingAp))
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				job.targetA = resultingAp;
				if (job.haulDroppedApparel)
				{
					resultingAp.SetForbidden(value: false, warnOnFail: false);
					StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(resultingAp);
					if (StoreUtility.TryFindBestBetterStoreCellFor(resultingAp, pawn, base.Map, currentPriority, pawn.Faction, out var foundCell))
					{
						job.count = resultingAp.stackCount;
						job.targetB = foundCell;
					}
					else
					{
						EndJobWith(JobCondition.Incompletable);
					}
				}
				else
				{
					EndJobWith(JobCondition.Succeeded);
				}
			}
		});
		if (job.haulDroppedApparel)
		{
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A).FailOn(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
		}
	}
}
