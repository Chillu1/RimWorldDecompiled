using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class TerrainDef : BuildableDef
{
	public enum TerrainEdgeType : byte
	{
		Hard,
		Fade,
		FadeRough,
		Water
	}

	public enum TerrainCategoryType : byte
	{
		Misc,
		Soil,
		Stone,
		Sand
	}

	public class SpaceEdgeGraphicData
	{
		[NoTranslate]
		public string OShapeTexPath;

		[NoTranslate]
		public string UShapeTexPath;

		[NoTranslate]
		public string CornerInnerTexPath;

		[NoTranslate]
		public string CornerOuterTexPath;

		[NoTranslate]
		public string FlatTexPath;

		[NoTranslate]
		public string LoopLeftTexPath;

		[NoTranslate]
		public string LoopRightTexPath;

		[NoTranslate]
		public string LoopSingleTexPath;

		[NoTranslate]
		public List<string> LoopTexPaths;

		public Color? overrideColor;

		private CachedMaterial oShape;

		private CachedMaterial uShape;

		private CachedMaterial cornerInner;

		private CachedMaterial cornerOuter;

		private CachedMaterial flat;

		private CachedMaterial loopLeft;

		private CachedMaterial loopRight;

		private CachedMaterial loopSingle;

		private ShaderTypeDef loopShader;

		private ShaderTypeDef cornerShader;

		private readonly List<CachedMaterial> loops = new List<CachedMaterial>();

		private bool initialized;

		public Shader LoopShader => loopShader?.Shader ?? ShaderDatabase.TerrainEdge;

		public Shader CornerShader => cornerShader?.Shader ?? ShaderDatabase.Transparent;

		public void EnsureInitialized(TerrainDef def)
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			Shader shader = LoopShader;
			Shader shader2 = CornerShader;
			oShape = new CachedMaterial(OShapeTexPath, shader2, overrideColor ?? def.color);
			uShape = new CachedMaterial(UShapeTexPath, shader2, overrideColor ?? def.color);
			cornerInner = new CachedMaterial(CornerInnerTexPath, shader2, overrideColor ?? def.color);
			cornerOuter = new CachedMaterial(CornerOuterTexPath, shader2, overrideColor ?? def.color);
			flat = new CachedMaterial(FlatTexPath, shader2, overrideColor ?? def.color);
			loopLeft = new CachedMaterial(LoopLeftTexPath, shader, overrideColor ?? def.color);
			loopRight = new CachedMaterial(LoopRightTexPath, shader, overrideColor ?? def.color);
			loopSingle = new CachedMaterial(LoopSingleTexPath, shader, overrideColor ?? def.color);
			foreach (string loopTexPath in LoopTexPaths)
			{
				loops.Add(new CachedMaterial(loopTexPath, shader, overrideColor ?? def.color));
			}
		}

		public Material GetMaterial(TerrainDef terrain, SectionLayer_TerrainEdges.EdgeType edgeType, int listIndexOffset = 0)
		{
			EnsureInitialized(terrain);
			return edgeType switch
			{
				SectionLayer_TerrainEdges.EdgeType.OShape => oShape.Material, 
				SectionLayer_TerrainEdges.EdgeType.UShape => uShape.Material, 
				SectionLayer_TerrainEdges.EdgeType.CornerInner => cornerInner.Material, 
				SectionLayer_TerrainEdges.EdgeType.CornerOuter => cornerOuter.Material, 
				SectionLayer_TerrainEdges.EdgeType.Flat => flat.Material, 
				SectionLayer_TerrainEdges.EdgeType.LoopLeft => loopLeft.Material, 
				SectionLayer_TerrainEdges.EdgeType.LoopRight => loopRight.Material, 
				SectionLayer_TerrainEdges.EdgeType.LoopSingle => loopSingle.Material, 
				SectionLayer_TerrainEdges.EdgeType.Loop => loops[Mathf.Abs(listIndexOffset) % loops.Count].Material, 
				_ => throw new ArgumentOutOfRangeException("edgeType", edgeType, null), 
			};
		}
	}

	[NoTranslate]
	public string texturePath;

	public TerrainEdgeType edgeType;

	public ShaderTypeDef customShader;

	public List<ShaderParameter> customShaderParameters;

	[NoTranslate]
	public string waterDepthShader;

	public List<ShaderParameter> waterDepthShaderParameters;

	public int renderPrecedence;

	public List<TerrainAffordanceDef> affordances = new List<TerrainAffordanceDef>();

	public bool layerable;

	[NoTranslate]
	public string scatterType;

	public bool takeFootprints;

	public bool natural;

	public bool takeSplashes;

	public bool avoidWander;

	public bool changeable = true;

	public TerrainDef smoothedTerrain;

	public TerrainDef gravshipReplacementTerrain;

	public bool holdSnowOrSand = true;

	public bool isPaintable;

	public bool extinguishesFire;

	public Color color = Color.white;

	public ColorDef colorDef;

	public TerrainDef driesTo;

	[NoTranslate]
	public List<string> tags;

	public TerrainDef burnedDef;

	public List<Tool> tools;

	public float extraDeteriorationFactor;

	public float destroyOnBombDamageThreshold = -1f;

	public bool destroyBuildingsOnDestroyed;

	public ThoughtDef traversedThought;

	public int extraDraftedPerceivedPathCost;

	public int extraNonDraftedPerceivedPathCost;

	public EffecterDef destroyEffect;

	public EffecterDef destroyEffectWater;

	public bool autoRebuildable;

	public TerrainCategoryType categoryType;

	public float meltSnowRadius;

	public float heatPerTick;

	public float igniteRadius;

	public float ignitePawnsIntervalTicks;

	public int burnDamage;

	public float burnIntervalTicks;

	public bool supportsRock = true;

	public float toxicBuildupFactor;

	public bool dontRender;

	public bool exposesToVacuum;

	public bool canFreeze;

	public bool dangerous;

	public bool preventCraters;

	public bool canEverTerraform = true;

	public WaterBodyType waterBodyType;

	public bool cropIcon = true;

	public float glowRadius;

	public ColorInt glowColor;

	public float throwFleckChance;

	public TerrainFleckData fleckData;

	public bool isFoundation;

	public bool bridge;

	[NoTranslate]
	public string bridgePropsPath;

	[NoTranslate]
	public string spaceBridgePropsPath;

	[Unsaved(false)]
	public Graphic bridgePropsLoopGraphic = BaseContent.BadGraphic;

	[Unsaved(false)]
	public Graphic bridgePropsRightGraphic = BaseContent.BadGraphic;

	[Unsaved(false)]
	public Graphic spaceBridgePropsLoopGraphic = BaseContent.BadGraphic;

	[Unsaved(false)]
	public Graphic spaceBridgePropsRightGraphic = BaseContent.BadGraphic;

	public bool temporary;

	public TempTerrainProps tempTerrain;

	public TerrainDef floodTerrain;

	public bool canBePolluted = true;

	[NoTranslate]
	public string pollutedTexturePath;

	[NoTranslate]
	public string pollutionOverlayTexturePath;

	public ShaderTypeDef pollutionShaderType;

	public Color pollutionColor = Color.white;

	public Vector2 pollutionOverlayScrollSpeed = Vector2.zero;

	public Vector2 pollutionOverlayScale = Vector2.one;

	public Color pollutionCloudColor = Color.white;

	public Color pollutionTintColor = Color.white;

	public ThingDef generatedFilth;

	public FilthSourceFlags filthAcceptanceMask = FilthSourceFlags.Any;

	public SpaceEdgeGraphicData spaceEdgeGraphicData;

	[Unsaved(false)]
	public Material waterDepthMaterial;

	[Unsaved(false)]
	public Graphic graphicPolluted = BaseContent.BadGraphic;

	public bool Removable => layerable;

	public bool IsCarpet => researchPrerequisites.NotNullAndContains(ResearchProjectDefOf.CarpetMaking);

	public Color DrawColor
	{
		get
		{
			if (colorDef != null)
			{
				return colorDef.color;
			}
			return color;
		}
	}

	public bool IsRiver => HasTag("River");

	public bool IsOcean => HasTag("Ocean");

	public bool IsWater => HasTag("Water");

	public bool IsIce => HasTag("Ice");

	public bool IsFlood => HasTag("Flood");

	public bool IsFine => HasTag("FineFloor");

	public bool IsSoil => HasTag("Soil");

	public bool IsRoad => HasTag("Road");

	public bool IsFloor => HasTag("Floor");

	public bool IsRock => HasTag("NaturalRock");

	public bool IsSubstructure
	{
		get
		{
			if (ModsConfig.OdysseyActive)
			{
				return HasTag("Substructure");
			}
			return false;
		}
	}

	public Shader Shader
	{
		get
		{
			if (customShader != null)
			{
				return customShader.Shader;
			}
			Shader result = null;
			switch (edgeType)
			{
			case TerrainEdgeType.Hard:
				result = ShaderDatabase.TerrainHard;
				break;
			case TerrainEdgeType.Fade:
				result = ShaderDatabase.TerrainFade;
				break;
			case TerrainEdgeType.FadeRough:
				result = ShaderDatabase.TerrainFadeRough;
				break;
			case TerrainEdgeType.Water:
				result = ShaderDatabase.TerrainWater;
				break;
			}
			return result;
		}
	}

	private Shader ShaderPolluted
	{
		get
		{
			if (pollutionShaderType != null)
			{
				return pollutionShaderType.Shader;
			}
			if (customShader != null)
			{
				return customShader.Shader;
			}
			Shader result = null;
			switch (edgeType)
			{
			case TerrainEdgeType.Hard:
				result = ShaderDatabase.TerrainHardPolluted;
				break;
			case TerrainEdgeType.Fade:
				result = ShaderDatabase.TerrainFadePolluted;
				break;
			case TerrainEdgeType.FadeRough:
				result = ShaderDatabase.TerrainFadeRoughPolluted;
				break;
			}
			return result;
		}
	}

	public Material DrawMatPolluted
	{
		get
		{
			if (graphicPolluted == BaseContent.BadGraphic)
			{
				return graphic.MatSingle;
			}
			return graphicPolluted.MatSingle;
		}
	}

	public override void PostLoad()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (drawStyleCategory == null)
			{
				drawStyleCategory = DrawStyleCategoryDefOf.Floors;
			}
			if (!dontRender)
			{
				if (graphic == BaseContent.BadGraphic)
				{
					graphic = GraphicDatabase.Get<Graphic_Terrain>(texturePath, Shader, Vector2.one, DrawColor, 2000 + renderPrecedence);
					if (edgeType == TerrainEdgeType.FadeRough || edgeType == TerrainEdgeType.Water)
					{
						graphic.MatSingle.SetTexture(ShaderPropertyIDs.AlphaAddTex, TexGame.AlphaAddTex);
					}
					if (customShader != null && customShaderParameters != null)
					{
						for (int i = 0; i < customShaderParameters.Count; i++)
						{
							customShaderParameters[i].Apply(graphic.MatSingle);
						}
					}
				}
				if (!waterDepthShader.NullOrEmpty())
				{
					waterDepthMaterial = MaterialAllocator.Create(ShaderDatabase.LoadShader(waterDepthShader));
					waterDepthMaterial.renderQueue = 2000 + renderPrecedence;
					waterDepthMaterial.SetTexture(ShaderPropertyIDs.AlphaAddTex, TexGame.AlphaAddTex);
					if (waterDepthShaderParameters != null)
					{
						for (int j = 0; j < waterDepthShaderParameters.Count; j++)
						{
							waterDepthShaderParameters[j].Apply(waterDepthMaterial);
						}
					}
				}
				if (ModsConfig.BiotechActive && graphicPolluted == BaseContent.BadGraphic && (!pollutionOverlayTexturePath.NullOrEmpty() || !pollutedTexturePath.NullOrEmpty()))
				{
					Texture2D texture2D = null;
					if (!pollutionOverlayTexturePath.NullOrEmpty())
					{
						texture2D = ContentFinder<Texture2D>.Get(pollutionOverlayTexturePath);
					}
					graphicPolluted = GraphicDatabase.Get<Graphic_Terrain>(pollutedTexturePath ?? texturePath, ShaderPolluted, Vector2.one, DrawColor, 2000 + renderPrecedence);
					Material matSingle = graphicPolluted.MatSingle;
					if (texture2D != null)
					{
						matSingle.SetTexture(ShaderPropertyIDs.BurnTex, texture2D);
					}
					matSingle.SetColor(ShaderPropertyIDs.BurnColor, pollutionColor);
					matSingle.SetVector(ShaderPropertyIDs.BurnScale, pollutionOverlayScale);
					matSingle.SetVector(ShaderPropertyIDs.ScrollSpeed, pollutionOverlayScrollSpeed);
					matSingle.SetColor(ShaderPropertyIDs.PollutionTintColor, pollutionTintColor);
					if (edgeType == TerrainEdgeType.FadeRough)
					{
						matSingle.SetTexture(ShaderPropertyIDs.AlphaAddTex, TexGame.AlphaAddTex);
					}
					if (matSingle != graphic.MatSingle)
					{
						matSingle.SetFloat(ShaderPropertyIDs.IsPolluted, 1f);
					}
					if ((pollutionShaderType != null || customShader != null) && customShaderParameters != null)
					{
						for (int k = 0; k < customShaderParameters.Count; k++)
						{
							customShaderParameters[k].Apply(matSingle);
						}
					}
				}
				if (!bridgePropsPath.NullOrEmpty() && bridgePropsLoopGraphic == BaseContent.BadGraphic)
				{
					bridgePropsLoopGraphic = GraphicDatabase.Get<Graphic_Terrain>(bridgePropsPath + "_Loop", ShaderDatabase.Transparent);
					bridgePropsRightGraphic = GraphicDatabase.Get<Graphic_Terrain>(bridgePropsPath + "_Right", ShaderDatabase.Transparent);
				}
				if (ModsConfig.OdysseyActive && !spaceBridgePropsPath.NullOrEmpty() && spaceBridgePropsLoopGraphic == BaseContent.BadGraphic)
				{
					spaceBridgePropsLoopGraphic = GraphicDatabase.Get<Graphic_Terrain>(spaceBridgePropsPath + "_Loop", ShaderDatabase.Transparent);
					spaceBridgePropsRightGraphic = GraphicDatabase.Get<Graphic_Terrain>(spaceBridgePropsPath + "_Right", ShaderDatabase.Transparent);
				}
			}
		});
		if (tools != null)
		{
			for (int num = 0; num < tools.Count; num++)
			{
				tools[num].id = num.ToString();
			}
		}
		base.PostLoad();
	}

	protected override void ResolveIcon()
	{
		base.ResolveIcon();
		uiIconColor = DrawColor;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (texturePath.NullOrEmpty() && !dontRender)
		{
			yield return "missing texturePath";
		}
		if (fertility < 0f)
		{
			yield return "Terrain Def " + this?.ToString() + " has no fertility value set.";
		}
		if (renderPrecedence > 400)
		{
			yield return "Render order " + renderPrecedence + " is out of range (must be < 400)";
		}
		if (generatedFilth != null && (filthAcceptanceMask & FilthSourceFlags.Terrain) > FilthSourceFlags.None)
		{
			yield return defName + " makes terrain filth and also accepts it.";
		}
		if (this.Flammable() && burnedDef == null && !layerable)
		{
			yield return "flammable but burnedDef is null and not layerable";
		}
		if (burnedDef != null && burnedDef.Flammable())
		{
			yield return "burnedDef is flammable";
		}
		if (throwFleckChance > 0f && fleckData.fleck == null)
		{
			yield return "throwFleckChance is > 0 but fleckData.fleck is null";
		}
	}

	public static TerrainDef Named(string defName)
	{
		return DefDatabase<TerrainDef>.GetNamed(defName);
	}

	public bool HasTag(string tag)
	{
		if (tags != null)
		{
			return tags.Contains(tag);
		}
		return false;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		string[] array = (from ta in affordances.Distinct()
			orderby ta.order
			select ta.label).ToArray();
		if (array.Length != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Terrain, "Supports".Translate(), array.ToCommaList().CapitalizeFirst(), "Stat_Thing_Terrain_Supports_Desc".Translate(), 2000);
		}
		if (IsFine && ModsConfig.RoyaltyActive)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Terrain, "Stat_Thing_Terrain_Fine_Name".Translate(), "Stat_Thing_Terrain_Fine_Value".Translate(), "Stat_Thing_Terrain_Fine_Desc".Translate(), 2000);
		}
	}
}
