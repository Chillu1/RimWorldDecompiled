using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class PawnRenderNodeProperties
{
	public enum Side
	{
		Center,
		Left,
		Right
	}

	public enum AttachmentColorType
	{
		Custom,
		Hair,
		Skin
	}

	public enum RenderNodePawnType
	{
		Any,
		HumanlikeOnly,
		NonHumanlikeOnly
	}

	public Type workerClass = typeof(PawnRenderNodeWorker);

	public List<Type> subworkerClasses;

	public Type nodeClass = typeof(PawnRenderNode);

	public PawnRenderNodeTagDef tagDef;

	public PawnRenderNodeTagDef parentTagDef;

	public List<PawnRenderNodeProperties> children;

	[NoTranslate]
	public string texPath;

	[NoTranslate]
	public List<string> texPaths;

	[NoTranslate]
	public string texPathFemale;

	[NoTranslate]
	public List<string> texPathsFemale;

	public List<BodyTypeGraphicData> bodyTypeGraphicPaths;

	public int texSeed;

	public bool useGraphic = true;

	public RenderSkipFlagDef skipFlag;

	public RotDrawMode rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting | RotDrawMode.Dessicated;

	public RenderNodePawnType pawnType = RenderNodePawnType.HumanlikeOnly;

	public BodyPartGroupDef linkedBodyPartsGroup;

	public Color? color;

	public float colorRGBPostFactor = 1f;

	public bool useRottenColor;

	public bool useSkinShader;

	public ShaderTypeDef shaderTypeDef;

	public AttachmentColorType colorType;

	public float baseLayer;

	public DrawData drawData;

	public bool oppositeFacingLayerWhenFlipped;

	public Vector2? overrideMeshSize;

	public Vector2 drawSize = Vector2.one;

	[NoTranslate]
	public string anchorTag;

	public Side side;

	public float narrowCrownHorizontalOffset;

	public List<Rot4> visibleFacing;

	public bool flipGraphic;

	public bool rotateIndependently;

	public string debugLabel;

	public PawnOverlayDrawer.OverlayLayer overlayLayer;

	public bool? overlayOverApparel;

	private PawnRenderNodeWorker workerInt;

	private List<PawnRenderSubWorker> subWorkersInt;

	public PawnRenderNodeWorker Worker => GenWorker<PawnRenderNodeWorker>.Get(workerClass);

	public List<PawnRenderSubWorker> SubWorkers => subWorkersInt ?? (subWorkersInt = PawnRenderNodeWorker.GetSubWorkerList(subworkerClasses));

	public void EnsureInitialized()
	{
		if (workerInt == null)
		{
			workerInt = GenWorker<PawnRenderNodeWorker>.Get(workerClass);
		}
		if (subWorkersInt == null)
		{
			subWorkersInt = PawnRenderNodeWorker.GetSubWorkerList(subworkerClasses);
		}
	}

	public virtual void ResolveReferences()
	{
	}

	public virtual void ResolveReferencesRecursive()
	{
		ResolveReferences();
		if (children == null)
		{
			return;
		}
		foreach (PawnRenderNodeProperties child in children)
		{
			child.ResolveReferencesRecursive();
		}
	}

	public virtual IEnumerable<string> ConfigErrors()
	{
		if (nodeClass == null)
		{
			yield return "Node " + debugLabel + ": nodeClass is null.";
		}
		else if (!typeof(PawnRenderNode).IsAssignableFrom(nodeClass))
		{
			yield return "Node " + debugLabel + ": nodeClass is not a PawnRenderNode or subclass thereof.";
		}
		if (workerClass == null)
		{
			yield return "Node " + debugLabel + ": workerClass is null.";
		}
		else if (!typeof(PawnRenderNodeWorker).IsAssignableFrom(workerClass))
		{
			yield return "Node " + debugLabel + ": workerClass is not a PawnRenderNodeWorker or subclass thereof.";
		}
		if (children == null)
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties child in children)
		{
			foreach (string item in child.ConfigErrors())
			{
				yield return item;
			}
		}
	}
}
