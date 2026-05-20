using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_OverlayStatus : PawnRenderNodeWorker_Overlay
{
	protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
	{
		return null;
	}

	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms) && !parms.Portrait)
		{
			return !parms.Cache;
		}
		return false;
	}

	public override void PostDraw(PawnRenderNode node, PawnDrawParms parms, Mesh mesh, Matrix4x4 matrix)
	{
		Vector3 pivot;
		Vector3 offset = OffsetFor(node, parms, out pivot);
		Quaternion quat = RotationFor(node, parms);
		if (node.Props.overlayLayer == PawnOverlayDrawer.OverlayLayer.Head)
		{
			offset += parms.pawn.Drawer.renderer.BaseHeadOffsetAt(Rot4.North);
		}
		parms.pawn.Drawer.renderer.StatusOverlays.RenderStatusOverlays(offset, quat, mesh);
	}
}
