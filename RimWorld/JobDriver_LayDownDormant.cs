namespace RimWorld;

public class JobDriver_LayDownDormant : JobDriver_LayDown
{
	public override bool CanSleep => true;

	public override bool LookForOtherJobs => false;
}
