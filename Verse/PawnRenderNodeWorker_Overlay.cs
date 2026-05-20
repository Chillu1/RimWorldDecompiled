using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public abstract class PawnRenderNodeWorker_Overlay : PawnRenderNodeWorker
{
	protected abstract PawnOverlayDrawer OverlayDrawer(Pawn pawn);

	public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
	{
		return false;
	}

	public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
	{
		Mesh mesh = node.GetMesh(parms);
		if (mesh != null)
		{
			requests.Add(new PawnGraphicDrawRequest(node, mesh));
		}
	}

	public override void PostDraw(PawnRenderNode node, PawnDrawParms parms, Mesh mesh, Matrix4x4 matrix)
	{
		OverlayDrawer(parms.pawn)?.RenderPawnOverlay(matrix, mesh, node.Props.overlayLayer, parms, node.Props.overlayOverApparel);
	}
}
