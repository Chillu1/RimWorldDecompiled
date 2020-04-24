using RimWorld;

namespace Verse
{
	public class CompHeatPusherPowered : CompHeatPusher
	{
		protected CompPowerTrader powerComp;

		protected CompRefuelable refuelableComp;

		protected CompBreakdownable breakdownableComp;

		protected override bool ShouldPushHeatNow
		{
			get
			{
				if (!base.ShouldPushHeatNow || !FlickUtility.WantsToBeOn(parent) || (powerComp != null && !powerComp.PowerOn) || (refuelableComp != null && !refuelableComp.HasFuel) || (breakdownableComp != null && breakdownableComp.BrokenDown))
				{
					return false;
				}
				return true;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			powerComp = parent.GetComp<CompPowerTrader>();
			refuelableComp = parent.GetComp<CompRefuelable>();
			breakdownableComp = parent.GetComp<CompBreakdownable>();
		}
	}
}
