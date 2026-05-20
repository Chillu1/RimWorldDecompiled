using Verse;

namespace RimWorld
{
	public class StatWorker_MechEnergyLossPerHP : StatWorker
	{
		public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			return base.ValueToString(val * 100f, finalized, numberSense);
		}
	}
}
