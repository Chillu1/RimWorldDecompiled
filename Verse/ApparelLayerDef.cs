using RimWorld;

namespace Verse;

public class ApparelLayerDef : Def
{
	public int drawOrder;

	public bool IsUtilityLayer => this == ApparelLayerDefOf.Belt;
}
