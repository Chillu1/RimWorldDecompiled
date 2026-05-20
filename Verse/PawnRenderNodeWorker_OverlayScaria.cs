using RimWorld;

namespace Verse;

public class PawnRenderNodeWorker_OverlayScaria : PawnRenderNodeWorker_Overlay
{
	protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
	{
		return pawn.Drawer.renderer.ScariaSoreDrawer;
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
