using Verse;

namespace RimWorld;

[DefOf]
public static class PawnRenderNodeTagDefOf
{
	public static PawnRenderNodeTagDef Head;

	public static PawnRenderNodeTagDef Body;

	public static PawnRenderNodeTagDef ApparelHead;

	public static PawnRenderNodeTagDef ApparelBody;

	static PawnRenderNodeTagDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PawnRenderNodeTagDefOf));
	}
}
