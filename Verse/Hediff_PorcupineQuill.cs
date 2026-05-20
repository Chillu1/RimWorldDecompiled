namespace Verse;

public class Hediff_PorcupineQuill : HediffWithComps
{
	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		if (pawn != null && pawn.RaceProps != null && !pawn.RaceProps.IsFlesh)
		{
			pawn.health.RemoveHediff(this);
		}
	}
}
