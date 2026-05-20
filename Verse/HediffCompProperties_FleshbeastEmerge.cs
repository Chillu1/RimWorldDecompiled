namespace Verse;

public class HediffCompProperties_FleshbeastEmerge : HediffCompProperties
{
	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterText;

	public HediffCompProperties_FleshbeastEmerge()
	{
		compClass = typeof(HediffComp_FleshbeastEmerge);
	}
}
