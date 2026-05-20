namespace RimWorld;

public class CompHackableEffecter : CompEffecter
{
	private CompHackable hackable;

	protected override bool ShouldShowEffecter()
	{
		if (!base.ShouldShowEffecter())
		{
			return false;
		}
		if (hackable == null)
		{
			hackable = parent.GetComp<CompHackable>();
		}
		return hackable?.ShouldBeLitNow() ?? true;
	}
}
