namespace Verse;

public class Projectile_SpawnsPawn : Projectile
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
		GenSpawn.Spawn(PawnGenerator.GeneratePawn(def.projectile.spawnsPawnKind, base.Launcher.Faction), loc, map);
	}
}
