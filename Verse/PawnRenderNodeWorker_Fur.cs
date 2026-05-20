namespace Verse;

public class PawnRenderNodeWorker_Fur : PawnRenderNodeWorker_Body
{
	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return parms.pawn.story?.furDef != null;
	}
}
