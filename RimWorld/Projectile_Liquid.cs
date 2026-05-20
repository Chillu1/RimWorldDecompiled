using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Projectile_Liquid : Projectile
{
	private Material materialResolved;

	public override Material DrawMat => materialResolved;

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		DoImpact(hitThing, base.Position);
		if (!blockedByShield && !def.projectile.soundImpact.NullOrUndefined())
		{
			def.projectile.soundImpact.PlayOneShot(SoundInfo.InMap(this));
		}
		for (int i = 0; i < def.projectile.numExtraHitCells; i++)
		{
			IntVec3 intVec = base.Position + GenAdj.AdjacentCellsAndInside[i];
			if (intVec.InBounds(base.Map))
			{
				DoImpact(hitThing, intVec);
			}
		}
		base.Impact(hitThing, blockedByShield);
	}

	private void DoImpact(Thing hitThing, IntVec3 cell)
	{
		if (def.projectile.filth != null && def.projectile.filthCount.TrueMax > 0 && Rand.Chance(def.projectile.filthChance) && !cell.Filled(base.Map))
		{
			FilthMaker.TryMakeFilth(cell, base.Map, def.projectile.filth, def.projectile.filthCount.RandomInRange);
		}
		TerrainDef terrain = cell.GetTerrain(base.Map);
		Frame firstThing = cell.GetFirstThing<Frame>(base.Map);
		Plant plant = cell.GetPlant(base.Map);
		if (def.projectile.spawnTerrain != null && Rand.Chance(def.projectile.terrainChance) && (def.projectile.terrainReplacesFloors || terrain.natural || !terrain.IsFloor) && (def.projectile.terrainReplacesFloors || !(firstThing?.def.entityDefToBuild is TerrainDef)) && (plant == null || plant.def.plant.treeCategory != TreeCategory.Super) && GenConstruct.CanBuildOnTerrain(def.projectile.spawnTerrain, cell, base.Map, Rot4.North))
		{
			firstThing?.Destroy();
			base.Map.terrainGrid.SetTerrain(cell, def.projectile.spawnTerrain);
		}
		List<Thing> thingList = cell.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (!(thing is Mote) && !(thing is Filth) && thing != hitThing && (!preventFriendlyFire || !(thing is Pawn a) || a.HostileTo(launcher)))
			{
				Find.BattleLog.Add(new BattleLogEntry_RangedImpact(launcher, thing, thing, equipmentDef, def, targetCoverDef));
				DamageInfo dinfo = new DamageInfo(base.DamageDef, def.projectile.GetDamageAmount(null), def.projectile.GetArmorPenetration(), -1f, launcher);
				thing.TakeDamage(dinfo);
			}
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (materialResolved == null)
		{
			materialResolved = def.DrawMatSingle;
		}
		base.DrawAt(drawLoc, flip);
	}
}
