using System;
using System.Collections.Generic;
using RimWorld;
using Unity.Burst;
using UnityEngine;

namespace Verse;

public class PawnRenderTree
{
	public Pawn pawn;

	public PawnRenderNode rootNode;

	private PawnDrawParms oldParms;

	public AnimationDef currentAnimation;

	public int animationStartTick = -99999;

	public Color? debugTint;

	private readonly List<PawnGraphicDrawRequest> drawRequests = new List<PawnGraphicDrawRequest>();

	private readonly Dictionary<PawnRenderNodeTagDef, PawnRenderNode> nodesByTag = new Dictionary<PawnRenderNodeTagDef, PawnRenderNode>();

	private readonly Dictionary<PawnRenderNode, List<PawnRenderNode>> nodeAncestors = new Dictionary<PawnRenderNode, List<PawnRenderNode>>();

	private readonly Dictionary<PawnRenderNodeTagDef, List<PawnRenderNode>> tmpChildTagNodes = new Dictionary<PawnRenderNodeTagDef, List<PawnRenderNode>>();

	private static readonly List<DynamicPawnRenderNodeSetup> dynamicNodeTypeInstances = new List<DynamicPawnRenderNodeSetup>();

	private const float ApparelLayerShellNorth = 88f;

	private readonly Queue<PawnRenderNode> nodeQueue = new Queue<PawnRenderNode>();

	public bool Resolved => rootNode != null;

	public Graphic BodyGraphic
	{
		get
		{
			TrySetupGraphIfNeeded();
			if (nodesByTag.TryGetValue(PawnRenderNodeTagDefOf.Body, out var value))
			{
				return value?.PrimaryGraphic;
			}
			return null;
		}
	}

	public Graphic HeadGraphic
	{
		get
		{
			TrySetupGraphIfNeeded();
			if (nodesByTag.TryGetValue(PawnRenderNodeTagDefOf.Head, out var value))
			{
				return value?.PrimaryGraphic;
			}
			return null;
		}
	}

	public int AnimationTick
	{
		get
		{
			if (currentAnimation == null || currentAnimation.durationTicks <= 0)
			{
				return 0;
			}
			int num = GenTicks.TicksGame - animationStartTick;
			if (currentAnimation.loopMode == LoopMode.Clamp)
			{
				if (num <= currentAnimation.durationTicks)
				{
					return num;
				}
				return currentAnimation.durationTicks;
			}
			return GenMath.PositiveMod(num, currentAnimation.durationTicks);
		}
	}

	public bool AnimationFinished
	{
		get
		{
			if (currentAnimation == null || currentAnimation.durationTicks <= 0)
			{
				return false;
			}
			int num = GenTicks.TicksGame - animationStartTick;
			if (currentAnimation.loopMode == LoopMode.End)
			{
				return num > currentAnimation.durationTicks;
			}
			return false;
		}
	}

	private bool HasValidPropsAndRenderTree => pawn.RaceProps.renderTree?.root?.nodeClass != null;

	public PawnRenderTree(Pawn pawn)
	{
		this.pawn = pawn;
		if (dynamicNodeTypeInstances.Count == 0)
		{
			InitializeDynamicNodeTypes();
		}
	}

	private static void InitializeDynamicNodeTypes()
	{
		foreach (Type item in typeof(DynamicPawnRenderNodeSetup).AllSubclassesNonAbstract())
		{
			dynamicNodeTypeInstances.Add((DynamicPawnRenderNodeSetup)Activator.CreateInstance(item));
		}
		dynamicNodeTypeInstances.Sort(delegate(DynamicPawnRenderNodeSetup x, DynamicPawnRenderNodeSetup y)
		{
			if (x.SetupAfter != null && x.SetupAfter.Contains(y.GetType()))
			{
				return 1;
			}
			return (y.SetupAfter != null && y.SetupAfter.Contains(x.GetType())) ? (-1) : 0;
		});
	}

	public void EnsureInitialized(PawnRenderFlags defaultRenderFlagsNow)
	{
		TrySetupGraphIfNeeded();
		rootNode?.EnsureInitialized(defaultRenderFlagsNow);
	}

	public void ParallelPreDraw(PawnDrawParms parms)
	{
		if (!HasValidPropsAndRenderTree)
		{
			return;
		}
		AdjustParms(ref parms);
		if (oldParms.ShouldRecache(parms) || rootNode.RecacheRequested)
		{
			TraverseTree(delegate(PawnRenderNode node)
			{
				node.requestRecache = false;
			});
			drawRequests.Clear();
			rootNode.AppendRequests(parms, drawRequests);
			oldParms = parms;
		}
		for (int num = 0; num < drawRequests.Count; num++)
		{
			PawnGraphicDrawRequest value = drawRequests[num];
			if (!TryGetMatrix(value.node, parms, out var matrix))
			{
				Log.ErrorOnce($"Failed to get matrix for {value.node} on {pawn}.", Gen.HashCombine(174383246, pawn.GetHashCode()));
				break;
			}
			if (value.node.CheckMaterialEveryDrawRequest)
			{
				value.material = value.node.Worker.GetFinalizedMaterial(value.node, parms);
			}
			value.preDrawnComputedMatrix = matrix;
			drawRequests[num] = value;
		}
	}

	public void Draw(PawnDrawParms parms)
	{
		if (!Resolved)
		{
			Log.ErrorOnce($"Attempted to draw {pawn} without a resolved render tree.", Gen.HashCombine(174383246, pawn.GetHashCode()));
			return;
		}
		using (new ProfilerBlock("PawnRenderTree.Draw"))
		{
			for (int i = 0; i < drawRequests.Count; i++)
			{
				PawnGraphicDrawRequest pawnGraphicDrawRequest = drawRequests[i];
				Material material = pawnGraphicDrawRequest.material;
				if (material != null)
				{
					pawnGraphicDrawRequest.node.Worker.PreDraw(pawnGraphicDrawRequest.node, material, parms);
					MaterialPropertyBlock block = pawnGraphicDrawRequest.node.Worker.GetMaterialPropertyBlock(pawnGraphicDrawRequest.node, material, parms);
					foreach (PawnRenderSubWorker subWorker in pawnGraphicDrawRequest.node.Props.SubWorkers)
					{
						subWorker.EditMaterialPropertyBlock(pawnGraphicDrawRequest.node, material, parms, ref block);
					}
					GenDraw.DrawMeshNowOrLater(pawnGraphicDrawRequest.mesh, pawnGraphicDrawRequest.preDrawnComputedMatrix, material, parms.DrawNow, block);
					block.Clear();
				}
				pawnGraphicDrawRequest.node.Worker.PostDraw(pawnGraphicDrawRequest.node, parms, pawnGraphicDrawRequest.mesh, pawnGraphicDrawRequest.preDrawnComputedMatrix);
			}
		}
	}

	public bool GetRootTPRS(PawnDrawParms parms, out Vector3 offset, out Vector3 pivot, out Quaternion rotation, out Vector3 scale)
	{
		offset = Vector3.zero;
		pivot = Vector3.zero;
		rotation = Quaternion.identity;
		scale = Vector3.zero;
		if (rootNode == null || nodeAncestors == null)
		{
			return false;
		}
		List<PawnRenderNode> value;
		bool result = nodeAncestors.TryGetValue(rootNode, out value);
		for (int i = 0; i < value.Count; i++)
		{
			value[i].GetTransform(parms, out var offset2, out var pivot2, out var rotation2, out var scale2);
			offset += offset2;
			pivot += pivot2;
			rotation *= rotation2;
			scale += scale2;
		}
		return result;
	}

	public bool TryGetMatrix(PawnRenderNode node, PawnDrawParms parms, out Matrix4x4 matrix)
	{
		matrix = parms.matrix;
		if (!nodeAncestors.TryGetValue(node, out var value))
		{
			SetDirty();
			TrySetupGraphIfNeeded();
			if (!nodeAncestors.TryGetValue(node, out value))
			{
				return false;
			}
		}
		for (int i = 0; i < value.Count; i++)
		{
			value[i].GetTransform(parms, out var offset, out var pivot, out var rotation, out var scale);
			bool canRotate = !node.Props.rotateIndependently || value[i] == node;
			ComputeMatrix(ref matrix, in offset, in pivot, in rotation, in scale, canRotate);
		}
		float num = node.Worker.AltitudeFor(node, parms);
		if (num != 0f)
		{
			matrix *= Matrix4x4.Translate(Vector3.up * num);
		}
		return true;
	}

	[BurstCompile]
	private void ComputeMatrix(ref Matrix4x4 matrix, in Vector3 offset, in Vector3 pivot, in Quaternion rotation, in Vector3 scale, bool canRotate)
	{
		if (offset != Vector3.zero)
		{
			matrix *= Matrix4x4.Translate(offset);
		}
		if (pivot != Vector3.zero)
		{
			matrix *= Matrix4x4.Translate(pivot);
		}
		if (canRotate && rotation != Quaternion.identity)
		{
			matrix *= Matrix4x4.Rotate(rotation);
		}
		if (scale != Vector3.one)
		{
			matrix *= Matrix4x4.Scale(scale);
		}
		if (pivot != Vector3.zero)
		{
			matrix *= Matrix4x4.Translate(pivot).inverse;
		}
	}

	private void AdjustParms(ref PawnDrawParms parms)
	{
		if (debugTint.HasValue)
		{
			parms.tint *= debugTint.Value;
		}
		if (!pawn.RaceProps.Humanlike)
		{
			return;
		}
		if (parms.crawling && !parms.Portrait && parms.facing == Rot4.South)
		{
			parms.facing = Rot4.North;
			parms.flipHead = true;
		}
		if (pawn.apparel != null && PawnRenderNodeWorker_Apparel_Head.HeadgearVisible(parms))
		{
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (item.def.apparel.renderSkipFlags != null)
				{
					foreach (RenderSkipFlagDef renderSkipFlag in item.def.apparel.renderSkipFlags)
					{
						if (renderSkipFlag != RenderSkipFlagDefOf.None)
						{
							parms.skipFlags |= renderSkipFlag;
						}
					}
				}
				else
				{
					if (item.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
					{
						parms.skipFlags |= RenderSkipFlagDefOf.Hair;
					}
					if (item.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
					{
						parms.skipFlags |= RenderSkipFlagDefOf.Hair;
						parms.skipFlags |= RenderSkipFlagDefOf.Beard;
						parms.skipFlags |= RenderSkipFlagDefOf.Eyes;
					}
				}
				if (item.def.apparel.forceEyesVisibleForRotations.Contains(parms.facing.AsInt))
				{
					parms.skipFlags &= ~(ulong)RenderSkipFlagDefOf.Eyes;
				}
			}
		}
		if (pawn.genes != null && !pawn.genes.TattoosVisible)
		{
			parms.skipFlags |= RenderSkipFlagDefOf.Tattoos;
		}
	}

	private void TrySetupGraphIfNeeded()
	{
		if (Resolved)
		{
			return;
		}
		PawnRenderNodeProperties pawnRenderNodeProperties = pawn.RaceProps.renderTree?.root;
		if (pawnRenderNodeProperties?.nodeClass == null)
		{
			return;
		}
		SetDirty();
		using (new ProfilerBlock("TrySetupGraph"))
		{
			try
			{
				rootNode = (PawnRenderNode)Activator.CreateInstance(pawnRenderNodeProperties.nodeClass, pawn, pawnRenderNodeProperties, this);
				SetupDynamicNodes();
			}
			catch (Exception arg)
			{
				Log.Error($"Exception setting up dynamic nodes for {pawn}: {arg}");
			}
			foreach (var (key, list2) in tmpChildTagNodes)
			{
				if (nodesByTag.TryGetValue(key, out var value))
				{
					value.AddChildren(list2.ToArray());
				}
			}
			if (Prefs.DevMode)
			{
				Dialog_DebugRenderTree dialog_DebugRenderTree = Find.WindowStack.WindowOfType<Dialog_DebugRenderTree>();
				if (dialog_DebugRenderTree != null && dialog_DebugRenderTree.pawn == pawn)
				{
					dialog_DebugRenderTree.Init(pawn);
				}
			}
			InitializeAncestors();
		}
		tmpChildTagNodes.Clear();
	}

	public bool TryGetNodeByTag(PawnRenderNodeTagDef tag, out PawnRenderNode node)
	{
		return nodesByTag.TryGetValue(tag, out node);
	}

	private void SetupDynamicNodes()
	{
		foreach (DynamicPawnRenderNodeSetup dynamicNodeTypeInstance in dynamicNodeTypeInstances)
		{
			if (dynamicNodeTypeInstance.HumanlikeOnly && !pawn.RaceProps.Humanlike)
			{
				continue;
			}
			foreach (var (child, parent) in dynamicNodeTypeInstance.GetDynamicNodes(pawn, this))
			{
				AddChild(child, parent);
			}
		}
		foreach (ThingComp allComp in pawn.AllComps)
		{
			List<PawnRenderNode> list = allComp.CompRenderNodes();
			if (list == null)
			{
				continue;
			}
			foreach (PawnRenderNode item in list)
			{
				if (ShouldAddNodeToTree(item?.Props))
				{
					AddChild(item, null);
				}
			}
		}
	}

	private void InitializeAncestors()
	{
		TraverseTree(delegate(PawnRenderNode node)
		{
			if (!nodeAncestors.ContainsKey(node))
			{
				nodeAncestors.Add(node, new List<PawnRenderNode>());
			}
			for (PawnRenderNode pawnRenderNode = node; pawnRenderNode != null; pawnRenderNode = pawnRenderNode.parent)
			{
				nodeAncestors[node].Add(pawnRenderNode);
			}
			nodeAncestors[node].Reverse();
		});
	}

	private void TraverseTree(Action<PawnRenderNode> action)
	{
		try
		{
			nodeQueue.Enqueue(rootNode);
			while (nodeQueue.Count > 0)
			{
				PawnRenderNode pawnRenderNode = nodeQueue.Dequeue();
				if (pawnRenderNode == null)
				{
					Log.ErrorOnce($"Node is null - you must called EnsureGraphicsInitialized() on the drawn dynamic thing {pawn} before drawing it.", Gen.HashCombine(1743846, pawn.GetHashCode()));
					break;
				}
				action(pawnRenderNode);
				if (pawnRenderNode.children != null)
				{
					PawnRenderNode[] children = pawnRenderNode.children;
					foreach (PawnRenderNode item in children)
					{
						nodeQueue.Enqueue(item);
					}
				}
			}
		}
		catch (Exception arg)
		{
			Log.Error($"Exception traversing pawn render node tree {pawn}: {arg}");
		}
		finally
		{
			nodeQueue.Clear();
		}
	}

	public bool ShouldAddNodeToTree(PawnRenderNodeProperties props)
	{
		if (props == null)
		{
			return false;
		}
		return props.pawnType switch
		{
			PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly => pawn.RaceProps.Humanlike, 
			PawnRenderNodeProperties.RenderNodePawnType.NonHumanlikeOnly => !pawn.RaceProps.Humanlike, 
			_ => true, 
		};
	}

	private void AddChild(PawnRenderNode child, PawnRenderNode parent)
	{
		if (parent == null)
		{
			parent = ((child.Props.parentTagDef == null || !nodesByTag.TryGetValue(child.Props.parentTagDef, out var value)) ? rootNode : value);
		}
		if (parent.Props.tagDef != null)
		{
			if (tmpChildTagNodes.TryGetValue(parent.Props.tagDef, out var value2))
			{
				value2.Add(child);
			}
			else
			{
				tmpChildTagNodes.Add(parent.Props.tagDef, new List<PawnRenderNode> { child });
			}
		}
		child.parent = parent;
	}

	public void SetTagNode(PawnRenderNodeTagDef tag, PawnRenderNode node)
	{
		nodesByTag[tag] = node;
	}

	public void SetDirty()
	{
		nodeAncestors.Clear();
		drawRequests.Clear();
		rootNode = null;
		nodesByTag.Clear();
		oldParms = default(PawnDrawParms);
	}

	public bool TryGetAnimationPartForNode(PawnRenderNode node, out AnimationPart animationPart)
	{
		animationPart = null;
		if (currentAnimation == null)
		{
			return false;
		}
		if (node.Props.tagDef == null)
		{
			return false;
		}
		return node.tree.currentAnimation.TryGetPartForNodeTag(node.Props.tagDef, out animationPart);
	}
}
