using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class GameCondition_Flashstorm : GameCondition
{
	private static readonly IntRange AreaRadiusRange = new IntRange(45, 60);

	private static readonly IntRange TicksBetweenStrikes = new IntRange(320, 800);

	private const int RainDisableTicksAfterConditionEnds = 30000;

	private const int AvoidConditionCauserExpandRect = 2;

	public IntVec2 centerLocation = IntVec2.Invalid;

	public IntRange areaRadiusOverride = IntRange.Zero;

	public IntRange initialStrikeDelay = IntRange.Zero;

	public bool ambientSound;

	private int areaRadius;

	private int nextLightningTicks;

	private Sustainer soundSustainer;

	public bool avoidConditionCauser;

	public int AreaRadius => areaRadius;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref centerLocation, "centerLocation");
		Scribe_Values.Look(ref areaRadius, "areaRadius", 0);
		Scribe_Values.Look(ref areaRadiusOverride, "areaRadiusOverride");
		Scribe_Values.Look(ref nextLightningTicks, "nextLightningTicks", 0);
		Scribe_Values.Look(ref initialStrikeDelay, "initialStrikeDelay");
		Scribe_Values.Look(ref ambientSound, "ambientSound", defaultValue: false);
		Scribe_Values.Look(ref avoidConditionCauser, "avoidConditionCauser", defaultValue: false);
	}

	public override void Init()
	{
		base.Init();
		areaRadius = ((areaRadiusOverride == IntRange.Zero) ? AreaRadiusRange.RandomInRange : areaRadiusOverride.RandomInRange);
		nextLightningTicks = Find.TickManager.TicksGame + initialStrikeDelay.RandomInRange;
		if (centerLocation.IsInvalid)
		{
			FindGoodCenterLocation();
		}
	}

	public override void GameConditionTick()
	{
		if (Find.TickManager.TicksGame > nextLightningTicks)
		{
			Vector2 vector = Rand.UnitVector2 * Rand.Range(0f, areaRadius);
			IntVec3 intVec = new IntVec3((int)Math.Round(vector.x) + centerLocation.x, 0, (int)Math.Round(vector.y) + centerLocation.z);
			if (IsGoodLocationForStrike(intVec))
			{
				base.SingleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(base.SingleMap, intVec));
				nextLightningTicks = Find.TickManager.TicksGame + TicksBetweenStrikes.RandomInRange;
			}
		}
		if (ambientSound)
		{
			if (soundSustainer == null || soundSustainer.Ended)
			{
				soundSustainer = SoundDefOf.FlashstormAmbience.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(centerLocation.ToIntVec3, base.SingleMap), MaintenanceType.PerTick));
			}
			else
			{
				soundSustainer.Maintain();
			}
		}
	}

	public override void End()
	{
		base.SingleMap.weatherDecider.DisableRainFor(30000);
		base.End();
	}

	private void FindGoodCenterLocation()
	{
		if (base.SingleMap.Size.x <= 16 || base.SingleMap.Size.z <= 16)
		{
			throw new Exception("Map too small for flashstorm.");
		}
		for (int i = 0; i < 10; i++)
		{
			centerLocation = new IntVec2(Rand.Range(8, base.SingleMap.Size.x - 8), Rand.Range(8, base.SingleMap.Size.z - 8));
			if (IsGoodCenterLocation(centerLocation))
			{
				break;
			}
		}
	}

	private bool IsGoodLocationForStrike(IntVec3 loc)
	{
		if (!loc.InBounds(base.SingleMap) || loc.Roofed(base.SingleMap) || !loc.Standable(base.SingleMap))
		{
			return false;
		}
		if (avoidConditionCauser && conditionCauser != null && conditionCauser.OccupiedRect().ExpandedBy(2).Contains(loc))
		{
			return false;
		}
		return true;
	}

	private bool IsGoodCenterLocation(IntVec2 loc)
	{
		int num = 0;
		int num2 = (int)(MathF.PI * (float)areaRadius * (float)areaRadius / 2f);
		foreach (IntVec3 potentiallyAffectedCell in GetPotentiallyAffectedCells(loc))
		{
			if (IsGoodLocationForStrike(potentiallyAffectedCell))
			{
				num++;
			}
			if (num >= num2)
			{
				break;
			}
		}
		return num >= num2;
	}

	private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
	{
		int x = center.x - areaRadius;
		while (x <= center.x + areaRadius)
		{
			int num;
			for (int z = center.z - areaRadius; z <= center.z + areaRadius; z = num)
			{
				if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= areaRadius * areaRadius)
				{
					yield return new IntVec3(x, 0, z);
				}
				num = z + 1;
			}
			num = x + 1;
			x = num;
		}
	}
}
