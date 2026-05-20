namespace Verse.Sound;

public class SoundParamSource_Underground : SoundParamSource
{
	public override string Label => "Underground";

	public override float ValueFor(Sample samp)
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap == null || currentMap.generatorDef?.isUnderground != true)
		{
			return 0f;
		}
		return 1f;
	}
}
