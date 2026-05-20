using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_ManTurret : JobDriver
{
	private const float SearchRadius = 40f;

	private const int MaxPawnReservations = 10;

	private const TargetIndex TurretInd = TargetIndex.A;

	private const TargetIndex HaulingInd = TargetIndex.B;

	private Building_TurretGun Turret => (Building_TurretGun)job.GetTarget(TargetIndex.A).Thing;

	private Thing Hauling => job.GetTarget(TargetIndex.B).Thing;

	private static bool GunNeedsLoading(Building b)
	{
		if (!(b is Building_TurretGun building_TurretGun))
		{
			return false;
		}
		CompChangeableProjectile compChangeableProjectile = building_TurretGun.gun.TryGetComp<CompChangeableProjectile>();
		if (compChangeableProjectile == null || compChangeableProjectile.Loaded)
		{
			return false;
		}
		return true;
	}

	private static bool GunNeedsRefueling(Building b)
	{
		if (!(b is Building_TurretGun thing))
		{
			return false;
		}
		CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
		if (compRefuelable == null || compRefuelable.HasFuel || !compRefuelable.Props.fuelIsMortarBarrel || Find.Storyteller.difficulty.classicMortars)
		{
			return false;
		}
		return true;
	}

	public static Thing FindAmmoForTurret(Pawn pawn, Building_TurretGun gun)
	{
		StorageSettings allowedShellsSettings = ((pawn.IsColonist || pawn.IsColonyMech) ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null);
		return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, ShellValidator);
		bool ShellValidator(Thing t)
		{
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(t, 10, 1))
			{
				return false;
			}
			if (allowedShellsSettings != null && !allowedShellsSettings.AllowedToAccept(t))
			{
				return false;
			}
			if (pawn.Faction != Faction.OfPlayer && t.def.projectileWhenLoaded?.projectile != null && !t.def.projectileWhenLoaded.projectile.damageDef.harmsHealth)
			{
				return false;
			}
			return true;
		}
	}

	public static Thing FindFuelForTurret(Pawn pawn, Building_TurretGun gun)
	{
		CompRefuelable refuelableComp = gun.TryGetComp<CompRefuelable>();
		if (refuelableComp == null)
		{
			return null;
		}
		return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, FuelValidator);
		bool FuelValidator(Thing t)
		{
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 10, 1))
			{
				return false;
			}
			return refuelableComp.Props.fuelFilter.Allows(t);
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		Toil gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil refuelIfNeeded = ToilMaker.MakeToil("MakeNewToils");
		refuelIfNeeded.initAction = delegate
		{
			Pawn actor = refuelIfNeeded.actor;
			Building building = (Building)actor.CurJob.targetA.Thing;
			Building_TurretGun building_TurretGun = building as Building_TurretGun;
			if (!GunNeedsRefueling(building))
			{
				JumpToToil(gotoTurret);
			}
			else
			{
				Thing thing = FindFuelForTurret(pawn, building_TurretGun);
				if (thing == null)
				{
					CompRefuelable compRefuelable = building.TryGetComp<CompRefuelable>();
					if (actor.Faction == Faction.OfPlayer && compRefuelable != null)
					{
						Messages.Message("MessageOutOfNearbyFuelFor".Translate(actor.LabelShort, building_TurretGun.Label, actor.Named("PAWN"), building_TurretGun.Named("GUN"), compRefuelable.Props.fuelFilter.Summary.Named("FUEL")).CapitalizeFirst(), building_TurretGun, MessageTypeDefOf.NegativeEvent);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				actor.CurJob.targetB = thing;
				actor.CurJob.count = 1;
			}
		};
		yield return refuelIfNeeded;
		yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
			.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
			.WithProgressBarToilDelay(TargetIndex.A);
		yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
		Toil loadIfNeeded = ToilMaker.MakeToil("MakeNewToils");
		loadIfNeeded.initAction = delegate
		{
			Pawn actor = loadIfNeeded.actor;
			Building obj = (Building)actor.CurJob.targetA.Thing;
			Building_TurretGun building_TurretGun = obj as Building_TurretGun;
			if (!GunNeedsLoading(obj))
			{
				JumpToToil(gotoTurret);
			}
			else
			{
				Thing thing = FindAmmoForTurret(pawn, building_TurretGun);
				if (thing == null)
				{
					if (actor.Faction == Faction.OfPlayer)
					{
						Messages.Message("MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun.Label, actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(), building_TurretGun, MessageTypeDefOf.NegativeEvent);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				actor.CurJob.targetB = thing;
				actor.CurJob.count = 1;
			}
		};
		yield return loadIfNeeded;
		yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		Toil loadShell = ToilMaker.MakeToil("MakeNewToils");
		loadShell.initAction = delegate
		{
			Pawn actor = loadShell.actor;
			SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(Turret.Position, Turret.Map));
			Turret.gun.TryGetComp<CompChangeableProjectile>().LoadShell(Hauling.def, 1);
			actor.carryTracker.innerContainer.ClearAndDestroyContents();
		};
		yield return loadShell;
		yield return gotoTurret;
		Toil man = ToilMaker.MakeToil("MakeNewToils");
		man.tickAction = delegate
		{
			Pawn actor = man.actor;
			Building building = (Building)actor.CurJob.targetA.Thing;
			if (GunNeedsLoading(building))
			{
				JumpToToil(loadIfNeeded);
			}
			else if (GunNeedsRefueling(building))
			{
				JumpToToil(refuelIfNeeded);
			}
			else
			{
				building.GetComp<CompMannable>().ManForATick(actor);
				man.actor.rotationTracker.FaceCell(building.Position);
			}
		};
		man.handlingFacing = true;
		man.defaultCompleteMode = ToilCompleteMode.Never;
		man.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		yield return man;
	}
}
