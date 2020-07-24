using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
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

		public ThingDef preExplosionSpawnThingDef;

		public float preExplosionSpawnChance;

		public int preExplosionSpawnThingCount = 1;

		public ThingDef postExplosionSpawnThingDef;

		public float postExplosionSpawnChance;

		public int postExplosionSpawnThingCount = 1;

		public float chanceToStartFire;

		public bool damageFalloff;

		public IntVec3? needLOSToCell1;

		public IntVec3? needLOSToCell2;

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
			if (!respawningAfterLoad)
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
			cellsToAffect.AddRange(damType.Worker.ExplosionCellsToHit(this));
			if (applyDamageToExplosionCellsNeighbors)
			{
				AddCellsNeighbors(cellsToAffect);
			}
			damType.Worker.ExplosionStart(this, cellsToAffect);
			PlayExplosionSound(explosionSound);
			MoteMaker.MakeWaterSplash(base.Position.ToVector3Shifted(), base.Map, radius * 6f, 20f);
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
		}

		public override void Tick()
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
					Log.Error(string.Concat("Explosion could not affect cell ", cellsToAffect[num], ": ", ex));
				}
				cellsToAffect.RemoveAt(num);
				num--;
			}
			if (!cellsToAffect.Any())
			{
				Destroy();
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
			Scribe_Values.Look(ref postExplosionSpawnChance, "postExplosionSpawnChance", 0f);
			Scribe_Values.Look(ref postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1);
			Scribe_Values.Look(ref chanceToStartFire, "chanceToStartFire", 0f);
			Scribe_Values.Look(ref damageFalloff, "dealMoreDamageAtCenter", defaultValue: false);
			Scribe_Values.Look(ref needLOSToCell1, "needLOSToCell1");
			Scribe_Values.Look(ref needLOSToCell2, "needLOSToCell2");
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Collections.Look(ref cellsToAffect, "cellsToAffect", LookMode.Value);
			Scribe_Collections.Look(ref damagedThings, "damagedThings", LookMode.Reference);
			Scribe_Collections.Look(ref ignoredThings, "ignoredThings", LookMode.Reference);
			Scribe_Collections.Look(ref addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
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
			return startTick + (int)((cell - base.Position).LengthHorizontal * 1.5f);
		}

		private void AffectCell(IntVec3 c)
		{
			if (c.InBounds(base.Map))
			{
				bool flag = ShouldCellBeAffectedOnlyByDamage(c);
				if (!flag && Rand.Chance(preExplosionSpawnChance) && c.Walkable(base.Map))
				{
					TrySpawnExplosionThing(preExplosionSpawnThingDef, c, preExplosionSpawnThingCount);
				}
				damType.Worker.ExplosionAffectCell(this, c, damagedThings, ignoredThings, !flag);
				if (!flag && Rand.Chance(postExplosionSpawnChance) && c.Walkable(base.Map))
				{
					TrySpawnExplosionThing(postExplosionSpawnThingDef, c, postExplosionSpawnThingCount);
				}
				float num = chanceToStartFire;
				if (damageFalloff)
				{
					num *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(base.Position) / radius);
				}
				if (Rand.Chance(num))
				{
					FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
				}
			}
		}

		private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
		{
			if (thingDef != null)
			{
				if (thingDef.IsFilth)
				{
					FilthMaker.TryMakeFilth(c, base.Map, thingDef, count);
					return;
				}
				Thing thing = ThingMaker.MakeThing(thingDef);
				thing.stackCount = count;
				GenSpawn.Spawn(thing, c, base.Map);
			}
		}

		private void PlayExplosionSound(SoundDef explosionSound)
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
}
