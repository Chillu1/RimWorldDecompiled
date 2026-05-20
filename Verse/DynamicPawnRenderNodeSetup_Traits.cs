using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class DynamicPawnRenderNodeSetup_Traits : DynamicPawnRenderNodeSetup
{
	public override bool HumanlikeOnly => true;

	public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
	{
		if (pawn.story?.traits == null)
		{
			yield break;
		}
		foreach (Trait t in pawn.story.traits.allTraits)
		{
			if (t.Suppressed || !t.CurrentData.HasDefinedGraphicProperties)
			{
				continue;
			}
			foreach (PawnRenderNodeProperties renderNodeProperty in t.CurrentData.RenderNodeProperties)
			{
				if (tree.ShouldAddNodeToTree(renderNodeProperty))
				{
					PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, tree);
					pawnRenderNode.trait = t;
					yield return (node: pawnRenderNode, parent: null);
				}
			}
		}
	}
}
