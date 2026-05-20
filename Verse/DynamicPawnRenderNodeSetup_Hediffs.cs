using System;
using System.Collections.Generic;

namespace Verse;

public class DynamicPawnRenderNodeSetup_Hediffs : DynamicPawnRenderNodeSetup
{
	public override bool HumanlikeOnly => false;

	public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
	{
		List<Hediff> list = pawn.health?.hediffSet?.hediffs;
		if (list == null)
		{
			yield break;
		}
		foreach (Hediff h in list)
		{
			if (!h.Visible || !h.def.HasDefinedGraphicProperties)
			{
				continue;
			}
			foreach (PawnRenderNodeProperties renderNodeProperty in h.def.RenderNodeProperties)
			{
				if (tree.ShouldAddNodeToTree(renderNodeProperty))
				{
					PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, tree);
					pawnRenderNode.hediff = h;
					pawnRenderNode.bodyPart = h.Part;
					yield return (node: pawnRenderNode, parent: null);
				}
			}
		}
	}
}
