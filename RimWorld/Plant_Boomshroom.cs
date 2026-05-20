using Verse;

namespace RimWorld;

public class Plant_Boomshroom : Plant
{
	public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
	{
		if (!base.Destroyed && HarvestableNow)
		{
			GenExplosion.DoExplosion(base.Position, base.Map, 4.9f, DamageDefOf.Flame, this);
		}
		base.Kill(dinfo, exactCulprit);
	}
}
