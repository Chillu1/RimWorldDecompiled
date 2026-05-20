using System;
using System.Collections.Generic;

namespace Verse;

public class DynamicPawnRenderNodeSetup_Mutants : DynamicPawnRenderNodeSetup
{
	public override bool HumanlikeOnly => false;

	public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
	{
		if (!pawn.IsMutant || !pawn.mutant.Def.HasDefinedGraphicProperties)
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties renderNodeProperty in pawn.mutant.Def.RenderNodeProperties)
		{
			if (tree.ShouldAddNodeToTree(renderNodeProperty))
			{
				yield return (node: (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, tree), parent: null);
			}
		}
	}
}
