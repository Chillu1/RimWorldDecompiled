namespace Verse;

public class Hediff_DeathRefusal_CreepJoiner : Hediff_DeathRefusal
{
	public override int MaxUses => 4;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		usesLeft = MaxUses;
	}
}
