namespace Verse;

public class Verb_ArcSprayProjectile : Verb_ArcSpray
{
	protected override void HitCell(IntVec3 cell)
	{
		base.HitCell(cell);
		Map map = caster.Map;
		if (GenSight.LineOfSight(caster.Position, cell, map, skipFirstCell: true))
		{
			((Projectile)GenSpawn.Spawn(verbProps.defaultProjectile, caster.Position, map)).Launch(caster, caster.DrawPos, cell, cell, ProjectileHitFlags.IntendedTarget, preventFriendlyFire);
		}
	}
}
