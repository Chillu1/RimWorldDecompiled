using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_AttachmentBody : PawnRenderNodeWorker_Body
{
	public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Vector3 vector = base.ScaleFor(node, parms);
		Vector2 bodyGraphicScale = parms.pawn.story.bodyType.bodyGraphicScale;
		return vector * ((bodyGraphicScale.x + bodyGraphicScale.y) / 2f);
	}
}
