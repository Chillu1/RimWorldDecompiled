using Verse;

namespace RimWorld;

public class CompHeatPusherEffecter : ThingComp
{
	private Effecter effecter;

	private CompHeatPusher pusherComp;

	public CompProperties_HeatPusherEffecter Props => (CompProperties_HeatPusherEffecter)props;

	public bool ShouldEmit => pusherComp.ShouldPushHeatNow;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		pusherComp = parent.GetComp<CompHeatPusher>();
	}

	public override void CompTick()
	{
		if (ShouldEmit && effecter == null)
		{
			effecter = Props.effecterDef.SpawnAttached(parent, parent.Map);
		}
		else if (!ShouldEmit && effecter != null)
		{
			effecter.Cleanup();
			effecter = null;
		}
		effecter?.EffectTick(parent, parent);
	}
}
