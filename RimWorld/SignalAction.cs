using Verse;

namespace RimWorld
{
	public abstract class SignalAction : Thing
	{
		public string signalTag;

		public override void Notify_SignalReceived(Signal signal)
		{
			base.Notify_SignalReceived(signal);
			if (signal.tag == signalTag)
			{
				DoAction(signal.args);
				if (!base.Destroyed)
				{
					Destroy();
				}
			}
		}

		protected abstract void DoAction(SignalArgs args);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref signalTag, "signalTag");
		}
	}
}
