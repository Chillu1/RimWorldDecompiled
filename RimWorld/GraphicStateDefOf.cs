using Verse;

namespace RimWorld;

[DefOf]
public static class GraphicStateDefOf
{
	public static GraphicStateDef Swimming;

	public static GraphicStateDef Stationary;

	static GraphicStateDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(GraphicStateDefOf));
	}
}
