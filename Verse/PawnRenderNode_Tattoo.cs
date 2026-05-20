using UnityEngine;

namespace Verse;

public abstract class PawnRenderNode_Tattoo : PawnRenderNode
{
	protected const float TattooOpacity = 0.8f;

	public PawnRenderNode_Tattoo(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Color ColorFor(Pawn pawn)
	{
		Color result = base.ColorFor(pawn);
		result.a *= 0.8f;
		return result;
	}
}
