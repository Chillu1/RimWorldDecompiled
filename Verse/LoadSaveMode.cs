namespace Verse
{
	public enum LoadSaveMode : byte
	{
		Inactive,
		Saving,
		LoadingVars,
		ResolvingCrossRefs,
		PostLoadInit
	}
}
