using RimWorld;

namespace Verse;

public class PawnRenderNodeWorker_OverlayFirefoam : PawnRenderNodeWorker_Overlay
{
	protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
	{
		return pawn.Drawer.renderer.FirefoamOverlays;
	}

	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return parms.pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam;
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms) && parms.rotDrawMode == RotDrawMode.Fresh)
		{
			return parms.pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam;
		}
		return false;
	}
}
