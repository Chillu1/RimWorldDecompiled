namespace Verse;

public class Projectile_SpawnsThing : Projectile
{
	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		Map map = base.Map;
		base.Impact(hitThing, blockedByShield);
		IntVec3 loc = base.Position;
		if (def.projectile.tryAdjacentFreeSpaces && base.Position.GetFirstBuilding(map) != null)
		{
			foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(base.Position))
			{
				if (item.GetFirstBuilding(map) == null && item.Standable(map))
				{
					loc = item;
					break;
				}
			}
		}
		Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(def.projectile.spawnsThingDef), loc, map);
		if (thing.def.CanHaveFaction)
		{
			thing.SetFaction(base.Launcher.Faction);
		}
	}
}
