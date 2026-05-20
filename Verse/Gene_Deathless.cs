namespace Verse;

public class Gene_Deathless : Gene
{
	public int lastSkillReductionTick = -99999;

	public override void PostRemove()
	{
		base.PostRemove();
		pawn.health.CheckForStateChange(null, null);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastSkillReductionTick, "lastSkillReductionTick", -99999);
	}
}
