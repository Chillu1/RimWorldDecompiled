using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompPowerPlant : CompPowerTrader
	{
		protected CompRefuelable refuelableComp;

		protected CompBreakdownable breakdownableComp;

		private Sustainer sustainerProducingPower;

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

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (sustainerProducingPower != null && !sustainerProducingPower.Ended)
			{
				sustainerProducingPower.End();
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			UpdateDesiredPowerOutput();
			if (base.Props.soundAmbientProducingPower == null)
			{
				return;
			}
			if (base.PowerOutput > 0.01f)
			{
				if (sustainerProducingPower == null || sustainerProducingPower.Ended)
				{
					sustainerProducingPower = base.Props.soundAmbientProducingPower.TrySpawnSustainer(SoundInfo.InMap(parent));
				}
				sustainerProducingPower.Maintain();
			}
			else if (sustainerProducingPower != null)
			{
				sustainerProducingPower.End();
				sustainerProducingPower = null;
			}
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
