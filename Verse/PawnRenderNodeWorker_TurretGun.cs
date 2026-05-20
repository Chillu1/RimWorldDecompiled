using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_TurretGun : PawnRenderNodeWorker
{
	public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Quaternion result = base.RotationFor(node, parms);
		if (node is PawnRenderNode_TurretGun pawnRenderNode_TurretGun)
		{
			result *= pawnRenderNode_TurretGun.turretComp.curRotation.ToQuat();
		}
		return result;
	}
}
