using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RemoveMechlink : JobDriver
{
	private const TargetIndex CorpseInd = TargetIndex.A;

	public const int RemoveTicks = 300;

	protected Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Corpse, job, 1, -1, null, errorOnFailed);
	}

	public override string GetReport()
	{
		return "ReportRemovingMechlink".Translate(HediffDefOf.MechlinkImplant.label);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckBiotech("Remove mechlink"))
		{
			yield break;
		}
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOn(() => !Corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		Toil toil = Toils_General.Wait(300, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).WithEffect(() => EffecterDefOf.Surgery, TargetIndex.A)
			.PlaySustainerOrSound(SoundDefOf.Recipe_Surgery)
			.PlaySoundAtEnd(SoundDefOf.Mechlink_Removed);
		toil.handlingFacing = true;
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			Pawn innerPawn = Corpse.InnerPawn;
			List<Hediff> hediffs = innerPawn.health.hediffSet.hediffs;
			for (int num = hediffs.Count - 1; num >= 0; num--)
			{
				if (hediffs[num].def == HediffDefOf.MechlinkImplant)
				{
					innerPawn.health.RemoveHediff(hediffs[num]);
				}
			}
			Mechlink obj = (Mechlink)ThingMaker.MakeThing(ThingDefOf.Mechlink);
			obj.sentMechsToPlayer = innerPawn.IsColonist || innerPawn.IsPrisoner || innerPawn.IsSlave || innerPawn.IsQuestLodger();
			GenPlace.TryPlaceThing(obj, Corpse.Position, base.Map, ThingPlaceMode.Near);
		};
		toil2.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil2;
	}
}
