using UnityEngine;

namespace Verse;

public class PawnRenderSubWorker
{
	public virtual bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
	{
		return true;
	}

	public virtual void EditMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms, ref MaterialPropertyBlock block)
	{
	}

	public virtual void EditMaterial(PawnRenderNode node, PawnDrawParms parms, ref Material material)
	{
	}

	public virtual void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
	{
	}

	public virtual void TransformLayer(PawnRenderNode node, PawnDrawParms parms, ref float layer)
	{
	}

	public virtual void TransformRotation(PawnRenderNode node, PawnDrawParms parms, ref Quaternion rotation)
	{
	}

	public virtual void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
	{
	}
}
