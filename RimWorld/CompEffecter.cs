using Verse;

namespace RimWorld;

public class CompEffecter : ThingComp
{
	private Effecter effecter;

	public CompProperties_EffecterBase Props => (CompProperties_EffecterBase)props;

	protected virtual bool ShouldShowEffecter()
	{
		if (parent.Spawned)
		{
			return parent.MapHeld == Find.CurrentMap;
		}
		return false;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (ShouldShowEffecter())
		{
			if (effecter == null)
			{
				effecter = Props.effecterDef.SpawnAttached(parent, parent.MapHeld);
			}
			effecter?.EffectTick(parent, parent);
		}
		else
		{
			effecter?.Cleanup();
			effecter = null;
		}
	}
}
