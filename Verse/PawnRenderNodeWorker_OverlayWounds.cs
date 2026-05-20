using RimWorld;

namespace Verse;

public class PawnRenderNodeWorker_OverlayWounds : PawnRenderNodeWorker_Overlay
{
	protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
	{
		return pawn.Drawer.renderer.WoundOverlays;
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			return parms.rotDrawMode != RotDrawMode.Dessicated;
		}
		return false;
	}
}
