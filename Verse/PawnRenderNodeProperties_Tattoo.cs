using RimWorld;

namespace Verse;

public class PawnRenderNodeProperties_Tattoo : PawnRenderNodeProperties
{
	public override void ResolveReferences()
	{
		skipFlag = RenderSkipFlagDefOf.Tattoos;
	}
}
