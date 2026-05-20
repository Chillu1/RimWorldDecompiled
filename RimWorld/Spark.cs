using Verse;

namespace RimWorld;

public class Spark : Projectile
{
	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		Map map = base.Map;
		base.Impact(hitThing, blockedByShield);
		Thing instigator = launcher;
		if (launcher is Fire fire)
		{
			instigator = fire.instigator;
		}
		FireUtility.TryStartFireIn(base.Position, map, 0.1f, instigator);
	}
}
