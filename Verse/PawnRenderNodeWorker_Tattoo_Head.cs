namespace Verse;

public class PawnRenderNodeWorker_Tattoo_Head : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!ModsConfig.IdeologyActive || parms.pawn.style?.FaceTattoo == null)
		{
			return false;
		}
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		if (!parms.pawn.style.FaceTattoo.visibleNorth && parms.facing == Rot4.North)
		{
			return false;
		}
		return true;
	}
}
