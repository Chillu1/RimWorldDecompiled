using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class WorldObjectDef : Def
{
	public Type worldObjectClass = typeof(WorldObject);

	public bool canHaveFaction = true;

	public bool saved = true;

	public bool canBePlayerHome;

	public bool treatAsPlayerHome;

	public bool canResizeToGravship;

	public bool requiresSignalJammerToReach;

	public IntVec3? overrideMapSize;

	public List<WorldObjectCompProperties> comps = new List<WorldObjectCompProperties>();

	public bool canHaveMap = true;

	public bool validLaunchTarget = true;

	public bool allowCaravanIncidentsWhichGenerateMap;

	public bool isTempIncidentMapOwner;

	public List<IncidentTargetTagDef> IncidentTargetTags;

	public bool selectable = true;

	public bool neverMultiSelect;

	public MapGeneratorDef mapGenerator;

	public List<Type> inspectorTabs;

	[Unsaved(false)]
	public List<InspectTabBase> inspectorTabsResolved;

	public bool useDynamicDrawer;

	public bool expandingIcon;

	[NoTranslate]
	public string expandingIconTexture;

	[NoTranslate]
	public string expandingIconMaskTexture;

	public float expandingIconPriority;

	[NoTranslate]
	public string texture;

	[NoTranslate]
	public string mapEdgeTexture;

	private ShaderTypeDef mapEdgeShader;

	[Unsaved(false)]
	private Material material;

	[Unsaved(false)]
	private Material expandingMaterial;

	[Unsaved(false)]
	private Texture2D expandingIconTextureInt;

	[Unsaved(false)]
	private Texture2D expandingIconTextureMaskInt;

	public bool expandMore;

	public bool rotateGraphicWhenTraveling;

	public Color? expandingIconColor;

	public float expandingIconDrawSize = 1f;

	public bool fullyExpandedInSpace;

	public ShaderTypeDef shader;

	public ShaderTypeDef expandingShader;

	public float drawAltitudeOffset;

	public bool blockExitGridUntilBattleIsWon;

	public RulePackDef nameMaker;

	public Material Material
	{
		get
		{
			if (texture.NullOrEmpty())
			{
				return null;
			}
			if (material == null)
			{
				material = MaterialPool.MatFrom(texture, shader?.Shader ?? ShaderDatabase.WorldOverlayTransparentLit, 3550);
			}
			return material;
		}
	}

	public Texture2D ExpandingIconTexture
	{
		get
		{
			if (expandingIconTextureInt == null)
			{
				if (expandingIconTexture.NullOrEmpty())
				{
					return null;
				}
				expandingIconTextureInt = ContentFinder<Texture2D>.Get(expandingIconTexture);
			}
			return expandingIconTextureInt;
		}
	}

	public Texture2D ExpandingIconTextureMask
	{
		get
		{
			if (expandingIconTextureMaskInt == null)
			{
				if (expandingIconMaskTexture.NullOrEmpty())
				{
					return null;
				}
				expandingIconTextureMaskInt = ContentFinder<Texture2D>.Get(expandingIconMaskTexture);
			}
			return expandingIconTextureMaskInt;
		}
	}

	public Material MapEdgeMaterial
	{
		get
		{
			if (mapEdgeTexture.NullOrEmpty())
			{
				return null;
			}
			if (material == null)
			{
				material = MaterialPool.MatFrom(mapEdgeTexture, ShaderDatabase.MapEdgeTerrain);
			}
			return material;
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (inspectorTabs == null)
		{
			return;
		}
		for (int i = 0; i < inspectorTabs.Count; i++)
		{
			if (inspectorTabsResolved == null)
			{
				inspectorTabsResolved = new List<InspectTabBase>();
			}
			try
			{
				inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(inspectorTabs[i]));
			}
			catch (Exception ex)
			{
				Log.Error("Could not instantiate inspector tab of type " + inspectorTabs[i]?.ToString() + ": " + ex);
			}
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		for (int i = 0; i < comps.Count; i++)
		{
			comps[i].ResolveReferences(this);
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		for (int i = 0; i < comps.Count; i++)
		{
			foreach (string item2 in comps[i].ConfigErrors(this))
			{
				yield return item2;
			}
		}
		if (expandMore && !expandingIcon)
		{
			yield return "has expandMore but doesn't have any expanding icon";
		}
	}
}
