namespace Verse;

public class Hediff_LightExposure : Hediff
{
	private const float ExposurePerSecond_Lit = 0.4f;

	private const float ExposurePerSecond_Unlit = -0.25f;

	public override bool ShouldRemove => false;

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Light exposure"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (pawn.SpawnedOrAnyParentSpawned && pawn.IsHashIntervalTick(60, delta) && !pawn.Dead)
		{
			bool flag = pawn.MapHeld.glowGrid.PsychGlowAt(pawn.PositionHeld) != PsychGlow.Dark;
			Severity += (flag ? 0.4f : (-0.25f));
		}
	}
}
