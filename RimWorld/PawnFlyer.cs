using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class PawnFlyer : Thing, IThingHolderTickable, IThingHolder, ISearchableContents
	{
		private ThingOwner<Thing> innerContainer;

		protected Vector3 startVec;

		private IntVec3 destCell;

		private float flightDistance;

		private bool pawnWasDrafted;

		private bool pawnCanFireAtWill = true;

		protected int ticksFlightTime = 120;

		protected int ticksFlying;

		private JobQueue jobQueue;

		protected EffecterDef flightEffecterDef;

		protected SoundDef soundLanding;

		private Thing carriedThing;

		private LocalTargetInfo target;

		private AbilityDef triggeringAbility;

		private Effecter flightEffecter;

		private int positionLastComputedTick = -1;

		private Vector3 groundPos;

		private Vector3 effectivePos;

		private float effectiveHeight;

		private const int CheckDestinationInterval = 15;

		public bool ShouldTickContents => ticksFlying < ticksFlightTime;

		protected Thing FlyingThing
		{
			get
			{
				if (innerContainer.InnerListForReading.Count <= 0)
				{
					return null;
				}
				return innerContainer.InnerListForReading[0];
			}
		}

		public Pawn FlyingPawn => FlyingThing as Pawn;

		public Thing CarriedThing => carriedThing;

		public ThingOwner SearchableContents => innerContainer;

		public override Vector3 DrawPos
		{
			get
			{
				RecomputePosition();
				return effectivePos;
			}
		}

		public override int UpdateRateTicks => 1;

		public Vector3 DestinationPos
		{
			get
			{
				Thing flyingThing = FlyingThing;
				return GenThing.TrueCenter(destCell, flyingThing.Rotation, flyingThing.def.size, flyingThing.def.Altitude);
			}
		}

		private void RecomputePosition()
		{
			if (positionLastComputedTick != ticksFlying)
			{
				positionLastComputedTick = ticksFlying;
				float t = (float)ticksFlying / (float)ticksFlightTime;
				float t2 = def.pawnFlyer.Worker.AdjustedProgress(t);
				effectiveHeight = def.pawnFlyer.Worker.GetHeight(t2);
				groundPos = Vector3.Lerp(startVec, DestinationPos, t2);
				Vector3 vector = Altitudes.AltIncVect * effectiveHeight;
				Vector3 vector2 = Vector3.forward * (def.pawnFlyer.heightFactor * effectiveHeight);
				effectivePos = groundPos + vector + vector2;
				base.Position = groundPos.ToIntVec3();
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

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			flightEffecter?.Cleanup();
			base.Destroy(mode);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
			{
				float a = Mathf.Max(flightDistance, 1f) / def.pawnFlyer.flightSpeed;
				a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
				ticksFlightTime = a.SecondsToTicks();
				ticksFlying = 0;
			}
		}

		protected virtual void RespawnPawn()
		{
			Thing flyingThing = FlyingThing;
			LandingEffects();
			innerContainer.TryDrop(flyingThing, destCell, flyingThing.MapHeld, ThingPlaceMode.Direct, out var lastResultingThing, null, null, playDropSound: false);
			Pawn pawn = flyingThing as Pawn;
			if (pawn?.drafter != null)
			{
				pawn.drafter.Drafted = pawnWasDrafted;
				pawn.drafter.FireAtWill = pawnCanFireAtWill;
			}
			flyingThing.Rotation = base.Rotation;
			if (carriedThing != null && innerContainer.TryDrop(carriedThing, destCell, flyingThing.MapHeld, ThingPlaceMode.Direct, out lastResultingThing, null, null, playDropSound: false) && pawn != null)
			{
				carriedThing.DeSpawn();
				if (!pawn.carryTracker.TryStartCarry(carriedThing))
				{
					Log.Error("Could not carry " + carriedThing.ToStringSafe() + " after respawning flyer pawn.");
				}
			}
			if (pawn == null)
			{
				return;
			}
			if (jobQueue != null)
			{
				pawn.jobs.RestoreCapturedJobs(jobQueue);
			}
			pawn.jobs.CheckForJobOverride(0f, ignoreQueue: false);
			if (def.pawnFlyer.stunDurationTicksRange != IntRange.Zero)
			{
				pawn.stances.stunner.StunFor(def.pawnFlyer.stunDurationTicksRange.RandomInRange, null, addBattleLog: false, showMote: false);
			}
			if (triggeringAbility == null)
			{
				return;
			}
			Ability ability = pawn.abilities.GetAbility(triggeringAbility);
			if (ability?.comps == null)
			{
				return;
			}
			foreach (AbilityComp comp in ability.comps)
			{
				if (comp is ICompAbilityEffectOnJumpCompleted compAbilityEffectOnJumpCompleted)
				{
					compAbilityEffectOnJumpCompleted.OnJumpCompleted(startVec.ToIntVec3(), target);
				}
			}
		}

		private void LandingEffects()
		{
			soundLanding?.PlayOneShot(new TargetInfo(base.Position, base.Map));
			FleckMaker.ThrowDustPuff(DestinationPos + Gen.RandomHorizontalVector(0.5f), base.Map, 2f);
		}

		protected override void TickInterval(int delta)
		{
			if (flightEffecter == null && flightEffecterDef != null)
			{
				flightEffecter = flightEffecterDef.Spawn();
				flightEffecter.Trigger(this, TargetInfo.Invalid);
			}
			else
			{
				flightEffecter?.EffectTick(this, TargetInfo.Invalid);
			}
			if (ticksFlying >= ticksFlightTime)
			{
				RespawnPawn();
				Destroy();
			}
			else if (this.IsHashIntervalTick(15, delta))
			{
				CheckDestination();
			}
			ticksFlying += delta;
		}

		private void CheckDestination()
		{
			if (JumpUtility.ValidJumpTarget(FlyingThing, base.Map, destCell))
			{
				return;
			}
			int num = GenRadial.NumCellsInRadius(3.9f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 cell = destCell + GenRadial.RadialPattern[i];
				if (JumpUtility.ValidJumpTarget(FlyingThing, base.Map, cell))
				{
					destCell = cell;
					break;
				}
			}
		}

		public void Notify_TransportedOnGravship(Gravship gravship)
		{
			IntVec3 intVec = gravship.Engine.Position - gravship.originalPosition;
			startVec += intVec.ToVector3();
			destCell += intVec;
			positionLastComputedTick = -1;
		}

		public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
		{
			RecomputePosition();
			if (FlyingPawn != null)
			{
				FlyingPawn.DynamicDrawPhaseAt(phase, effectivePos);
			}
			else
			{
				FlyingThing?.DynamicDrawPhaseAt(phase, effectivePos);
			}
			base.DynamicDrawPhaseAt(phase, drawLoc, flip);
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			DrawShadow(groundPos, effectiveHeight);
			if (CarriedThing != null && FlyingPawn != null)
			{
				PawnRenderUtility.DrawCarriedThing(FlyingPawn, effectivePos, CarriedThing);
			}
		}

		private void DrawShadow(Vector3 drawLoc, float height)
		{
			Material shadowMaterial = def.pawnFlyer.ShadowMaterial;
			if (!(shadowMaterial == null))
			{
				float num = Mathf.Lerp(1f, 0.6f, height);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawLoc, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
			}
		}

		public static PawnFlyer MakeFlyer(ThingDef flyingDef, Pawn pawn, IntVec3 destCell, EffecterDef flightEffecterDef, SoundDef landingSound, bool flyWithCarriedThing = false, Vector3? overrideStartVec = null, Ability triggeringAbility = null, LocalTargetInfo target = default(LocalTargetInfo))
		{
			PawnFlyer pawnFlyer = (PawnFlyer)ThingMaker.MakeThing(flyingDef);
			pawnFlyer.startVec = overrideStartVec ?? pawn.TrueCenter();
			pawnFlyer.Rotation = pawn.Rotation;
			pawnFlyer.flightDistance = pawn.Position.DistanceTo(destCell);
			pawnFlyer.destCell = destCell;
			pawnFlyer.pawnWasDrafted = pawn.Drafted;
			pawnFlyer.flightEffecterDef = flightEffecterDef;
			pawnFlyer.soundLanding = landingSound;
			pawnFlyer.triggeringAbility = triggeringAbility?.def;
			pawnFlyer.target = target;
			if (pawn.drafter != null)
			{
				pawnFlyer.pawnCanFireAtWill = pawn.drafter.FireAtWill;
			}
			if (pawn.CurJob != null)
			{
				if (pawn.CurJob.def == JobDefOf.CastJump)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else
				{
					pawn.jobs.SuspendCurrentJob(JobCondition.InterruptForced);
				}
			}
			pawnFlyer.jobQueue = pawn.jobs.CaptureAndClearJobQueue();
			if (flyWithCarriedThing && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out pawnFlyer.carriedThing))
			{
				if (pawnFlyer.carriedThing.holdingOwner != null)
				{
					pawnFlyer.carriedThing.holdingOwner.Remove(pawnFlyer.carriedThing);
				}
				pawnFlyer.carriedThing.DeSpawn();
			}
			if (pawn.Spawned)
			{
				pawn.DeSpawn(DestroyMode.WillReplace);
			}
			if (!pawnFlyer.innerContainer.TryAdd(pawn))
			{
				Log.Error("Could not add " + pawn.ToStringSafe() + " to a flyer.");
				pawn.Destroy();
			}
			if (pawnFlyer.carriedThing != null && !pawnFlyer.innerContainer.TryAdd(pawnFlyer.carriedThing))
			{
				Log.Error("Could not add " + pawnFlyer.carriedThing.ToStringSafe() + " to a flyer.");
			}
			return pawnFlyer;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startVec, "startVec");
			Scribe_Values.Look(ref destCell, "destCell");
			Scribe_Values.Look(ref flightDistance, "flightDistance", 0f);
			Scribe_Values.Look(ref pawnWasDrafted, "pawnWasDrafted", defaultValue: false);
			Scribe_Values.Look(ref pawnCanFireAtWill, "pawnCanFireAtWill", defaultValue: true);
			Scribe_Values.Look(ref ticksFlightTime, "ticksFlightTime", 0);
			Scribe_Values.Look(ref ticksFlying, "ticksFlying", 0);
			Scribe_Defs.Look(ref flightEffecterDef, "flightEffecterDef");
			Scribe_Defs.Look(ref soundLanding, "soundLanding");
			Scribe_Defs.Look(ref triggeringAbility, "triggeringAbility");
			Scribe_References.Look(ref carriedThing, "carriedThing");
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Deep.Look(ref jobQueue, "jobQueue");
			Scribe_TargetInfo.Look(ref target, "target");
		}
	}
}
