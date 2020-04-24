using RimWorld;

namespace Verse
{
	public class TriggerUnfogged : Thing
	{
		public string signalTag;

		private bool everFogged;

		public override void Tick()
		{
			if (base.Spawned)
			{
				if (base.Position.Fogged(base.Map))
				{
					everFogged = true;
				}
				else if (everFogged)
				{
					Activated();
				}
				else
				{
					Destroy();
				}
			}
		}

		public void Activated()
		{
			Find.SignalManager.SendSignal(new Signal(signalTag));
			if (!base.Destroyed)
			{
				Destroy();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref signalTag, "signalTag");
			Scribe_Values.Look(ref everFogged, "everFogged", defaultValue: false);
		}
	}
}
