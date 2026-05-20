using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Explosion : Thing
{
	public float radius;

	public DamageDef damType;

	public int damAmount;

	public float armorPenetration;

	public Thing instigator;

	public ThingDef weapon;

	public ThingDef projectile;

	public Thing intendedTarget;

	public bool applyDamageToExplosionCellsNeighbors;

	public FloatRange? affectedAngle;

	public ThingDef preExplosionSpawnThingDef;

	public float preExplosionSpawnChance;

	public int preExplosionSpawnThingCount = 1;

	public ThingDef postExplosionSpawnThingDef;

	public ThingDef postExplosionSpawnThingDefWater;

	public float postExplosionSpawnChance;

	public int postExplosionSpawnThingCount = 1;

	public GasType? postExplosionGasType;

	public int postExplosionGasAmount = 255;

	public float? postExplosionGasRadiusOverride;

	public float chanceToStartFire;

	public SimpleCurve flammabilityChanceCurve;

	public bool damageFalloff;

	public IntVec3? needLOSToCell1;

	public IntVec3? needLOSToCell2;

	public bool doVisualEffects = true;

	public float propagationSpeed = 1f;

	public float excludeRadius;

	public bool doSoundEffects = true;

	public float screenShakeFactor = 1f;

	public List<IntVec3> overrideCells;

	public ThingDef preExplosionSpawnSingleThingDef;

	public ThingDef postExplosionSpawnSingleThingDef;

	private int startTick;

	private List<IntVec3> cellsToAffect;

	private List<Thing> damagedThings;

	private List<Thing> ignoredThings;

	private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;

	private const float DamageFactorAtEdge = 0.2f;

	private static HashSet<IntVec3> tmpCells = new HashSet<IntVec3>();

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
		{
			cellsToAffect = SimplePool<List<IntVec3>>.Get();
			cellsToAffect.Clear();
			damagedThings = SimplePool<List<Thing>>.Get();
			damagedThings.Clear();
			addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
			addedCellsAffectedOnlyByDamage.Clear();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		cellsToAffect.Clear();
		SimplePool<List<IntVec3>>.Return(cellsToAffect);
		cellsToAffect = null;
		damagedThings.Clear();
		SimplePool<List<Thing>>.Return(damagedThings);
		damagedThings = null;
		addedCellsAffectedOnlyByDamage.Clear();
		SimplePool<HashSet<IntVec3>>.Return(addedCellsAffectedOnlyByDamage);
		addedCellsAffectedOnlyByDamage = null;
	}

	public virtual void StartExplosion(SoundDef explosionSound, List<Thing> ignoredThings)
	{
		if (!base.Spawned)
		{
			Log.Error("Called StartExplosion() on unspawned thing.");
			return;
		}
		startTick = Find.TickManager.TicksGame;
		this.ignoredThings = ignoredThings;
		cellsToAffect.Clear();
		damagedThings.Clear();
		addedCellsAffectedOnlyByDamage.Clear();
		if (!overrideCells.NullOrEmpty())
		{
			cellsToAffect.AddRange(overrideCells);
		}
		else
		{
			cellsToAffect.AddRange(damType.Worker.ExplosionCellsToHit(this));
		}
		if (applyDamageToExplosionCellsNeighbors)
		{
			AddCellsNeighbors(cellsToAffect);
		}
		damType.Worker.ExplosionStart(this, cellsToAffect);
		PlayExplosionSound(explosionSound);
		if (doVisualEffects)
		{
			FleckMaker.WaterSplash(base.Position.ToVector3Shifted(), base.Map, radius * 6f, 20f);
		}
		cellsToAffect.Sort((IntVec3 a, IntVec3 b) => GetCellAffectTick(b).CompareTo(GetCellAffectTick(a)));
		RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate(Region x)
		{
			List<Thing> allThings = x.ListerThings.AllThings;
			for (int num = allThings.Count - 1; num >= 0; num--)
			{
				if (allThings[num].Spawned)
				{
					allThings[num].Notify_Explosion(this);
				}
			}
			return false;
		}, 25);
		TrySpawnSingleThing(preExplosionSpawnSingleThingDef);
	}

	protected override void Tick()
	{
		int ticksGame = Find.TickManager.TicksGame;
		int num = cellsToAffect.Count - 1;
		while (num >= 0 && ticksGame >= GetCellAffectTick(cellsToAffect[num]))
		{
			try
			{
				AffectCell(cellsToAffect[num]);
			}
			catch (Exception ex)
			{
				Log.Error("Explosion could not affect cell " + cellsToAffect[num].ToString() + ": " + ex);
			}
			cellsToAffect.RemoveAt(num);
			num--;
		}
		if (!cellsToAffect.Any())
		{
			ExplosionEnded();
			Destroy();
		}
	}

	protected virtual void ExplosionEnded()
	{
		TrySpawnSingleThing(postExplosionSpawnSingleThingDef);
	}

	private void TrySpawnSingleThing(ThingDef thingDef)
	{
		if (thingDef == null)
		{
			return;
		}
		CellRect cellRect = base.Position.RectAbout(thingDef.Size);
		bool flag = false;
		if (thingDef.terrainAffordanceNeeded != null)
		{
			foreach (IntVec3 item in cellRect)
			{
				if (!item.GetAffordances(base.Map).Contains(thingDef.terrainAffordanceNeeded))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			TrySpawnExplosionThing(thingDef, base.Position, 1);
		}
	}

	public int GetDamageAmountAt(IntVec3 c)
	{
		if (!damageFalloff)
		{
			return damAmount;
		}
		float t = c.DistanceTo(base.Position) / radius;
		return Mathf.Max(GenMath.RoundRandom(Mathf.Lerp(damAmount, (float)damAmount * 0.2f, t)), 1);
	}

	public float GetArmorPenetrationAt(IntVec3 c)
	{
		if (!damageFalloff)
		{
			return armorPenetration;
		}
		float t = c.DistanceTo(base.Position) / radius;
		return Mathf.Lerp(armorPenetration, armorPenetration * 0.2f, t);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref radius, "radius", 0f);
		Scribe_Defs.Look(ref damType, "damType");
		Scribe_Values.Look(ref damAmount, "damAmount", 0);
		Scribe_Values.Look(ref armorPenetration, "armorPenetration", 0f);
		Scribe_References.Look(ref instigator, "instigator");
		Scribe_Defs.Look(ref weapon, "weapon");
		Scribe_Defs.Look(ref projectile, "projectile");
		Scribe_References.Look(ref intendedTarget, "intendedTarget");
		Scribe_Values.Look(ref applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors", defaultValue: false);
		Scribe_Defs.Look(ref preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
		Scribe_Values.Look(ref preExplosionSpawnChance, "preExplosionSpawnChance", 0f);
		Scribe_Values.Look(ref preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1);
		Scribe_Defs.Look(ref postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
		Scribe_Defs.Look(ref postExplosionSpawnThingDefWater, "postExplosionSpawnThingDefWater");
		Scribe_Values.Look(ref postExplosionSpawnChance, "postExplosionSpawnChance", 0f);
		Scribe_Values.Look(ref postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1);
		Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");
		Scribe_Values.Look(ref chanceToStartFire, "chanceToStartFire", 0f);
		Scribe_Deep.Look(ref flammabilityChanceCurve, "flammabilityChanceCurve");
		Scribe_Values.Look(ref damageFalloff, "dealMoreDamageAtCenter", defaultValue: false);
		Scribe_Values.Look(ref needLOSToCell1, "needLOSToCell1");
		Scribe_Values.Look(ref needLOSToCell2, "needLOSToCell2");
		Scribe_Values.Look(ref affectedAngle, "affectedAngle");
		Scribe_Values.Look(ref propagationSpeed, "propagationSpeed", 0f);
		Scribe_Values.Look(ref excludeRadius, "canTargetLocations", 0f);
		Scribe_Values.Look(ref doSoundEffects, "doSoundEffects", defaultValue: true);
		Scribe_Values.Look(ref doVisualEffects, "doVisualEffects", defaultValue: true);
		Scribe_Values.Look(ref screenShakeFactor, "screenShakeFactor", 1f);
		Scribe_Defs.Look(ref postExplosionSpawnSingleThingDef, "postExplosionSpawnSingleThingDef");
		Scribe_Defs.Look(ref preExplosionSpawnSingleThingDef, "preExplosionSpawnSingleThingDef");
		Scribe_Values.Look(ref startTick, "startTick", 0);
		Scribe_Collections.Look(ref cellsToAffect, "cellsToAffect", LookMode.Value);
		Scribe_Collections.Look(ref damagedThings, "damagedThings", LookMode.Reference);
		Scribe_Collections.Look(ref ignoredThings, "ignoredThings", LookMode.Reference);
		Scribe_Collections.Look(ref addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
		Scribe_Collections.Look(ref overrideCells, "overrideCells", LookMode.Value);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (damagedThings != null)
		{
			damagedThings.RemoveAll((Thing x) => x == null);
		}
		if (ignoredThings != null)
		{
			ignoredThings.RemoveAll((Thing x) => x == null);
		}
	}

	private int GetCellAffectTick(IntVec3 cell)
	{
		return startTick + (int)((cell - base.Position).LengthHorizontal * 1.5f / propagationSpeed);
	}

	private void AffectCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map) || (excludeRadius > 0f && (float)c.DistanceToSquared(base.Position) < excludeRadius * excludeRadius))
		{
			return;
		}
		TerrainDef terrain = c.GetTerrain(base.Map);
		bool flag = ShouldCellBeAffectedOnlyByDamage(c);
		if (!flag && Rand.Chance(preExplosionSpawnChance) && c.Walkable(base.Map))
		{
			TrySpawnExplosionThing(preExplosionSpawnThingDef, c, preExplosionSpawnThingCount);
		}
		damType.Worker.ExplosionAffectCell(this, c, damagedThings, ignoredThings, !flag);
		if (!flag)
		{
			if (Rand.Chance(postExplosionSpawnChance) && c.Walkable(base.Map))
			{
				ThingDef thingDef = (terrain.IsWater ? (postExplosionSpawnThingDefWater ?? postExplosionSpawnThingDef) : postExplosionSpawnThingDef);
				TrySpawnExplosionThing(thingDef, c, postExplosionSpawnThingCount);
			}
			if (postExplosionGasType.HasValue)
			{
				float num = postExplosionGasRadiusOverride ?? radius;
				float num2 = num * num;
				if ((float)c.DistanceToSquared(base.Position) <= num2)
				{
					GasUtility.AddGas(c, base.Map, postExplosionGasType.Value, postExplosionGasAmount);
				}
			}
		}
		float num3 = chanceToStartFire;
		if (damageFalloff)
		{
			num3 *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(base.Position) / radius);
		}
		if (Rand.Chance(num3))
		{
			FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f), instigator, flammabilityChanceCurve);
		}
		if (terrain.temporary)
		{
			TempTerrainProps tempTerrain = terrain.tempTerrain;
			if (tempTerrain != null && tempTerrain.removedByExplosions)
			{
				base.Map.terrainGrid.RemoveTempTerrain(c);
			}
		}
	}

	private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
	{
		if (thingDef != null)
		{
			Thing thing;
			if (thingDef.IsFilth)
			{
				FilthMaker.TryMakeFilth(c, base.Map, thingDef, count);
			}
			else if (GenSpawn.TrySpawn(thingDef, c, base.Map, out thing))
			{
				thing.stackCount = count;
				thing.TryGetComp<CompReleaseGas>()?.StartRelease();
			}
		}
	}

	private void PlayExplosionSound(SoundDef explosionSound)
	{
		if (doSoundEffects)
		{
			if ((!Prefs.DevMode) ? (!explosionSound.NullOrUndefined()) : (explosionSound != null))
			{
				explosionSound.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			else
			{
				damType.soundExplosion.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
		}
	}

	private void AddCellsNeighbors(List<IntVec3> cells)
	{
		tmpCells.Clear();
		addedCellsAffectedOnlyByDamage.Clear();
		for (int i = 0; i < cells.Count; i++)
		{
			tmpCells.Add(cells[i]);
		}
		for (int j = 0; j < cells.Count; j++)
		{
			if (!cells[j].Walkable(base.Map))
			{
				continue;
			}
			for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
			{
				IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
				if (intVec.InBounds(base.Map) && tmpCells.Add(intVec))
				{
					addedCellsAffectedOnlyByDamage.Add(intVec);
				}
			}
		}
		cells.Clear();
		foreach (IntVec3 tmpCell in tmpCells)
		{
			cells.Add(tmpCell);
		}
		tmpCells.Clear();
	}

	private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
	{
		if (!applyDamageToExplosionCellsNeighbors)
		{
			return false;
		}
		return addedCellsAffectedOnlyByDamage.Contains(c);
	}
}
