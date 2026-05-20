using RimWorld;

namespace Verse;

public class PawnRenderNodeWorker_OverlayShambler : PawnRenderNodeWorker_Overlay
{
	protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
	{
		return pawn.Drawer.renderer.ShamblerScarDrawer;
	}

	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return parms.pawn.IsShambler;
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms) && parms.pawn.IsShambler)
		{
			if (parms.pawn.mutant != null)
			{
				return parms.pawn.mutant.HasTurned;
			}
			return true;
		}
		return false;
	}
}
