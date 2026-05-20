using RimWorld;

namespace Verse;

public class TriggerContainerEmptied : Thing
{
	public string signalTag;

	public Thing container;

	protected override void Tick()
	{
		if (!base.Spawned || !this.IsHashIntervalTick(60))
		{
			return;
		}
		CompThingContainer compThingContainer = container?.TryGetComp<CompThingContainer>();
		if (compThingContainer == null)
		{
			Destroy();
		}
		else if (!compThingContainer.innerContainer.Any)
		{
			Find.SignalManager.SendSignal(new Signal(signalTag));
			if (!base.Destroyed)
			{
				Destroy();
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref signalTag, "signalTag");
		Scribe_References.Look(ref container, "container");
	}
}
