namespace RimWorld;

public class CompCauseGameCondition_AncientReactor : CompCauseGameCondition
{
	private CompHackable hackableComp;

	public override bool Active => !HackableComp.IsHacked;

	public CompHackable HackableComp => hackableComp ?? (hackableComp = parent.GetComp<CompHackable>());
}
