using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse
{
	public static class GenExplosion
	{
		private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

		public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null)
		{
			if (map == null)
			{
				Log.Warning("Tried to do explosion in a null map.");
				return;
			}
			if (damAmount < 0)
			{
				damAmount = damType.defaultDamage;
				armorPenetration = damType.defaultArmorPenetration;
				if (damAmount < 0)
				{
					Log.ErrorOnce("Attempted to trigger an explosion without defined damage", 91094882);
					damAmount = 1;
				}
			}
			if (armorPenetration < 0f)
			{
				armorPenetration = (float)damAmount * 0.015f;
			}
			Explosion obj = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
			IntVec3? needLOSToCell = null;
			IntVec3? needLOSToCell2 = null;
			if (direction.HasValue)
			{
				CalculateNeededLOSToCells(center, map, direction.Value, out needLOSToCell, out needLOSToCell2);
			}
			obj.radius = radius;
			obj.damType = damType;
			obj.instigator = instigator;
			obj.damAmount = damAmount;
			obj.armorPenetration = armorPenetration;
			obj.weapon = weapon;
			obj.projectile = projectile;
			obj.intendedTarget = intendedTarget;
			obj.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
			obj.preExplosionSpawnChance = preExplosionSpawnChance;
			obj.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
			obj.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
			obj.postExplosionSpawnChance = postExplosionSpawnChance;
			obj.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
			obj.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
			obj.chanceToStartFire = chanceToStartFire;
			obj.damageFalloff = damageFalloff;
			obj.needLOSToCell1 = needLOSToCell;
			obj.needLOSToCell2 = needLOSToCell2;
			obj.StartExplosion(explosionSound, ignoredThings);
		}

		private static void CalculateNeededLOSToCells(IntVec3 position, Map map, float direction, out IntVec3? needLOSToCell1, out IntVec3? needLOSToCell2)
		{
			needLOSToCell1 = null;
			needLOSToCell2 = null;
			if (position.CanBeSeenOverFast(map))
			{
				return;
			}
			direction = GenMath.PositiveMod(direction, 360f);
			IntVec3 intVec = position;
			intVec.z++;
			IntVec3 intVec2 = position;
			intVec2.z--;
			IntVec3 intVec3 = position;
			intVec3.x--;
			IntVec3 intVec4 = position;
			intVec4.x++;
			if (direction < 90f)
			{
				if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
				{
					needLOSToCell1 = intVec3;
				}
				if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
				{
					needLOSToCell2 = intVec;
				}
			}
			else if (direction < 180f)
			{
				if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
				{
					needLOSToCell1 = intVec;
				}
				if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
				{
					needLOSToCell2 = intVec4;
				}
			}
			else if (direction < 270f)
			{
				if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
				{
					needLOSToCell1 = intVec4;
				}
				if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
				{
					needLOSToCell2 = intVec2;
				}
			}
			else
			{
				if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
				{
					needLOSToCell1 = intVec2;
				}
				if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
				{
					needLOSToCell2 = intVec3;
				}
			}
		}

		public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius)
		{
			GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, Find.CurrentMap, radius).ToList());
		}

		public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		{
			Room room = exploder.GetRoom();
			for (int i = 0; i < PawnNotifyCellCount; i++)
			{
				IntVec3 c = exploder.Position + GenRadial.RadialPattern[i];
				if (!c.InBounds(exploder.Map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(exploder.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Pawn pawn = thingList[j] as Pawn;
					if (pawn != null && (int)pawn.RaceProps.intelligence >= 2 && (onlyFaction == null || pawn.Faction == onlyFaction) && damage.ExternalViolenceFor(pawn))
					{
						Room room2 = pawn.GetRoom();
						if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(exploder.Position, pawn.Position, exploder.Map, skipFirstCell: true)))
						{
							pawn.mindState.Notify_DangerousExploderAboutToExplode(exploder);
						}
					}
				}
			}
		}
	}
}
