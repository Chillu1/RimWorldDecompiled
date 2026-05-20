namespace RimWorld;

public class CompProperties_ObeliskDeactivationInteractor : CompProperties_Interactable
{
	public int shardsRequired = 2;

	public CompProperties_ObeliskDeactivationInteractor()
	{
		compClass = typeof(CompObeliskDeactivationInteractor);
	}
}
