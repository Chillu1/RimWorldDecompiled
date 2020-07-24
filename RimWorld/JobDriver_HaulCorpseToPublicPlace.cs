using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_HaulCorpseToPublicPlace : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex GraveInd = TargetIndex.B;

		private const TargetIndex CellInd = TargetIndex.C;

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private Building_Grave Grave => (Building_Grave)job.GetTarget(TargetIndex.B).Thing;

		private bool InGrave => Grave != null;

		private Thing Target => (Thing)(((object)Grave) ?? ((object)Corpse));

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
		}

		public override string GetReport()
		{
			if (InGrave && Grave.def == ThingDefOf.Grave)
			{
				return "ReportDiggingUpCorpse".Translate();
			}
			return base.GetReport();
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Jump.JumpIfTargetInvalid(TargetIndex.B, gotoCorpse);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.B);
			yield return Toils_General.Wait(300).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B)
				.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
			yield return Toils_General.Open(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return gotoCorpse;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return FindCellToDropCorpseToil();
			yield return Toils_Reserve.Reserve(TargetIndex.C);
			yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.Touch);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: false);
			yield return ForbidAndNotifyMentalStateToil();
		}

		private Toil FindCellToDropCorpseToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					IntVec3 result = IntVec3.Invalid;
					if (!Rand.Chance(0.8f) || !TryFindTableCell(out result))
					{
						bool flag = false;
						if (RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out IntVec3 result2) && CellFinder.TryRandomClosewalkCellNear(result2, pawn.Map, 5, out result, (IntVec3 x) => pawn.CanReserve(x) && x.GetFirstItem(pawn.Map) == null))
						{
							flag = true;
						}
						if (!flag)
						{
							result = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, 10, (IntVec3 x) => pawn.CanReserve(x) && x.GetFirstItem(pawn.Map) == null);
						}
					}
					job.SetTarget(TargetIndex.C, result);
				},
				atomicWithPrevious = true
			};
		}

		private Toil ForbidAndNotifyMentalStateToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					Corpse?.SetForbidden(value: true);
					(pawn.MentalState as MentalState_CorpseObsession)?.Notify_CorpseHauled();
				},
				atomicWithPrevious = true
			};
		}

		private bool TryFindTableCell(out IntVec3 cell)
		{
			tmpCells.Clear();
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if (!building.def.IsTable)
				{
					continue;
				}
				foreach (IntVec3 item in building.OccupiedRect())
				{
					if (pawn.CanReserveAndReach(item, PathEndMode.OnCell, Danger.Deadly) && item.GetFirstItem(pawn.Map) == null)
					{
						tmpCells.Add(item);
					}
				}
			}
			if (!tmpCells.Any())
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = tmpCells.RandomElement();
			return true;
		}
	}
}
