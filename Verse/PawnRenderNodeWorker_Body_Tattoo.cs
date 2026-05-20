namespace Verse;

public class PawnRenderNodeWorker_Body_Tattoo : PawnRenderNodeWorker_Body
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!ModsConfig.IdeologyActive || parms.pawn.style?.BodyTattoo == null)
		{
			return false;
		}
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		if (!parms.pawn.style.BodyTattoo.visibleNorth && parms.facing == Rot4.North)
		{
			return false;
		}
		return true;
	}
}
