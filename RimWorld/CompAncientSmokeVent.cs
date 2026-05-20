namespace RimWorld;

public class CompAncientSmokeVent : CompAncientVent
{
	protected override void ToggleIndividualVent(bool on)
	{
		parent.GetComp<CompFleckEmitterLongTerm>().Enabled = on;
	}
}
