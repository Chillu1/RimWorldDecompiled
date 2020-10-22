using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class PawnFlyer : Thing, IThingHolder
	{
		private ThingOwner<Thing> innerContainer;

		protected Vector3 startVec;

		private float flightDistance;

		private bool pawnWasDrafted;

		private bool pawnWasSelected;

		protected int ticksFlightTime = 120;

		protected int ticksFlying;

		private JobQueue jobQueue;

		public Pawn FlyingPawn
		{
			get
			{
				if (innerContainer.InnerListForReading.Count <= 0)
				{
					return null;
				}
				return innerContainer.InnerListForReading[0] as Pawn;
			}
		}

		public Vector3 DestinationPos
		{
			get
			{
				Pawn flyingPawn = FlyingPawn;
				return GenThing.TrueCenter(base.Position, flyingPawn.Rotation, flyingPawn.def.size, flyingPawn.def.Altitude);
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public PawnFlyer()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				float a = Mathf.Max(flightDistance, 1f) / def.pawnFlyer.flightSpeed;
				a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
				ticksFlightTime = a.SecondsToTicks();
				ticksFlying = 0;
			}
		}

		protected virtual void RespawnPawn()
		{
			Pawn flyingPawn = FlyingPawn;
			innerContainer.TryDrop_NewTmp(flyingPawn, base.Position, flyingPawn.MapHeld, ThingPlaceMode.Direct, out var _, null, null, playDropSound: false);
			if (flyingPawn.drafter != null)
			{
				flyingPawn.drafter.Drafted = pawnWasDrafted;
			}
			if (pawnWasSelected && Find.CurrentMap == flyingPawn.Map)
			{
				Find.Selector.Unshelve(flyingPawn, playSound: false);
			}
			if (jobQueue != null)
			{
				flyingPawn.jobs.RestoreCapturedJobs(jobQueue);
			}
		}

		public override void Tick()
		{
			if (ticksFlying >= ticksFlightTime)
			{
				RespawnPawn();
				Destroy();
			}
			else
			{
				if (ticksFlying % 5 == 0)
				{
					CheckDestination();
				}
				innerContainer.ThingOwnerTick();
			}
			ticksFlying++;
		}

		private void CheckDestination()
		{
			if (Verb_Jump.ValidJumpTarget(base.Map, base.Position))
			{
				return;
			}
			int num = GenRadial.NumCellsInRadius(3.9f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
				if (Verb_Jump.ValidJumpTarget(base.Map, intVec))
				{
					base.Position = intVec;
					break;
				}
			}
		}

		public static PawnFlyer MakeFlyer(ThingDef flyingDef, Pawn pawn, IntVec3 destCell)
		{
			PawnFlyer pawnFlyer = (PawnFlyer)ThingMaker.MakeThing(flyingDef);
			if (!pawnFlyer.ValidateFlyer())
			{
				return null;
			}
			pawnFlyer.startVec = pawn.TrueCenter();
			pawnFlyer.flightDistance = pawn.Position.DistanceTo(destCell);
			pawnFlyer.pawnWasDrafted = pawn.Drafted;
			pawnFlyer.pawnWasSelected = Find.Selector.IsSelected(pawn);
			if (pawnFlyer.pawnWasDrafted)
			{
				Find.Selector.ShelveSelected(pawn);
			}
			pawnFlyer.jobQueue = pawn.jobs.CaptureAndClearJobQueue();
			pawn.DeSpawn();
			if (!pawnFlyer.innerContainer.TryAdd(pawn))
			{
				Log.Error("Could not add " + pawn.ToStringSafe() + " to a flyer.");
				pawn.Destroy();
			}
			return pawnFlyer;
		}

		protected virtual bool ValidateFlyer()
		{
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref startVec, "startVec");
			Scribe_Values.Look(ref flightDistance, "flightDistance", 0f);
			Scribe_Values.Look(ref pawnWasDrafted, "pawnWasDrafted", defaultValue: false);
			Scribe_Values.Look(ref pawnWasSelected, "pawnWasSelected", defaultValue: false);
			Scribe_Values.Look(ref ticksFlightTime, "ticksFlightTime", 0);
			Scribe_Values.Look(ref ticksFlying, "ticksFlying", 0);
			Scribe_Deep.Look(ref jobQueue, "jobQueue");
		}
	}
}
