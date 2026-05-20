using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PutApparelOnOutfitStand : JobDriver
{
	private const TargetIndex ApparelInd = TargetIndex.A;

	private const TargetIndex OutfitStandInd = TargetIndex.B;

	private const TargetIndex ApparelSourceIndex = TargetIndex.C;

	private Apparel Apparel => job.GetTarget(TargetIndex.A).Thing as Apparel;

	private Building_OutfitStand DestOutfitStand => job.GetTarget(TargetIndex.B).Thing as Building_OutfitStand;

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

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (TargetIsOnApparelSource)
		{
			job.targetC = (Thing)Apparel.ParentHolder;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (TargetIsOnApparelSource)
		{
			if (pawn.Reserve((Thing)Apparel.ParentHolder, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(DestOutfitStand, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}
		if (pawn.Reserve(Apparel, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(DestOutfitStand, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnBurningImmobile(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.B);
		bool usingSource = TargetIsOnApparelSource;
		if (usingSource)
		{
			yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.C);
		}
		else
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		}
		if (usingSource)
		{
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				ApparelSource.RemoveApparel(Apparel);
				pawn.carryTracker.TryStartCarry(Apparel);
			};
			yield return toil;
		}
		else
		{
			job.count = 1;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		}
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.B);
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			if (pawn.carryTracker.CarriedThing is Apparel apparel)
			{
				if (!DestOutfitStand.HasRoomForApparelOfDef(apparel.def) && !DestOutfitStand.TryDropThingsToMakeRoomForThingOfDef(apparel.def))
				{
					EndJobWith(JobCondition.Errored);
				}
				else
				{
					pawn.carryTracker.innerContainer.Remove(apparel);
					DestOutfitStand.AddApparel(apparel);
				}
			}
		};
		yield return toil2;
	}
}
