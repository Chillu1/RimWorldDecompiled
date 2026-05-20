using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompFleckEmitter : ThingComp
	{
		public int ticksSinceLastEmitted;

		private CompProperties_FleckEmitter Props => (CompProperties_FleckEmitter)props;

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
			if ((comp3 == null || comp3.Initiated) && Props.emissionInterval != -1)
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
		}

		protected void Emit()
		{
			FleckMaker.Static(parent.DrawPos + Props.offset, parent.MapHeld, Props.fleck);
			if (!Props.soundOnEmission.NullOrUndefined())
			{
				Props.soundOnEmission.PlayOneShot(SoundInfo.InMap(parent));
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksSinceLastEmitted, ((Props.saveKeysPrefix != null) ? (Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0);
		}
	}
}
