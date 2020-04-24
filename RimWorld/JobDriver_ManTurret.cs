using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_ManTurret : JobDriver
	{
		private const float ShellSearchRadius = 40f;

		private const int MaxPawnAmmoReservations = 10;

		private static bool GunNeedsLoading(Building b)
		{
			Building_TurretGun building_TurretGun = b as Building_TurretGun;
			if (building_TurretGun == null)
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

		public static Thing FindAmmoForTurret(Pawn pawn, Building_TurretGun gun)
		{
			StorageSettings allowedShellsSettings = pawn.IsColonist ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null;
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (t.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(t, 10, 1))
				{
					return false;
				}
				return (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t)) ? true : false;
			};
			return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, validator);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil loadIfNeeded = new Toil();
			loadIfNeeded.initAction = delegate
			{
				Pawn actor3 = loadIfNeeded.actor;
				Building obj = (Building)actor3.CurJob.targetA.Thing;
				Building_TurretGun building_TurretGun2 = obj as Building_TurretGun;
				if (!GunNeedsLoading(obj))
				{
					JumpToToil(gotoTurret);
				}
				else
				{
					Thing thing = FindAmmoForTurret(pawn, building_TurretGun2);
					if (thing == null)
					{
						if (actor3.Faction == Faction.OfPlayer)
						{
							Messages.Message("MessageOutOfNearbyShellsFor".Translate(actor3.LabelShort, building_TurretGun2.Label, actor3.Named("PAWN"), building_TurretGun2.Named("GUN")).CapitalizeFirst(), building_TurretGun2, MessageTypeDefOf.NegativeEvent);
						}
						actor3.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					actor3.CurJob.targetB = thing;
					actor3.CurJob.count = 1;
				}
			};
			yield return loadIfNeeded;
			yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor2 = loadIfNeeded.actor;
				Building_TurretGun building_TurretGun = ((Building)actor2.CurJob.targetA.Thing) as Building_TurretGun;
				SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(building_TurretGun.Position, building_TurretGun.Map));
				building_TurretGun.gun.TryGetComp<CompChangeableProjectile>().LoadShell(actor2.CurJob.targetB.Thing.def, 1);
				actor2.carryTracker.innerContainer.ClearAndDestroyContents();
			};
			yield return toil;
			yield return gotoTurret;
			Toil man = new Toil();
			man.tickAction = delegate
			{
				Pawn actor = man.actor;
				Building building = (Building)actor.CurJob.targetA.Thing;
				if (GunNeedsLoading(building))
				{
					JumpToToil(loadIfNeeded);
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
}
