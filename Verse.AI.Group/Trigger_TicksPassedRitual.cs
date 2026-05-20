using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_TicksPassedRitual : Trigger
	{
		private int duration = 100;

		private RitualStage stage;

		protected TriggerData_TicksPassedRitual Data => (TriggerData_TicksPassedRitual)data;

		public Trigger_TicksPassedRitual(int tickLimit, RitualStage stage)
		{
			data = new TriggerData_TicksPassedRitual();
			duration = tickLimit;
			this.stage = stage;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				LordJob_Ritual ritual = lord.LordJob as LordJob_Ritual;
				TriggerData_TicksPassedRitual triggerData_TicksPassedRitual = Data;
				triggerData_TicksPassedRitual.ticksPassed += stage.ProgressPerTick(ritual);
				return triggerData_TicksPassedRitual.ticksPassed > (float)duration;
			}
			return false;
		}

		public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
		{
			if (!transition.sources.Contains(previousToil))
			{
				Data.ticksPassed = 0f;
			}
		}
	}
}
