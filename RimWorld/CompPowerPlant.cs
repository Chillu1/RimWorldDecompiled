namespace RimWorld
{
	public class CompPowerPlant : CompPowerTrader
	{
		protected CompRefuelable refuelableComp;

		protected CompBreakdownable breakdownableComp;

		protected virtual float DesiredPowerOutput => 0f - base.Props.basePowerConsumption;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			refuelableComp = parent.GetComp<CompRefuelable>();
			breakdownableComp = parent.GetComp<CompBreakdownable>();
			if (base.Props.basePowerConsumption < 0f && !parent.IsBrokenDown() && FlickUtility.WantsToBeOn(parent))
			{
				base.PowerOn = true;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			UpdateDesiredPowerOutput();
		}

		public void UpdateDesiredPowerOutput()
		{
			if ((breakdownableComp != null && breakdownableComp.BrokenDown) || (refuelableComp != null && !refuelableComp.HasFuel) || (flickableComp != null && !flickableComp.SwitchIsOn) || !base.PowerOn)
			{
				base.PowerOutput = 0f;
			}
			else
			{
				base.PowerOutput = DesiredPowerOutput;
			}
		}
	}
}
