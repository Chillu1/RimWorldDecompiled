using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompPowerPlant : CompPowerTrader
{
	protected CompRefuelable refuelableComp;

	protected CompBreakdownable breakdownableComp;

	protected CompAutoPowered autoPoweredComp;

	protected CompToxifier toxifier;

	private Sustainer sustainerProducingPower;

	protected virtual float DesiredPowerOutput => 0f - base.Props.PowerConsumption;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		refuelableComp = parent.GetComp<CompRefuelable>();
		breakdownableComp = parent.GetComp<CompBreakdownable>();
		autoPoweredComp = parent.GetComp<CompAutoPowered>();
		toxifier = parent.GetComp<CompToxifier>();
		if (base.Props.PowerConsumption < 0f && !parent.IsBrokenDown() && FlickUtility.WantsToBeOn(parent))
		{
			base.PowerOn = true;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
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

	public virtual void UpdateDesiredPowerOutput()
	{
		if ((breakdownableComp != null && breakdownableComp.BrokenDown) || (refuelableComp != null && !refuelableComp.HasFuel) || (flickableComp != null && !flickableComp.SwitchIsOn) || (autoPoweredComp != null && !autoPoweredComp.WantsToBeOn) || (toxifier != null && !toxifier.CanPolluteNow) || !base.PowerOn)
		{
			base.PowerOutput = 0f;
		}
		else
		{
			base.PowerOutput = DesiredPowerOutput;
		}
	}
}
