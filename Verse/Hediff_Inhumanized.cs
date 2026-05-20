namespace Verse;

public class Hediff_Inhumanized : Hediff
{
	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModsConfig.AnomalyActive)
		{
			pawn.health.RemoveHediff(this);
			return;
		}
		base.PostAdd(dinfo);
		Find.StudyManager.UpdateStudiableCache(pawn, pawn.MapHeld);
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		Find.StudyManager.UpdateStudiableCache(pawn, pawn.MapHeld);
	}
}
