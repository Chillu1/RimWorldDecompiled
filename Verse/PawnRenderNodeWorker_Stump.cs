namespace Verse;

public class PawnRenderNodeWorker_Stump : PawnRenderNodeWorker_Head
{
	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return parms.flags.FlagSet(PawnRenderFlags.HeadStump);
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			return parms.flags.FlagSet(PawnRenderFlags.HeadStump);
		}
		return false;
	}
}
