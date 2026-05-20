using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Pawn_FlightTracker : IExposable
{
	private enum FlightState
	{
		Grounded,
		Flying,
		TakingOff,
		Landing
	}

	private Pawn pawn;

	private int flyingTicks = -1;

	private int flightCooldownTicks;

	private int lerpTick;

	private AnimationDef playing;

	private FlightState flightState;

	private const int TakeoffDurationTicks = 50;

	private const int LandingDurationTicks = 50;

	private static readonly SimpleCurve TakeoffCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.5f, 0.6f),
		new CurvePoint(1f, 1f)
	};

	private static readonly SimpleCurve LandingCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(0.5f, 0.4f),
		new CurvePoint(1f, 0f)
	};

	public bool Flying => flightState != FlightState.Grounded;

	public bool CanEverFly
	{
		get
		{
			if (pawn.IsMutant && pawn.mutant.Def.disableFlying)
			{
				return false;
			}
			if (!pawn.RaceProps.canFlyInVacuum)
			{
				Map mapHeld = pawn.MapHeld;
				if (mapHeld != null)
				{
					BiomeDef biome = mapHeld.Biome;
					if (biome != null && biome.inVacuum)
					{
						return false;
					}
				}
			}
			return pawn.GetStatValue(StatDefOf.MaxFlightTime, applyPostProcess: true, 300) > 0f;
		}
	}

	public bool CanFlyNow
	{
		get
		{
			if (!Flying && CanEverFly)
			{
				return flightCooldownTicks <= 0;
			}
			return false;
		}
	}

	public int MaxFlightTicks => Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.MaxFlightTime, applyPostProcess: true, 300) * 60f);

	public float PositionOffsetFactor => flightState switch
	{
		FlightState.Flying => 1f, 
		FlightState.TakingOff => TakeoffCurve.Evaluate((float)lerpTick / 50f), 
		FlightState.Landing => LandingCurve.Evaluate((float)lerpTick / 50f), 
		_ => 0f, 
	};

	public Pawn_FlightTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void FlightTick()
	{
		CheckFlyAnimation();
		switch (flightState)
		{
		case FlightState.TakingOff:
			lerpTick++;
			if (lerpTick >= 50)
			{
				flightState = FlightState.Flying;
				lerpTick = 0;
			}
			break;
		case FlightState.Landing:
			lerpTick++;
			if (lerpTick < 50)
			{
				break;
			}
			flightState = FlightState.Grounded;
			lerpTick = 0;
			flightCooldownTicks = Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.FlightCooldown) * 60f);
			flyingTicks = -1;
			if (pawn.Downed && !pawn.Position.WalkableBy(pawn.Map, pawn))
			{
				pawn.Kill(null, null);
				Corpse corpse = pawn.Corpse;
				if (corpse != null && !corpse.Destroyed)
				{
					corpse.Destroy();
				}
			}
			break;
		case FlightState.Flying:
			flyingTicks++;
			if (flyingTicks >= MaxFlightTicks)
			{
				flightState = FlightState.Landing;
			}
			break;
		case FlightState.Grounded:
			if (flightCooldownTicks > 0)
			{
				flightCooldownTicks--;
			}
			if (pawn.CurJob != null && pawn.CurJob.flying)
			{
				pawn.CurJob.flying = false;
			}
			break;
		}
	}

	public void Notify_JobStarted(Job job)
	{
		if (!CanEverFly)
		{
			return;
		}
		if (job.def.tryStartFlying && CanFlyNow)
		{
			if (Rand.Chance((job.def.overrideFlyChance >= 0f) ? job.def.overrideFlyChance : pawn.RaceProps.flightStartChanceOnJobStart))
			{
				StartFlyingInternal();
				job.flying = true;
				return;
			}
		}
		else if ((job.def.tryStartFlying || job.def.ifFlyingKeepFlying) && Flying)
		{
			job.flying = true;
			return;
		}
		job.flying = false;
		if (Flying)
		{
			ForceLand();
		}
	}

	public void StartFlying()
	{
		if (CanFlyNow)
		{
			StartFlyingInternal();
		}
	}

	private void StartFlyingInternal()
	{
		flightState = FlightState.TakingOff;
		lerpTick = 0;
		flyingTicks = -1;
		CheckFlyAnimation();
	}

	private void CheckFlyAnimation()
	{
		bool flying = Flying;
		if (!flying && playing != null)
		{
			if (pawn.Drawer.renderer.CurAnimation == playing)
			{
				pawn.Drawer.renderer.SetAnimation(null);
			}
			playing = null;
		}
		else if (flying)
		{
			AnimationDef bestFlyAnimation = GetBestFlyAnimation(pawn);
			if (pawn.Drawer.renderer.CurAnimation != bestFlyAnimation)
			{
				pawn.Drawer.renderer.SetAnimation(bestFlyAnimation);
				playing = bestFlyAnimation;
			}
		}
	}

	public void ForceLand()
	{
		if (flightState == FlightState.TakingOff || flightState == FlightState.Flying)
		{
			flightState = FlightState.Landing;
			lerpTick = 25;
			flightCooldownTicks = Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.FlightCooldown) * 60f) + 50;
		}
	}

	public string GetStatusString()
	{
		if (Flying)
		{
			return "Flying".Translate().CapitalizeFirst();
		}
		return null;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref flyingTicks, "flyingTicks", -1);
		Scribe_Values.Look(ref flightCooldownTicks, "flightCooldownTicks", 0);
		Scribe_Values.Look(ref lerpTick, "lerpTick", 0);
		Scribe_Values.Look(ref flightState, "flightState", FlightState.Grounded);
		Scribe_Defs.Look(ref playing, "playing");
	}

	public static AnimationDef GetBestFlyAnimation(Pawn pawn, Rot4? facingOverride = null)
	{
		if (pawn?.kindDef == null)
		{
			return null;
		}
		if (pawn.RaceProps.Humanlike)
		{
			return null;
		}
		Rot4 obj = facingOverride ?? pawn.Rotation;
		bool flag = obj == Rot4.South;
		bool flag2 = obj == Rot4.North;
		bool flag3 = pawn.gender == Gender.Female;
		Pawn_AgeTracker ageTracker = pawn.ageTracker;
		if (ageTracker != null)
		{
			PawnKindLifeStage curKindLifeStage = ageTracker.CurKindLifeStage;
			if (curKindLifeStage != null)
			{
				if (flag && curKindLifeStage.flyingAnimationSouth != null)
				{
					if (flag3 && curKindLifeStage.flyingAnimationSouthFemale != null)
					{
						return curKindLifeStage.flyingAnimationSouthFemale;
					}
					return curKindLifeStage.flyingAnimationSouth;
				}
				if ((flag2 || flag) && curKindLifeStage.flyingAnimationNorth != null)
				{
					if (flag3 && curKindLifeStage.flyingAnimationNorthFemale != null)
					{
						return curKindLifeStage.flyingAnimationNorthFemale;
					}
					return curKindLifeStage.flyingAnimationNorth;
				}
				if (curKindLifeStage.flyingAnimationEast != null)
				{
					if (flag3 && curKindLifeStage.flyingAnimationEastFemale != null)
					{
						return curKindLifeStage.flyingAnimationEastFemale;
					}
					return curKindLifeStage.flyingAnimationEast;
				}
				if (curKindLifeStage.flyingAnimationNorth != null)
				{
					if (flag3 && curKindLifeStage.flyingAnimationNorthFemale != null)
					{
						return curKindLifeStage.flyingAnimationNorthFemale;
					}
					return curKindLifeStage.flyingAnimationNorth;
				}
				if (curKindLifeStage.flyingAnimationSouth != null)
				{
					if (flag3 && curKindLifeStage.flyingAnimationSouthFemale != null)
					{
						return curKindLifeStage.flyingAnimationSouthFemale;
					}
					return curKindLifeStage.flyingAnimationSouth;
				}
			}
		}
		if (pawn.ageTracker == null)
		{
			return null;
		}
		int num = pawn.ageTracker.CurLifeStageIndex;
		while (num > 0)
		{
			num--;
			PawnKindLifeStage curKindLifeStage = pawn.kindDef.lifeStages[num];
			if (flag && curKindLifeStage.flyingAnimationSouth != null)
			{
				if (flag3 && curKindLifeStage.flyingAnimationSouthFemale != null)
				{
					return curKindLifeStage.flyingAnimationSouthFemale;
				}
				return curKindLifeStage.flyingAnimationSouth;
			}
			if ((flag2 || flag) && curKindLifeStage.flyingAnimationNorth != null)
			{
				if (flag3 && curKindLifeStage.flyingAnimationNorthFemale != null)
				{
					return curKindLifeStage.flyingAnimationNorthFemale;
				}
				return curKindLifeStage.flyingAnimationNorth;
			}
			if (curKindLifeStage.flyingAnimationEast != null)
			{
				if (flag3 && curKindLifeStage.flyingAnimationEastFemale != null)
				{
					return curKindLifeStage.flyingAnimationEastFemale;
				}
				return curKindLifeStage.flyingAnimationEast;
			}
			if (curKindLifeStage.flyingAnimationNorth != null)
			{
				if (flag3 && curKindLifeStage.flyingAnimationNorthFemale != null)
				{
					return curKindLifeStage.flyingAnimationNorthFemale;
				}
				return curKindLifeStage.flyingAnimationNorth;
			}
			if (curKindLifeStage.flyingAnimationSouth != null)
			{
				if (flag3 && curKindLifeStage.flyingAnimationSouthFemale != null)
				{
					return curKindLifeStage.flyingAnimationSouthFemale;
				}
				return curKindLifeStage.flyingAnimationSouth;
			}
		}
		return null;
	}
}
