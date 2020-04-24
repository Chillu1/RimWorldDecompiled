using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class ThingFilter : IExposable
	{
		[Unsaved(false)]
		private Action settingsChangedCallback;

		[Unsaved(false)]
		private TreeNode_ThingCategory displayRootCategoryInt;

		[Unsaved(false)]
		private HashSet<ThingDef> allowedDefs = new HashSet<ThingDef>();

		[Unsaved(false)]
		private List<SpecialThingFilterDef> disallowedSpecialFilters = new List<SpecialThingFilterDef>();

		private FloatRange allowedHitPointsPercents = FloatRange.ZeroToOne;

		public bool allowedHitPointsConfigurable = true;

		private QualityRange allowedQualities = QualityRange.All;

		public bool allowedQualitiesConfigurable = true;

		[MustTranslate]
		public string customSummary;

		private List<ThingDef> thingDefs;

		[NoTranslate]
		private List<string> categories;

		[NoTranslate]
		private List<string> tradeTagsToAllow;

		[NoTranslate]
		private List<string> tradeTagsToDisallow;

		[NoTranslate]
		private List<string> thingSetMakerTagsToAllow;

		[NoTranslate]
		private List<string> thingSetMakerTagsToDisallow;

		[NoTranslate]
		private List<string> disallowedCategories;

		[NoTranslate]
		private List<string> specialFiltersToAllow;

		[NoTranslate]
		private List<string> specialFiltersToDisallow;

		private List<StuffCategoryDef> stuffCategoriesToAllow;

		private List<ThingDef> allowAllWhoCanMake;

		private FoodPreferability disallowWorsePreferability;

		private bool disallowInedibleByHuman;

		private Type allowWithComp;

		private Type disallowWithComp;

		private float disallowCheaperThan = float.MinValue;

		private List<ThingDef> disallowedThingDefs;

		public string Summary
		{
			get
			{
				if (!customSummary.NullOrEmpty())
				{
					return customSummary;
				}
				if (thingDefs != null && thingDefs.Count == 1 && categories.NullOrEmpty() && tradeTagsToAllow.NullOrEmpty() && tradeTagsToDisallow.NullOrEmpty() && thingSetMakerTagsToAllow.NullOrEmpty() && thingSetMakerTagsToDisallow.NullOrEmpty() && disallowedCategories.NullOrEmpty() && specialFiltersToAllow.NullOrEmpty() && specialFiltersToDisallow.NullOrEmpty() && stuffCategoriesToAllow.NullOrEmpty() && allowAllWhoCanMake.NullOrEmpty() && disallowWorsePreferability == FoodPreferability.Undefined && !disallowInedibleByHuman && allowWithComp == null && disallowWithComp == null && disallowCheaperThan == float.MinValue && disallowedThingDefs.NullOrEmpty())
				{
					return thingDefs[0].label;
				}
				if (thingDefs.NullOrEmpty() && categories != null && categories.Count == 1 && tradeTagsToAllow.NullOrEmpty() && tradeTagsToDisallow.NullOrEmpty() && thingSetMakerTagsToAllow.NullOrEmpty() && thingSetMakerTagsToDisallow.NullOrEmpty() && disallowedCategories.NullOrEmpty() && specialFiltersToAllow.NullOrEmpty() && specialFiltersToDisallow.NullOrEmpty() && stuffCategoriesToAllow.NullOrEmpty() && allowAllWhoCanMake.NullOrEmpty() && disallowWorsePreferability == FoodPreferability.Undefined && !disallowInedibleByHuman && allowWithComp == null && disallowWithComp == null && disallowCheaperThan == float.MinValue && disallowedThingDefs.NullOrEmpty())
				{
					return DefDatabase<ThingCategoryDef>.GetNamed(categories[0]).label;
				}
				if (allowedDefs.Count == 1)
				{
					return allowedDefs.First().label;
				}
				return "UsableIngredients".Translate();
			}
		}

		public ThingRequest BestThingRequest
		{
			get
			{
				if (allowedDefs.Count == 1)
				{
					return ThingRequest.ForDef(allowedDefs.First());
				}
				bool flag = true;
				bool flag2 = true;
				foreach (ThingDef allowedDef in allowedDefs)
				{
					if (!allowedDef.EverHaulable)
					{
						flag = false;
					}
					if (allowedDef.category != ThingCategory.Pawn)
					{
						flag2 = false;
					}
				}
				if (flag)
				{
					return ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
				}
				if (flag2)
				{
					return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
				}
				return ThingRequest.ForGroup(ThingRequestGroup.Everything);
			}
		}

		public ThingDef AnyAllowedDef => allowedDefs.FirstOrDefault();

		public IEnumerable<ThingDef> AllowedThingDefs => allowedDefs;

		private static IEnumerable<ThingDef> AllStorableThingDefs => DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.EverStorable(willMinifyIfPossible: true));

		public int AllowedDefCount => allowedDefs.Count;

		public FloatRange AllowedHitPointsPercents
		{
			get
			{
				return allowedHitPointsPercents;
			}
			set
			{
				if (!(allowedHitPointsPercents == value))
				{
					allowedHitPointsPercents = value;
					if (settingsChangedCallback != null)
					{
						settingsChangedCallback();
					}
				}
			}
		}

		public QualityRange AllowedQualityLevels
		{
			get
			{
				return allowedQualities;
			}
			set
			{
				if (!(allowedQualities == value))
				{
					allowedQualities = value;
					if (settingsChangedCallback != null)
					{
						settingsChangedCallback();
					}
				}
			}
		}

		public TreeNode_ThingCategory DisplayRootCategory
		{
			get
			{
				if (displayRootCategoryInt == null)
				{
					RecalculateDisplayRootCategory();
				}
				if (displayRootCategoryInt == null)
				{
					return ThingCategoryNodeDatabase.RootNode;
				}
				return displayRootCategoryInt;
			}
			set
			{
				if (value != displayRootCategoryInt)
				{
					displayRootCategoryInt = value;
					RecalculateSpecialFilterConfigurability();
				}
			}
		}

		public ThingFilter()
		{
		}

		public ThingFilter(Action settingsChangedCallback)
		{
			this.settingsChangedCallback = settingsChangedCallback;
		}

		public virtual void ExposeData()
		{
			Scribe_Collections.Look(ref disallowedSpecialFilters, "disallowedSpecialFilters", LookMode.Def);
			Scribe_Collections.Look(ref allowedDefs, "allowedDefs");
			Scribe_Values.Look(ref allowedHitPointsPercents, "allowedHitPointsPercents");
			Scribe_Values.Look(ref allowedQualities, "allowedQualityLevels");
		}

		public void ResolveReferences()
		{
			for (int i = 0; i < DefDatabase<SpecialThingFilterDef>.AllDefsListForReading.Count; i++)
			{
				SpecialThingFilterDef specialThingFilterDef = DefDatabase<SpecialThingFilterDef>.AllDefsListForReading[i];
				if (!specialThingFilterDef.allowedByDefault)
				{
					SetAllow(specialThingFilterDef, allow: false);
				}
			}
			if (thingDefs != null)
			{
				for (int j = 0; j < thingDefs.Count; j++)
				{
					if (thingDefs[j] != null)
					{
						SetAllow(thingDefs[j], allow: true);
					}
					else
					{
						Log.Error("ThingFilter could not find thing def named " + thingDefs[j]);
					}
				}
			}
			if (categories != null)
			{
				for (int k = 0; k < categories.Count; k++)
				{
					ThingCategoryDef named = DefDatabase<ThingCategoryDef>.GetNamed(categories[k]);
					if (named != null)
					{
						SetAllow(named, allow: true);
					}
				}
			}
			if (tradeTagsToAllow != null)
			{
				for (int l = 0; l < tradeTagsToAllow.Count; l++)
				{
					List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int m = 0; m < allDefsListForReading.Count; m++)
					{
						ThingDef thingDef = allDefsListForReading[m];
						if (thingDef.tradeTags != null && thingDef.tradeTags.Contains(tradeTagsToAllow[l]))
						{
							SetAllow(thingDef, allow: true);
						}
					}
				}
			}
			if (tradeTagsToDisallow != null)
			{
				for (int n = 0; n < tradeTagsToDisallow.Count; n++)
				{
					List<ThingDef> allDefsListForReading2 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num = 0; num < allDefsListForReading2.Count; num++)
					{
						ThingDef thingDef2 = allDefsListForReading2[num];
						if (thingDef2.tradeTags != null && thingDef2.tradeTags.Contains(tradeTagsToDisallow[n]))
						{
							SetAllow(thingDef2, allow: false);
						}
					}
				}
			}
			if (thingSetMakerTagsToAllow != null)
			{
				for (int num2 = 0; num2 < thingSetMakerTagsToAllow.Count; num2++)
				{
					List<ThingDef> allDefsListForReading3 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num3 = 0; num3 < allDefsListForReading3.Count; num3++)
					{
						ThingDef thingDef3 = allDefsListForReading3[num3];
						if (thingDef3.thingSetMakerTags != null && thingDef3.thingSetMakerTags.Contains(thingSetMakerTagsToAllow[num2]))
						{
							SetAllow(thingDef3, allow: true);
						}
					}
				}
			}
			if (thingSetMakerTagsToDisallow != null)
			{
				for (int num4 = 0; num4 < thingSetMakerTagsToDisallow.Count; num4++)
				{
					List<ThingDef> allDefsListForReading4 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num5 = 0; num5 < allDefsListForReading4.Count; num5++)
					{
						ThingDef thingDef4 = allDefsListForReading4[num5];
						if (thingDef4.thingSetMakerTags != null && thingDef4.thingSetMakerTags.Contains(thingSetMakerTagsToDisallow[num4]))
						{
							SetAllow(thingDef4, allow: false);
						}
					}
				}
			}
			if (disallowedCategories != null)
			{
				for (int num6 = 0; num6 < disallowedCategories.Count; num6++)
				{
					ThingCategoryDef named2 = DefDatabase<ThingCategoryDef>.GetNamed(disallowedCategories[num6]);
					if (named2 != null)
					{
						SetAllow(named2, allow: false);
					}
				}
			}
			if (specialFiltersToAllow != null)
			{
				for (int num7 = 0; num7 < specialFiltersToAllow.Count; num7++)
				{
					SetAllow(SpecialThingFilterDef.Named(specialFiltersToAllow[num7]), allow: true);
				}
			}
			if (specialFiltersToDisallow != null)
			{
				for (int num8 = 0; num8 < specialFiltersToDisallow.Count; num8++)
				{
					SetAllow(SpecialThingFilterDef.Named(specialFiltersToDisallow[num8]), allow: false);
				}
			}
			if (stuffCategoriesToAllow != null)
			{
				for (int num9 = 0; num9 < stuffCategoriesToAllow.Count; num9++)
				{
					SetAllow(stuffCategoriesToAllow[num9], allow: true);
				}
			}
			if (allowAllWhoCanMake != null)
			{
				for (int num10 = 0; num10 < allowAllWhoCanMake.Count; num10++)
				{
					SetAllowAllWhoCanMake(allowAllWhoCanMake[num10]);
				}
			}
			if (disallowWorsePreferability != 0)
			{
				List<ThingDef> allDefsListForReading5 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num11 = 0; num11 < allDefsListForReading5.Count; num11++)
				{
					ThingDef thingDef5 = allDefsListForReading5[num11];
					if (thingDef5.IsIngestible && thingDef5.ingestible.preferability != 0 && (int)thingDef5.ingestible.preferability < (int)disallowWorsePreferability)
					{
						SetAllow(thingDef5, allow: false);
					}
				}
			}
			if (disallowInedibleByHuman)
			{
				List<ThingDef> allDefsListForReading6 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num12 = 0; num12 < allDefsListForReading6.Count; num12++)
				{
					ThingDef thingDef6 = allDefsListForReading6[num12];
					if (thingDef6.IsIngestible && !ThingDefOf.Human.race.CanEverEat(thingDef6))
					{
						SetAllow(thingDef6, allow: false);
					}
				}
			}
			if (allowWithComp != null)
			{
				List<ThingDef> allDefsListForReading7 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num13 = 0; num13 < allDefsListForReading7.Count; num13++)
				{
					ThingDef thingDef7 = allDefsListForReading7[num13];
					if (thingDef7.HasComp(allowWithComp))
					{
						SetAllow(thingDef7, allow: true);
					}
				}
			}
			if (disallowWithComp != null)
			{
				List<ThingDef> allDefsListForReading8 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num14 = 0; num14 < allDefsListForReading8.Count; num14++)
				{
					ThingDef thingDef8 = allDefsListForReading8[num14];
					if (thingDef8.HasComp(disallowWithComp))
					{
						SetAllow(thingDef8, allow: false);
					}
				}
			}
			if (disallowCheaperThan != float.MinValue)
			{
				List<ThingDef> list = new List<ThingDef>();
				foreach (ThingDef allowedDef in allowedDefs)
				{
					if (allowedDef.BaseMarketValue < disallowCheaperThan)
					{
						list.Add(allowedDef);
					}
				}
				for (int num15 = 0; num15 < list.Count; num15++)
				{
					SetAllow(list[num15], allow: false);
				}
			}
			if (disallowedThingDefs == null)
			{
				return;
			}
			for (int num16 = 0; num16 < disallowedThingDefs.Count; num16++)
			{
				if (disallowedThingDefs[num16] != null)
				{
					SetAllow(disallowedThingDefs[num16], allow: false);
				}
				else
				{
					Log.Error("ThingFilter could not find excepted thing def named " + disallowedThingDefs[num16]);
				}
			}
		}

		public void RecalculateDisplayRootCategory()
		{
			if (ThingCategoryNodeDatabase.allThingCategoryNodes == null)
			{
				DisplayRootCategory = ThingCategoryNodeDatabase.RootNode;
				return;
			}
			int lastFoundCategory = -1;
			object lockObject = new object();
			GenThreading.ParallelFor(0, ThingCategoryNodeDatabase.allThingCategoryNodes.Count, delegate(int index)
			{
				TreeNode_ThingCategory treeNode_ThingCategory = ThingCategoryNodeDatabase.allThingCategoryNodes[index];
				bool flag = false;
				bool flag2 = false;
				foreach (ThingDef allowedDef in allowedDefs)
				{
					if (treeNode_ThingCategory.catDef.ContainedInThisOrDescendant(allowedDef))
					{
						flag2 = true;
					}
					else
					{
						flag = true;
					}
				}
				if (!flag && flag2)
				{
					lock (lockObject)
					{
						if (index > lastFoundCategory)
						{
							lastFoundCategory = index;
						}
					}
				}
			});
			if (lastFoundCategory == -1)
			{
				DisplayRootCategory = ThingCategoryNodeDatabase.RootNode;
			}
			else
			{
				DisplayRootCategory = ThingCategoryNodeDatabase.allThingCategoryNodes[lastFoundCategory];
			}
		}

		private void RecalculateSpecialFilterConfigurability()
		{
			if (DisplayRootCategory == null)
			{
				allowedHitPointsConfigurable = true;
				allowedQualitiesConfigurable = true;
			}
			else
			{
				allowedHitPointsConfigurable = false;
				allowedQualitiesConfigurable = false;
				foreach (ThingDef descendantThingDef in DisplayRootCategory.catDef.DescendantThingDefs)
				{
					if (descendantThingDef.useHitPoints)
					{
						allowedHitPointsConfigurable = true;
					}
					if (descendantThingDef.HasComp(typeof(CompQuality)))
					{
						allowedQualitiesConfigurable = true;
					}
					if (allowedHitPointsConfigurable && allowedQualitiesConfigurable)
					{
						break;
					}
				}
			}
		}

		public bool IsAlwaysDisallowedDueToSpecialFilters(ThingDef def)
		{
			for (int i = 0; i < disallowedSpecialFilters.Count; i++)
			{
				if (disallowedSpecialFilters[i].Worker.AlwaysMatches(def))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void CopyAllowancesFrom(ThingFilter other)
		{
			allowedDefs.Clear();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				SetAllow(allDef, other.Allows(allDef));
			}
			disallowedSpecialFilters = other.disallowedSpecialFilters.ListFullCopyOrNull();
			allowedHitPointsPercents = other.allowedHitPointsPercents;
			allowedHitPointsConfigurable = other.allowedHitPointsConfigurable;
			allowedQualities = other.allowedQualities;
			allowedQualitiesConfigurable = other.allowedQualitiesConfigurable;
			thingDefs = other.thingDefs.ListFullCopyOrNull();
			categories = other.categories.ListFullCopyOrNull();
			tradeTagsToAllow = other.tradeTagsToAllow.ListFullCopyOrNull();
			tradeTagsToDisallow = other.tradeTagsToDisallow.ListFullCopyOrNull();
			thingSetMakerTagsToAllow = other.thingSetMakerTagsToAllow.ListFullCopyOrNull();
			thingSetMakerTagsToDisallow = other.thingSetMakerTagsToDisallow.ListFullCopyOrNull();
			disallowedCategories = other.disallowedCategories.ListFullCopyOrNull();
			specialFiltersToAllow = other.specialFiltersToAllow.ListFullCopyOrNull();
			specialFiltersToDisallow = other.specialFiltersToDisallow.ListFullCopyOrNull();
			stuffCategoriesToAllow = other.stuffCategoriesToAllow.ListFullCopyOrNull();
			allowAllWhoCanMake = other.allowAllWhoCanMake.ListFullCopyOrNull();
			disallowWorsePreferability = other.disallowWorsePreferability;
			disallowInedibleByHuman = other.disallowInedibleByHuman;
			allowWithComp = other.allowWithComp;
			disallowWithComp = other.disallowWithComp;
			disallowCheaperThan = other.disallowCheaperThan;
			disallowedThingDefs = other.disallowedThingDefs.ListFullCopyOrNull();
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
		}

		public void SetAllow(ThingDef thingDef, bool allow)
		{
			if (allow != Allows(thingDef))
			{
				if (allow)
				{
					allowedDefs.Add(thingDef);
				}
				else
				{
					allowedDefs.Remove(thingDef);
				}
				if (settingsChangedCallback != null)
				{
					settingsChangedCallback();
				}
				displayRootCategoryInt = null;
			}
		}

		public void SetAllow(SpecialThingFilterDef sfDef, bool allow)
		{
			if (!sfDef.configurable || allow == Allows(sfDef))
			{
				return;
			}
			if (allow)
			{
				if (disallowedSpecialFilters.Contains(sfDef))
				{
					disallowedSpecialFilters.Remove(sfDef);
				}
			}
			else if (!disallowedSpecialFilters.Contains(sfDef))
			{
				disallowedSpecialFilters.Add(sfDef);
			}
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetAllow(ThingCategoryDef categoryDef, bool allow, IEnumerable<ThingDef> exceptedDefs = null, IEnumerable<SpecialThingFilterDef> exceptedFilters = null)
		{
			if (!ThingCategoryNodeDatabase.initialized)
			{
				Log.Error("SetAllow categories won't work before ThingCategoryDatabase is initialized.");
			}
			foreach (ThingDef descendantThingDef in categoryDef.DescendantThingDefs)
			{
				if (exceptedDefs == null || !exceptedDefs.Contains(descendantThingDef))
				{
					SetAllow(descendantThingDef, allow);
				}
			}
			foreach (SpecialThingFilterDef descendantSpecialThingFilterDef in categoryDef.DescendantSpecialThingFilterDefs)
			{
				if (exceptedFilters == null || !exceptedFilters.Contains(descendantSpecialThingFilterDef))
				{
					SetAllow(descendantSpecialThingFilterDef, allow);
				}
			}
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetAllow(StuffCategoryDef cat, bool allow)
		{
			for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
			{
				ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
				if (thingDef.IsStuff && thingDef.stuffProps.categories.Contains(cat))
				{
					SetAllow(thingDef, allow);
				}
			}
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetAllowAllWhoCanMake(ThingDef thing)
		{
			for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
			{
				ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
				if (thingDef.IsStuff && thingDef.stuffProps.CanMake(thing))
				{
					SetAllow(thingDef, allow: true);
				}
			}
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetFromPreset(StorageSettingsPreset preset)
		{
			if (preset == StorageSettingsPreset.DefaultStockpile)
			{
				SetAllow(ThingCategoryDefOf.Foods, allow: true);
				SetAllow(ThingCategoryDefOf.Manufactured, allow: true);
				SetAllow(ThingCategoryDefOf.ResourcesRaw, allow: true);
				SetAllow(ThingCategoryDefOf.Items, allow: true);
				SetAllow(ThingCategoryDefOf.Buildings, allow: true);
				SetAllow(ThingCategoryDefOf.Weapons, allow: true);
				SetAllow(ThingCategoryDefOf.Apparel, allow: true);
				SetAllow(ThingCategoryDefOf.BodyParts, allow: true);
			}
			if (preset == StorageSettingsPreset.DumpingStockpile)
			{
				SetAllow(ThingCategoryDefOf.Corpses, allow: true);
				SetAllow(ThingCategoryDefOf.Chunks, allow: true);
			}
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetDisallowAll(IEnumerable<ThingDef> exceptedDefs = null, IEnumerable<SpecialThingFilterDef> exceptedFilters = null)
		{
			allowedDefs.RemoveWhere((ThingDef d) => exceptedDefs == null || !exceptedDefs.Contains(d));
			disallowedSpecialFilters.RemoveAll((SpecialThingFilterDef sf) => sf.configurable && (exceptedFilters == null || !exceptedFilters.Contains(sf)));
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public void SetAllowAll(ThingFilter parentFilter, bool includeNonStorable = false)
		{
			allowedDefs.Clear();
			if (parentFilter != null)
			{
				foreach (ThingDef allowedDef in parentFilter.allowedDefs)
				{
					allowedDefs.Add(allowedDef);
				}
			}
			else if (includeNonStorable)
			{
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					allowedDefs.Add(allDef);
				}
			}
			else
			{
				foreach (ThingDef allStorableThingDef in AllStorableThingDefs)
				{
					allowedDefs.Add(allStorableThingDef);
				}
			}
			disallowedSpecialFilters.RemoveAll((SpecialThingFilterDef sf) => sf.configurable);
			if (settingsChangedCallback != null)
			{
				settingsChangedCallback();
			}
			displayRootCategoryInt = null;
		}

		public virtual bool Allows(Thing t)
		{
			t = t.GetInnerIfMinified();
			if (!Allows(t.def))
			{
				return false;
			}
			if (t.def.useHitPoints)
			{
				float f = (float)t.HitPoints / (float)t.MaxHitPoints;
				f = GenMath.RoundedHundredth(f);
				if (!allowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(f)))
				{
					return false;
				}
			}
			if (allowedQualities != QualityRange.All && t.def.FollowQualityThingFilter())
			{
				if (!t.TryGetQuality(out QualityCategory qc))
				{
					qc = QualityCategory.Normal;
				}
				if (!allowedQualities.Includes(qc))
				{
					return false;
				}
			}
			for (int i = 0; i < disallowedSpecialFilters.Count; i++)
			{
				if (disallowedSpecialFilters[i].Worker.Matches(t) && t.def.IsWithinCategory(disallowedSpecialFilters[i].parentCategory))
				{
					return false;
				}
			}
			return true;
		}

		public bool Allows(ThingDef def)
		{
			return allowedDefs.Contains(def);
		}

		public bool Allows(SpecialThingFilterDef sf)
		{
			return !disallowedSpecialFilters.Contains(sf);
		}

		public ThingRequest GetThingRequest()
		{
			if (AllowedThingDefs.Any((ThingDef def) => !def.alwaysHaulable))
			{
				return ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
			}
			return ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways);
		}

		public override string ToString()
		{
			return Summary;
		}
	}
}
