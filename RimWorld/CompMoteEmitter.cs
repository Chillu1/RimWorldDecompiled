using Verse;

namespace RimWorld
{
	public class CompMoteEmitter : ThingComp
	{
		public int ticksSinceLastEmitted;

		protected Mote mote;

		private CompProperties_MoteEmitter Props => (CompProperties_MoteEmitter)props;

		public override void CompTick()
		{
			CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
			if (comp != null && !comp.PowerOn)
			{
				return;
			}
			CompSendSignalOnCountdown comp2 = parent.GetComp<CompSendSignalOnCountdown>();
			if (comp2 != null && comp2.ticksLeft <= 0)
			{
				return;
			}
			CompInitiatable comp3 = parent.GetComp<CompInitiatable>();
			if (comp3 != null && !comp3.Initiated)
			{
				return;
			}
			if (Props.emissionInterval != -1 && !Props.maintain)
			{
				if (ticksSinceLastEmitted >= Props.emissionInterval)
				{
					Emit();
					ticksSinceLastEmitted = 0;
				}
				else
				{
					ticksSinceLastEmitted++;
				}
			}
			else if (mote == null)
			{
				Emit();
			}
			if (Props.maintain && mote != null)
			{
				mote.Maintain();
			}
		}

		protected void Emit()
		{
			mote = MoteMaker.MakeStaticMote(parent.DrawPos + Props.offset, parent.Map, Props.mote);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksSinceLastEmitted, ((Props.saveKeysPrefix != null) ? (Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0);
		}
	}
}
