using Verse;

namespace RimWorld;

public class FlameThrower : Projectile
{
	private static FloatRange FireSizeRange = new FloatRange(0.55f, 0.85f);

	private static float ChanceToMakeFilth = 0.25f;

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		IntVec3 position = base.Position;
		Map map = base.Map;
		base.Impact(hitThing, blockedByShield);
		Find.BattleLog.Add(new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef));
		if (hitThing != null && (!(hitThing is Pawn t) || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(t))))
		{
			hitThing.TryAttachFire(FireSizeRange.RandomInRange, launcher);
		}
		FireUtility.TryStartFireIn(position, map, FireSizeRange.RandomInRange, launcher);
		if (Rand.Chance(ChanceToMakeFilth))
		{
			FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Fuel);
		}
	}
}
