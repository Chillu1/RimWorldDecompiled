using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlanetLayerDef : Def
{
	private BiomeDef defaultBiome;

	private WorldObjectDef defaultMapWorldObject;

	private WorldObjectDef settlementWorldObjectDef;

	private WorldObjectDef destroyedSettlementWorldObjectDef;

	public bool onlyAllowWhitelistedIncidents;

	public bool onlyAllowWhitelistedBiomes;

	public bool onlyAllowWhitelistedGameConditions;

	public bool onlyAllowWhitelistedQuests;

	public bool onlyAllowWhitelistedArrivals;

	public bool onlyAllowWhitelistedArrivalModes;

	public bool alwaysRaycastable = true;

	public bool obstructsExpandingIcons = true;

	public bool canFormCaravans = true;

	public bool isSpace;

	public float rangeDistanceFactor = 1f;

	public float generatedLocationFactor = 1f;

	public float raidPointsFactor = 1f;

	public BiomeDef backgroundBiome;

	public bool ignoreNoBuildArea;

	public Color lineColor = Color.white;

	public float lineWidthFactor = 1f;

	private List<WorldGenStepDef> worldGenSteps = new List<WorldGenStepDef>();

	public List<Type> worldDrawLayers = new List<Type>();

	private List<Type> worldTabs = new List<Type>();

	public Type layerType = typeof(PlanetLayer);

	public Type tileType = typeof(Tile);

	[MustTranslate]
	public string elevationString = "{0}m";

	[MustTranslate]
	public string viewGizmoTooltip;

	[MustTranslate]
	public string gerundLabel;

	public bool viewGizmoOnlyVisibleWithDirectConnection = true;

	public FloatRange settlementsPer100kTiles = new FloatRange(75f, 85f);

	public SimpleCurve viewAngleSettlementsFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(1f, 1f)
	};

	public string viewGizmoTexPath;

	[Unsaved(false)]
	private List<WorldGenStepDef> cachedGenSteps;

	[Unsaved(false)]
	private readonly List<WITab> cachedTabs = new List<WITab>();

	[Unsaved(false)]
	private Texture2D cachedGizmoTexture;

	private bool usesSurfaceTiles;

	public bool SurfaceTiles => usesSurfaceTiles;

	public BiomeDef DefaultBiome => defaultBiome ?? BiomeDefOf.TemperateForest;

	public Texture2D ViewGizmoTexture
	{
		get
		{
			if (!cachedGizmoTexture)
			{
				return cachedGizmoTexture = ContentFinder<Texture2D>.Get(viewGizmoTexPath);
			}
			return cachedGizmoTexture;
		}
	}

	public WorldObjectDef DefaultWorldObject => defaultMapWorldObject ?? WorldObjectDefOf.Settlement;

	public WorldObjectDef SettlementWorldObjectDef => settlementWorldObjectDef ?? WorldObjectDefOf.Settlement;

	public WorldObjectDef DestroyedSettlementWorldObjectDef => destroyedSettlementWorldObjectDef ?? WorldObjectDefOf.DestroyedSettlement;

	public Material WorldLineMaterial => MaterialPool.MatFrom(GenDraw.OneSidedLineTexPath, ShaderDatabase.WorldOverlayTransparent, lineColor, 3590);

	public Material WorldLineMaterialHighVis => MaterialPool.MatFrom(GenDraw.OneSidedLineOpaqueTexPath, ShaderDatabase.WorldOverlayAdditiveTwoSided, lineColor, 3590);

	public List<WorldGenStepDef> GenStepsInOrder => cachedGenSteps ?? (cachedGenSteps = (from x in worldGenSteps
		orderby x.order, x.index
		select x).ToList());

	public List<WITab> Tabs => cachedTabs;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (tileType == null || !tileType.SameOrSubclassOf<Tile>())
		{
			yield return "tileType type is not the same or subclass of Tile";
		}
		if (layerType == null || !layerType.SameOrSubclassOf<PlanetLayer>())
		{
			yield return "layerType type is not the same or subclass of PlanetLayer";
		}
		foreach (Type worldDrawLayer in worldDrawLayers)
		{
			if (!worldDrawLayer.IsSubclassOf(typeof(WorldDrawLayer)))
			{
				yield return string.Format("{0} layer {1} is type that is not a subclass of {2}", "worldDrawLayers", worldDrawLayer, "WorldDrawLayer");
			}
		}
		foreach (Type worldTab in worldTabs)
		{
			if (!worldTab.IsSubclassOf(typeof(WITab)))
			{
				yield return string.Format("{0} layer {1} is type that is not a subclass of {2}", "worldTabs", worldTab, "WITab");
			}
		}
		if (string.IsNullOrEmpty(viewGizmoTexPath))
		{
			yield return "Must provide a texture to viewGizmoTexPath, use PlaceholderImage if required.";
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		usesSurfaceTiles = typeof(SurfaceTile).IsAssignableFrom(tileType);
		foreach (Type worldTab in worldTabs)
		{
			cachedTabs.Add((WITab)Activator.CreateInstance(worldTab));
		}
	}
}
