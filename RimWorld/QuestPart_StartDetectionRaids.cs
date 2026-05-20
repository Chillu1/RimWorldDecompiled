using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_StartDetectionRaids : QuestPart
	{
		public string inSignal;

		public FloatRange? delayRangeHours;

		public WorldObject worldObject;

		public int? firstRaidDelayTicks;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				TimedDetectionRaids component = worldObject.GetComponent<TimedDetectionRaids>();
				int ticks = 240000;
				if (delayRangeHours.HasValue)
				{
					ticks = (int)(delayRangeHours.Value.RandomInRange * 2500f);
					component.delayRangeHours = delayRangeHours.Value;
				}
				if (firstRaidDelayTicks.HasValue)
				{
					ticks = firstRaidDelayTicks.Value;
				}
				component.StartDetectionCountdown(ticks);
				component.SetNotifiedSilently();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref worldObject, "worldObject");
			Scribe_Values.Look(ref delayRangeHours, "delayRangeHours");
			Scribe_Values.Look(ref firstRaidDelayTicks, "firstRaidDelayTicks");
		}
	}
}
