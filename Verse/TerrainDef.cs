using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class TerrainDef : BuildableDef
	{
		public enum TerrainEdgeType : byte
		{
			Hard,
			Fade,
			FadeRough,
			Water
		}

		[NoTranslate]
		public string texturePath;

		public TerrainEdgeType edgeType;

		[NoTranslate]
		public string waterDepthShader;

		public List<ShaderParameter> waterDepthShaderParameters;

		public int renderPrecedence;

		public List<TerrainAffordanceDef> affordances = new List<TerrainAffordanceDef>();

		public bool layerable;

		[NoTranslate]
		public string scatterType;

		public bool takeFootprints;

		public bool takeSplashes;

		public bool avoidWander;

		public bool changeable = true;

		public TerrainDef smoothedTerrain;

		public bool holdSnow = true;

		public bool extinguishesFire;

		public Color color = Color.white;

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

		public ThingDef generatedFilth;

		public FilthSourceFlags filthAcceptanceMask = FilthSourceFlags.Any;

		[Unsaved(false)]
		public Material waterDepthMaterial;

		public bool Removable => layerable;

		public bool IsCarpet
		{
			get
			{
				if (researchPrerequisites != null)
				{
					return researchPrerequisites.Contains(ResearchProjectDefOf.CarpetMaking);
				}
				return false;
			}
		}

		public bool IsRiver => HasTag("River");

		public bool IsWater => HasTag("Water");

		public bool IsFine => HasTag("FineFloor");

		public override void PostLoad()
		{
			placingDraggableDimensions = 2;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Shader shader = null;
				switch (edgeType)
				{
				case TerrainEdgeType.Hard:
					shader = ShaderDatabase.TerrainHard;
					break;
				case TerrainEdgeType.Fade:
					shader = ShaderDatabase.TerrainFade;
					break;
				case TerrainEdgeType.FadeRough:
					shader = ShaderDatabase.TerrainFadeRough;
					break;
				case TerrainEdgeType.Water:
					shader = ShaderDatabase.TerrainWater;
					break;
				}
				graphic = GraphicDatabase.Get<Graphic_Terrain>(texturePath, shader, Vector2.one, color, 2000 + renderPrecedence);
				if (shader == ShaderDatabase.TerrainFadeRough || shader == ShaderDatabase.TerrainWater)
				{
					graphic.MatSingle.SetTexture("_AlphaAddTex", TexGame.AlphaAddTex);
				}
				if (!waterDepthShader.NullOrEmpty())
				{
					waterDepthMaterial = MaterialAllocator.Create(ShaderDatabase.LoadShader(waterDepthShader));
					waterDepthMaterial.renderQueue = 2000 + renderPrecedence;
					waterDepthMaterial.SetTexture("_AlphaAddTex", TexGame.AlphaAddTex);
					if (waterDepthShaderParameters != null)
					{
						for (int j = 0; j < waterDepthShaderParameters.Count; j++)
						{
							waterDepthShaderParameters[j].Apply(waterDepthMaterial);
						}
					}
				}
			});
			if (tools != null)
			{
				for (int i = 0; i < tools.Count; i++)
				{
					tools[i].id = i.ToString();
				}
			}
			base.PostLoad();
		}

		protected override void ResolveIcon()
		{
			base.ResolveIcon();
			uiIconColor = color;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (texturePath.NullOrEmpty())
			{
				yield return "missing texturePath";
			}
			if (fertility < 0f)
			{
				yield return string.Concat("Terrain Def ", this, " has no fertility value set.");
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
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Supports".Translate(), array.ToCommaList().CapitalizeFirst(), "Stat_Thing_Terrain_Supports_Desc".Translate(), 2000);
			}
			if (IsFine)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_Terrain_Fine_Name".Translate(), "Stat_Thing_Terrain_Fine_Value".Translate(), "Stat_Thing_Terrain_Fine_Desc".Translate(), 2000);
			}
		}
	}
}
