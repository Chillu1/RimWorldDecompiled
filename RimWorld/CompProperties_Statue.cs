using Verse;

namespace RimWorld;

public class CompProperties_Statue : CompProperties_Art
{
	public GraphicData statueBaseGraphic;

	public CompProperties_Statue()
	{
		compClass = typeof(CompStatue);
	}
}
