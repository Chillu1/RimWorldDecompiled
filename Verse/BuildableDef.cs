using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class BuildableDef : Def
	{
		public List<StatModifier> statBases;

		public Traversability passability;

		public int pathCost;

		public bool pathCostIgnoreRepeat = true;

		public float fertility = -1f;

		public List<ThingDefCountClass> costList;

		public int costStuffCount;

		public List<StuffCategoryDef> stuffCategories;

		public int placingDraggableDimensions;

		public bool clearBuildingArea = true;

		public Rot4 defaultPlacingRot = Rot4.North;

		public float resourcesFractionWhenDeconstructed = 0.75f;

		public bool useStuffTerrainAffordance;

		public TerrainAffordanceDef terrainAffordanceNeeded;

		public List<ThingDef> buildingPrerequisites;

		public List<ResearchProjectDef> researchPrerequisites;

		public int constructionSkillPrerequisite;

		public int artisticSkillPrerequisite;

		public TechLevel minTechLevelToBuild;

		public TechLevel maxTechLevelToBuild;

		public AltitudeLayer altitudeLayer = AltitudeLayer.Item;

		public EffecterDef repairEffect;

		public EffecterDef constructEffect;

		public List<ColorForStuff> colorPerStuff;

		public bool menuHidden;

		public float specialDisplayRadius;

		public List<Type> placeWorkers;

		public DesignationCategoryDef designationCategory;

		public DesignatorDropdownGroupDef designatorDropdown;

		public KeyBindingDef designationHotKey;

		[NoTranslate]
		public string uiIconPath;

		public Vector2 uiIconOffset;

		public Color uiIconColor = Color.white;

		public int uiIconForStackCount = -1;

		[Unsaved(false)]
		public ThingDef blueprintDef;

		[Unsaved(false)]
		public ThingDef installBlueprintDef;

		[Unsaved(false)]
		public ThingDef frameDef;

		[Unsaved(false)]
		private List<PlaceWorker> placeWorkersInstantiatedInt;

		[Unsaved(false)]
		public Graphic graphic = BaseContent.BadGraphic;

		[Unsaved(false)]
		public Texture2D uiIcon = BaseContent.BadTex;

		[Unsaved(false)]
		public float uiIconAngle;

		public virtual IntVec2 Size => new IntVec2(1, 1);

		public bool MadeFromStuff => !stuffCategories.NullOrEmpty();

		public bool BuildableByPlayer => designationCategory != null;

		public Material DrawMatSingle
		{
			get
			{
				if (graphic == null)
				{
					return null;
				}
				return graphic.MatSingle;
			}
		}

		public float Altitude => altitudeLayer.AltitudeFor();

		public bool AffectsFertility => fertility >= 0f;

		public List<PlaceWorker> PlaceWorkers
		{
			get
			{
				if (placeWorkers == null)
				{
					return null;
				}
				placeWorkersInstantiatedInt = new List<PlaceWorker>();
				foreach (Type placeWorker in placeWorkers)
				{
					placeWorkersInstantiatedInt.Add((PlaceWorker)Activator.CreateInstance(placeWorker));
				}
				return placeWorkersInstantiatedInt;
			}
		}

		public bool IsResearchFinished
		{
			get
			{
				if (researchPrerequisites != null)
				{
					for (int i = 0; i < researchPrerequisites.Count; i++)
					{
						if (!researchPrerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public bool ForceAllowPlaceOver(BuildableDef other)
		{
			if (PlaceWorkers == null)
			{
				return false;
			}
			for (int i = 0; i < PlaceWorkers.Count; i++)
			{
				if (PlaceWorkers[i].ForceAllowPlaceOver(other))
				{
					return true;
				}
			}
			return false;
		}

		public override void PostLoad()
		{
			base.PostLoad();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!uiIconPath.NullOrEmpty())
				{
					uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
				}
				else
				{
					ResolveIcon();
				}
			});
		}

		protected virtual void ResolveIcon()
		{
			if (graphic == null || graphic == BaseContent.BadGraphic)
			{
				return;
			}
			Graphic outerGraphic = graphic;
			if (uiIconForStackCount >= 1 && this is ThingDef)
			{
				Graphic_StackCount graphic_StackCount = graphic as Graphic_StackCount;
				if (graphic_StackCount != null)
				{
					outerGraphic = graphic_StackCount.SubGraphicForStackCount(uiIconForStackCount, (ThingDef)this);
				}
			}
			Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(defaultPlacingRot);
			uiIcon = (Texture2D)material.mainTexture;
			uiIconColor = material.color;
		}

		public Color GetColorForStuff(ThingDef stuff)
		{
			if (colorPerStuff.NullOrEmpty())
			{
				return stuff.stuffProps.color;
			}
			for (int i = 0; i < colorPerStuff.Count; i++)
			{
				ColorForStuff colorForStuff = colorPerStuff[i];
				if (colorForStuff.Stuff == stuff)
				{
					return colorForStuff.Color;
				}
			}
			return stuff.stuffProps.color;
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (useStuffTerrainAffordance && !MadeFromStuff)
			{
				yield return "useStuffTerrainAffordance is true but it's not made from stuff";
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
			{
				yield return item;
			}
			if (!BuildableByPlayer)
			{
				yield break;
			}
			IEnumerable<TerrainAffordanceDef> enumerable = Enumerable.Empty<TerrainAffordanceDef>();
			if (PlaceWorkers != null)
			{
				enumerable = enumerable.Concat(PlaceWorkers.SelectMany((PlaceWorker pw) => pw.DisplayAffordances()));
			}
			TerrainAffordanceDef terrainAffordanceNeed = this.GetTerrainAffordanceNeed(req.StuffDef);
			if (terrainAffordanceNeed != null)
			{
				enumerable = enumerable.Concat(terrainAffordanceNeed);
			}
			string[] array = (from ta in enumerable.Distinct()
				orderby ta.order
				select ta.label).ToArray();
			if (array.Length != 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "TerrainRequirement".Translate(), array.ToCommaList().CapitalizeFirst(), "Stat_Thing_TerrainRequirement_Desc".Translate(), 1101);
			}
		}

		public override string ToString()
		{
			return defName;
		}

		public override int GetHashCode()
		{
			return defName.GetHashCode();
		}
	}
}
