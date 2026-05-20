namespace Verse;

public class HediffComp_DisappearsOnDeath : HediffComp
{
	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		base.Notify_PawnDied(dinfo, culprit);
		base.Pawn.health.RemoveHediff(parent);
	}
}
