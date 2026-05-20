namespace RimWorld;

public class CompProperties_CompAnalyzableUnlockResearch : CompProperties_Analyzable
{
	public int analysisID;

	public bool requiresMechanitor;

	public CompProperties_CompAnalyzableUnlockResearch()
	{
		compClass = typeof(CompAnalyzableUnlockResearch);
	}
}
