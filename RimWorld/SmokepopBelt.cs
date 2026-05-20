using Verse;

namespace RimWorld;

public class SmokepopBelt : Apparel
{
	private float ApparelScorePerBeltRadius = 0.046f;

	public override void Notify_BulletImpactNearby(BulletImpactData impactData)
	{
		base.Notify_BulletImpactNearby(impactData);
		Pawn wearer = base.Wearer;
		if (wearer != null && !wearer.Dead && !impactData.bullet.DamageDef.isExplosive && impactData.bullet.Launcher is Building_Turret && impactData.bullet.Launcher != null && impactData.bullet.Launcher.HostileTo(base.Wearer) && !wearer.IsColonist && wearer.Spawned && !wearer.Position.AnyGas(wearer.Map, GasType.BlindSmoke))
		{
			Verb_SmokePop.Pop(this.TryGetComp<CompApparelReloadable>());
		}
	}

	public override float GetSpecialApparelScoreOffset()
	{
		return this.GetStatValue(StatDefOf.PackRadius) * ApparelScorePerBeltRadius;
	}
}
