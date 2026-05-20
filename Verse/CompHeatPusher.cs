namespace Verse;

public class CompHeatPusher : ThingComp
{
	private const int HeatPushInterval = 60;

	public bool enabled = true;

	public CompProperties_HeatPusher Props => (CompProperties_HeatPusher)props;

	public virtual bool ShouldPushHeatNow
	{
		get
		{
			if (!parent.SpawnedOrAnyParentSpawned)
			{
				return false;
			}
			CompProperties_HeatPusher compProperties_HeatPusher = Props;
			float ambientTemperature = parent.AmbientTemperature;
			if (enabled && ambientTemperature < compProperties_HeatPusher.heatPushMaxTemperature)
			{
				return ambientTemperature > compProperties_HeatPusher.heatPushMinTemperature;
			}
			return false;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (parent.IsHashIntervalTick(60) && ShouldPushHeatNow)
		{
			GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond);
		}
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		if (ShouldPushHeatNow)
		{
			GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 4.1666665f);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref enabled, "enabled", defaultValue: true);
	}
}
