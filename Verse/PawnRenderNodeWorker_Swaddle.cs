using RimWorld;

namespace Verse;

public class PawnRenderNodeWorker_Swaddle : PawnRenderNodeWorker_Body
{
	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return parms.pawn.DevelopmentalStage.Baby();
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			return parms.pawn.SwaddleBaby();
		}
		return false;
	}
}
