using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_DrawAdditionalGraphics : CompProperties
{
	public List<GraphicData> graphics;

	public CompProperties_DrawAdditionalGraphics()
	{
		compClass = typeof(CompDrawAdditionalGraphics);
	}
}
