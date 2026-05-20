using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Carried : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		if (parms.Portrait || parms.pawn.Dead || !parms.pawn.Spawned)
		{
			return false;
		}
		return true;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 result = base.OffsetFor(node, parms, out pivot);
		result.y = AltitudeFor(node, parms);
		return result;
	}

	public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
	{
		requests.Add(new PawnGraphicDrawRequest(node));
	}

	public override void PostDraw(PawnRenderNode node, PawnDrawParms parms, Mesh mesh, Matrix4x4 matrix)
	{
		Vector3 pivot;
		Vector3 vector = parms.matrix.Position() + OffsetFor(node, parms, out pivot);
		if (parms.pawn.carryTracker?.CarriedThing != null)
		{
			PawnRenderUtility.DrawCarriedThing(parms.pawn, vector, parms.pawn.carryTracker.CarriedThing);
		}
		else
		{
			PawnRenderUtility.DrawEquipmentAndApparelExtras(parms.pawn, vector, parms.facing, parms.flags);
		}
	}
}
