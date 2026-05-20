using Verse;

namespace RimWorld;

public class EffecterOnDeath : ThingComp
{
	private Effecter effecter;

	public CompProperties_EffecterOnDeath Props => (CompProperties_EffecterOnDeath)props;

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		Props.effecterDef.SpawnMaintained(parent.PositionHeld, prevMap);
		base.Notify_Killed(prevMap, dinfo);
	}
}
