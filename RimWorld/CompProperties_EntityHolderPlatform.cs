using Verse;

namespace RimWorld;

public class CompProperties_EntityHolderPlatform : CompProperties_EntityHolder
{
	[NoTranslate]
	public string untetheredGraphicTexPath;

	[NoTranslate]
	public string tilingChainTexPath;

	[NoTranslate]
	public string baseChainFastenerTexPath;

	[NoTranslate]
	public string targetChainFastenerTexPath;

	public SoundDef entityLungeSoundHi;

	public SoundDef entityLungeSoundLow;

	public CompProperties_EntityHolderPlatform()
	{
		compClass = typeof(CompEntityHolderPlatform);
	}
}
