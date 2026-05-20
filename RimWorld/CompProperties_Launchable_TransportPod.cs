namespace RimWorld;

public class CompProperties_Launchable_TransportPod : CompProperties_Launchable
{
	public bool requiresFuelingPort = true;

	public CompProperties_Launchable_TransportPod()
	{
		compClass = typeof(CompLaunchable_TransportPod);
	}
}
