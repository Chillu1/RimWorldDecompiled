namespace RimWorld;

public class JobDriver_LayDownDeactivated : JobDriver_LayDown
{
	public override bool CanSleep => true;

	public override bool CanRest => false;

	public override bool LookForOtherJobs => false;
}
