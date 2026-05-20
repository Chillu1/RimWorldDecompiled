using System;
using System.Collections.Generic;

namespace Verse;

public class DynamicPawnRenderNodeSetup_Genes : DynamicPawnRenderNodeSetup
{
	public override bool HumanlikeOnly => true;

	public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			yield break;
		}
		foreach (Gene g in pawn.genes.GenesListForReading)
		{
			if (!g.Active || !g.def.HasDefinedGraphicProperties)
			{
				continue;
			}
			foreach (PawnRenderNodeProperties renderNodeProperty in g.def.RenderNodeProperties)
			{
				if (tree.ShouldAddNodeToTree(renderNodeProperty))
				{
					PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, tree);
					pawnRenderNode.gene = g;
					yield return (node: pawnRenderNode, parent: null);
				}
			}
		}
	}
}
