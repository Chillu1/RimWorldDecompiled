namespace RimWorld;

public class CompPowerPlantAncientReactor : CompPowerPlant
{
	private CompHackable hackableComp;

	protected override float DesiredPowerOutput
	{
		get
		{
			if (!HackableComp.IsHacked)
			{
				return base.DesiredPowerOutput;
			}
			return 0f;
		}
	}

	public CompHackable HackableComp => hackableComp ?? (hackableComp = parent.GetComp<CompHackable>());
}
