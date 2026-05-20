using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class DynamicPawnRenderNodeSetup_Apparel : DynamicPawnRenderNodeSetup
{
	private const int ApparelLayerShellNorth = 88;

	public override bool HumanlikeOnly => true;

	public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
	{
		if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
		{
			yield break;
		}
		Dictionary<PawnRenderNode, int> layerOffsets = new Dictionary<PawnRenderNode, int>();
		PawnRenderNode node;
		PawnRenderNode headApparelNode = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead, out node) ? node : null);
		PawnRenderNode node2;
		PawnRenderNode bodyApparelNode = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelBody, out node2) ? node2 : null);
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (!ShouldAddApparelNode(item))
			{
				continue;
			}
			foreach (var result in ProcessApparel(pawn, tree, item, headApparelNode, bodyApparelNode, layerOffsets))
			{
				if (result.node != null)
				{
					yield return result;
				}
				if (result.parent != null && !layerOffsets.TryAdd(result.parent, 1))
				{
					layerOffsets[result.parent]++;
				}
			}
		}
	}

	private static bool ShouldAddApparelNode(Apparel gear)
	{
		return !gear.def.IsWeapon;
	}

	private static IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> ProcessApparel(Pawn pawn, PawnRenderTree tree, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode, Dictionary<PawnRenderNode, int> layerOffsets)
	{
		if (ap.def.apparel.HasDefinedGraphicProperties)
		{
			foreach (PawnRenderNodeProperties renderNodeProperty in ap.def.apparel.RenderNodeProperties)
			{
				if (tree.ShouldAddNodeToTree(renderNodeProperty))
				{
					PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, tree);
					pawnRenderNode.apparel = ap;
					yield return (node: pawnRenderNode, parent: null);
				}
			}
		}
		if (!ApparelGraphicRecordGetter.TryGetGraphicApparel(ap, pawn.story.bodyType, pawn.Drawer.renderer.StatueColor.HasValue, out var _))
		{
			yield break;
		}
		PawnRenderNodeProperties pawnRenderNodeProperties = null;
		PawnRenderNode pawnRenderNode2 = null;
		DrawData drawData = ap.def.apparel.drawData;
		ApparelLayerDef lastLayer = ap.def.apparel.LastLayer;
		bool flag = lastLayer == ApparelLayerDefOf.Overhead || lastLayer == ApparelLayerDefOf.EyeCover;
		if (ap.def.apparel.parentTagDef != null && tree.TryGetNodeByTag(ap.def.apparel.parentTagDef, out var node))
		{
			pawnRenderNode2 = node;
			if (headApparelNode != null && pawnRenderNode2 == headApparelNode)
			{
				flag = true;
			}
			else if (bodyApparelNode != null && pawnRenderNode2 == bodyApparelNode)
			{
				flag = false;
			}
		}
		if (headApparelNode != null && flag)
		{
			if (pawnRenderNode2 == null)
			{
				pawnRenderNode2 = headApparelNode;
			}
			int valueOrDefault = layerOffsets.GetValueOrDefault(pawnRenderNode2, 0);
			pawnRenderNodeProperties = new PawnRenderNodeProperties
			{
				debugLabel = ap.def.defName,
				workerClass = typeof(PawnRenderNodeWorker_Apparel_Head),
				baseLayer = pawnRenderNode2.Props.baseLayer + (float)valueOrDefault,
				drawData = drawData,
				parentTagDef = ap.def.apparel.parentTagDef
			};
		}
		else if (bodyApparelNode != null)
		{
			if (pawnRenderNode2 == null)
			{
				pawnRenderNode2 = bodyApparelNode;
			}
			int valueOrDefault2 = layerOffsets.GetValueOrDefault(pawnRenderNode2, 0);
			pawnRenderNodeProperties = new PawnRenderNodeProperties
			{
				debugLabel = ap.def.defName,
				workerClass = typeof(PawnRenderNodeWorker_Apparel_Body),
				baseLayer = pawnRenderNode2.Props.baseLayer + (float)valueOrDefault2,
				drawData = drawData,
				parentTagDef = ap.def.apparel.parentTagDef
			};
			if (drawData == null && !ap.def.apparel.shellRenderedBehindHead)
			{
				if (lastLayer == ApparelLayerDefOf.Shell)
				{
					pawnRenderNodeProperties.drawData = DrawData.NewWithData(new DrawData.RotationalData(Rot4.North, 88f));
				}
				else if (ap.RenderAsPack())
				{
					pawnRenderNodeProperties.drawData = DrawData.NewWithData(new DrawData.RotationalData(Rot4.North, 93f), new DrawData.RotationalData(Rot4.South, -3f));
				}
			}
		}
		if (tree.ShouldAddNodeToTree(pawnRenderNodeProperties))
		{
			yield return (node: new PawnRenderNode_Apparel(pawn, pawnRenderNodeProperties, tree, ap), parent: pawnRenderNode2);
		}
		else
		{
			yield return (node: null, parent: pawnRenderNode2);
		}
	}
}
