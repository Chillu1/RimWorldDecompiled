using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IdeoStyleTracker : IExposable
{
	public Ideo ideo;

	private DefMap<HairDef, StyleItemSpawningProperties> hairFrequencies;

	private DefMap<BeardDef, StyleItemSpawningProperties> beardFrequencies;

	private DefMap<TattooDef, StyleItemSpawningProperties> tattooFrequencies;

	private Dictionary<ThingDef, StyleCategoryPair> styleForThingDef = new Dictionary<ThingDef, StyleCategoryPair>();

	private int hairAndBeardStylesAvailable = -1;

	private int tattooStylesAvailable = -1;

	private HairDef cachedDisplayHairDef;

	private TattooDef cachedDisplayTattooDef;

	private List<StyleItemTagWeighted> memberStyleTags = new List<StyleItemTagWeighted>();

	public int NumHairAndBeardStylesAvailable
	{
		get
		{
			if (hairAndBeardStylesAvailable < 0)
			{
				RecacheStyleItemCounts();
			}
			return hairAndBeardStylesAvailable;
		}
	}

	public int NumTattooStylesAvailable
	{
		get
		{
			if (tattooStylesAvailable < 0)
			{
				RecacheStyleItemCounts();
			}
			return tattooStylesAvailable;
		}
	}

	public HairDef DisplayedHairDef
	{
		get
		{
			if (cachedDisplayHairDef == null)
			{
				if (hairFrequencies == null)
				{
					return null;
				}
				Rand.PushState(ideo.id);
				cachedDisplayHairDef = hairFrequencies.RandomElementByWeight((KeyValuePair<HairDef, StyleItemSpawningProperties> x) => (float)x.Value.frequency).Key;
				Rand.PopState();
			}
			return cachedDisplayHairDef;
		}
	}

	public TattooDef DisplayedTattooDef
	{
		get
		{
			if (cachedDisplayTattooDef == null)
			{
				if (tattooFrequencies == null)
				{
					return null;
				}
				Rand.PushState(ideo.id);
				if (!tattooFrequencies.TryRandomElementByWeight((KeyValuePair<TattooDef, StyleItemSpawningProperties> x) => GetWeight(x, ignoreFrequency: false), out var result))
				{
					tattooFrequencies.TryRandomElementByWeight((KeyValuePair<TattooDef, StyleItemSpawningProperties> x) => GetWeight(x, ignoreFrequency: true), out result);
				}
				Rand.PopState();
				cachedDisplayTattooDef = result.Key;
			}
			return cachedDisplayTattooDef;
			static int GetWeight(KeyValuePair<TattooDef, StyleItemSpawningProperties> pair, bool ignoreFrequency)
			{
				if (pair.Key == TattooDefOf.NoTattoo_Body || pair.Key == TattooDefOf.NoTattoo_Face)
				{
					return 0;
				}
				if (ignoreFrequency)
				{
					return 1;
				}
				return (int)pair.Value.frequency;
			}
		}
	}

	public IdeoStyleTracker()
	{
	}

	public IdeoStyleTracker(Ideo ideo)
	{
		this.ideo = ideo;
	}

	private void InitializeDefMaps()
	{
		hairFrequencies = new DefMap<HairDef, StyleItemSpawningProperties>();
		beardFrequencies = new DefMap<BeardDef, StyleItemSpawningProperties>();
		tattooFrequencies = new DefMap<TattooDef, StyleItemSpawningProperties>();
	}

	public StyleItemFrequency GetFrequency(StyleItemDef def)
	{
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			RecalculateAvailableStyleItems();
		}
		if (def is HairDef def2)
		{
			return hairFrequencies[def2].frequency;
		}
		if (def is BeardDef def3)
		{
			return beardFrequencies[def3].frequency;
		}
		if (def is TattooDef def4)
		{
			return tattooFrequencies[def4].frequency;
		}
		return StyleItemFrequency.Never;
	}

	public StyleGender GetGender(StyleItemDef def)
	{
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			RecalculateAvailableStyleItems();
		}
		if (def is HairDef def2)
		{
			return hairFrequencies[def2].gender;
		}
		if (def is BeardDef def3)
		{
			return beardFrequencies[def3].gender;
		}
		if (def is TattooDef def4)
		{
			return tattooFrequencies[def4].gender;
		}
		return StyleGender.Any;
	}

	public void SetFrequency(StyleItemDef def, StyleItemFrequency freq)
	{
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			RecalculateAvailableStyleItems();
		}
		if (def is HairDef def2)
		{
			hairFrequencies[def2].frequency = freq;
		}
		else if (def is BeardDef def3)
		{
			beardFrequencies[def3].frequency = freq;
		}
		else if (def is TattooDef def4)
		{
			tattooFrequencies[def4].frequency = freq;
		}
		hairAndBeardStylesAvailable = -1;
		tattooStylesAvailable = -1;
		cachedDisplayHairDef = null;
		cachedDisplayTattooDef = null;
	}

	public void SetGender(StyleItemDef def, StyleGender gender)
	{
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			RecalculateAvailableStyleItems();
		}
		if (def is HairDef def2)
		{
			hairFrequencies[def2].gender = gender;
		}
		else if (def is BeardDef def3)
		{
			beardFrequencies[def3].gender = gender;
		}
		else if (def is TattooDef def4)
		{
			tattooFrequencies[def4].gender = gender;
		}
	}

	public void EnsureAtLeastOneStyleItemAvailable()
	{
		bool flag = false;
		foreach (KeyValuePair<HairDef, StyleItemSpawningProperties> hairFrequency in hairFrequencies)
		{
			if (hairFrequency.Value.frequency > StyleItemFrequency.Never)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			hairFrequencies[HairDefOf.Bald].frequency = StyleItemFrequency.Normal;
			hairFrequencies[HairDefOf.Bald].gender = StyleGender.Any;
		}
		flag = false;
		foreach (KeyValuePair<BeardDef, StyleItemSpawningProperties> beardFrequency in beardFrequencies)
		{
			if (beardFrequency.Value.frequency != StyleItemFrequency.Never)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			beardFrequencies[BeardDefOf.NoBeard].frequency = StyleItemFrequency.Normal;
			beardFrequencies[BeardDefOf.NoBeard].gender = StyleGender.Any;
		}
		bool flag2 = false;
		bool flag3 = false;
		foreach (KeyValuePair<TattooDef, StyleItemSpawningProperties> tattooFrequency in tattooFrequencies)
		{
			if (tattooFrequency.Value.frequency != StyleItemFrequency.Never)
			{
				if (tattooFrequency.Key.tattooType == TattooType.Face)
				{
					flag2 = true;
				}
				else if (tattooFrequency.Key.tattooType == TattooType.Body)
				{
					flag3 = true;
				}
			}
			if (flag2 && flag3)
			{
				break;
			}
		}
		if (!flag2)
		{
			tattooFrequencies[TattooDefOf.NoTattoo_Face].frequency = StyleItemFrequency.Normal;
			tattooFrequencies[TattooDefOf.NoTattoo_Face].gender = StyleGender.Any;
		}
		if (!flag3)
		{
			tattooFrequencies[TattooDefOf.NoTattoo_Body].frequency = StyleItemFrequency.Normal;
			tattooFrequencies[TattooDefOf.NoTattoo_Face].gender = StyleGender.Any;
		}
		hairAndBeardStylesAvailable = -1;
		tattooStylesAvailable = -1;
	}

	private void RecacheStyleItemCounts()
	{
		if (ideo.culture == null)
		{
			return;
		}
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			RecalculateAvailableStyleItems();
		}
		hairAndBeardStylesAvailable = 0;
		foreach (HairDef allDef in DefDatabase<HairDef>.AllDefs)
		{
			if (hairFrequencies[allDef].frequency != StyleItemFrequency.Never)
			{
				hairAndBeardStylesAvailable++;
			}
		}
		foreach (BeardDef allDef2 in DefDatabase<BeardDef>.AllDefs)
		{
			if (beardFrequencies[allDef2].frequency != StyleItemFrequency.Never)
			{
				hairAndBeardStylesAvailable++;
			}
		}
		tattooStylesAvailable = 0;
		foreach (TattooDef allDef3 in DefDatabase<TattooDef>.AllDefs)
		{
			if (allDef3 != TattooDefOf.NoTattoo_Face && allDef3 != TattooDefOf.NoTattoo_Body && tattooFrequencies[allDef3].frequency != StyleItemFrequency.Never)
			{
				tattooStylesAvailable++;
			}
		}
	}

	public void RecalculateAvailableStyleItems()
	{
		if (!ideo.culture.styleItemTags.NullOrEmpty())
		{
			memberStyleTags.AddRange(ideo.culture.styleItemTags);
		}
		for (int i = 0; i < ideo.memes.Count; i++)
		{
			if (ideo.memes[i].styleItemTags.NullOrEmpty())
			{
				continue;
			}
			int j;
			for (j = 0; j < ideo.memes[i].styleItemTags.Count; j++)
			{
				StyleItemTagWeighted styleItemTagWeighted = memberStyleTags.Find((StyleItemTagWeighted x) => x.Tag == ideo.memes[i].styleItemTags[j].Tag);
				if (styleItemTagWeighted == null)
				{
					memberStyleTags.Add(ideo.memes[i].styleItemTags[j]);
				}
				else
				{
					styleItemTagWeighted.Add(ideo.memes[i].styleItemTags[j]);
				}
			}
		}
		InitializeDefMaps();
		SetupFrequency<HairDef>();
		SetupFrequency<BeardDef>();
		SetupFrequency<TattooDef>();
		EnsureAtLeastOneStyleItemAvailable();
		memberStyleTags.Clear();
		void SetupFrequency<T>() where T : StyleItemDef
		{
			foreach (T allDef in DefDatabase<T>.AllDefs)
			{
				float freq = PawnStyleItemChooser.StyleItemChoiceLikelihoodFromTags(allDef, memberStyleTags);
				bool flag = allDef is TattooDef && ideo.classicMode;
				SetFrequency(allDef, (!flag) ? PawnStyleItemChooser.GetStyleItemFrequency(freq) : StyleItemFrequency.Never);
				SetGender(allDef, allDef.styleGender);
			}
		}
	}

	private bool NeedToRecacheStyleItems()
	{
		if (hairFrequencies == null || beardFrequencies == null || tattooFrequencies == null)
		{
			return true;
		}
		foreach (KeyValuePair<HairDef, StyleItemSpawningProperties> hairFrequency in hairFrequencies)
		{
			if (hairFrequency.Value.frequency != StyleItemFrequency.Never)
			{
				return false;
			}
		}
		foreach (KeyValuePair<BeardDef, StyleItemSpawningProperties> beardFrequency in beardFrequencies)
		{
			if (beardFrequency.Value.frequency != StyleItemFrequency.Never)
			{
				return false;
			}
		}
		foreach (KeyValuePair<TattooDef, StyleItemSpawningProperties> tattooFrequency in tattooFrequencies)
		{
			if (tattooFrequency.Value.frequency != StyleItemFrequency.Never)
			{
				return false;
			}
		}
		return true;
	}

	public StyleCategoryPair StyleForThingDef(ThingDef thing, Precept precept = null)
	{
		if (thing == null)
		{
			return null;
		}
		if (styleForThingDef.ContainsKey(thing))
		{
			return styleForThingDef[thing];
		}
		if (Find.IdeoManager.classicMode)
		{
			foreach (StyleCategoryDef selectedStyleCategory in Find.IdeoManager.selectedStyleCategories)
			{
				ThingStyleDef thingStyleDef = selectedStyleCategory.GetStyleForThingDef(thing, precept);
				if (thingStyleDef != null)
				{
					StyleCategoryPair styleCategoryPair = new StyleCategoryPair
					{
						styleDef = thingStyleDef,
						category = selectedStyleCategory
					};
					styleForThingDef.Add(thing, styleCategoryPair);
					return styleCategoryPair;
				}
			}
		}
		foreach (ThingStyleCategoryWithPriority thingStyleCategory in ideo.thingStyleCategories)
		{
			ThingStyleDef thingStyleDef2 = thingStyleCategory.category.GetStyleForThingDef(thing, precept);
			if (thingStyleDef2 != null)
			{
				StyleCategoryPair styleCategoryPair2 = new StyleCategoryPair
				{
					styleDef = thingStyleDef2,
					category = thingStyleCategory.category
				};
				styleForThingDef.Add(thing, styleCategoryPair2);
				return styleCategoryPair2;
			}
		}
		return null;
	}

	public void SetStyleForThingDef(ThingDef thing, StyleCategoryPair styleAndCat)
	{
		styleForThingDef.SetOrAdd(thing, styleAndCat);
	}

	public void ResetStylesForThingDef()
	{
		styleForThingDef.RemoveAll((KeyValuePair<ThingDef, StyleCategoryPair> kvp) => !ideo.PreceptsListForReading.Any((Precept p) => p is Precept_ThingDef precept_ThingDef && precept_ThingDef.ThingDef == kvp.Key) || !kvp.Key.canEditAnyStyle);
	}

	public void ResetStyleForThing(ThingDef thingDef)
	{
		styleForThingDef.Remove(thingDef);
	}

	public virtual void CopyTo(IdeoStyleTracker other)
	{
		other.InitializeDefMaps();
		foreach (KeyValuePair<HairDef, StyleItemSpawningProperties> hairFrequency in hairFrequencies)
		{
			other.hairFrequencies[hairFrequency.Key] = hairFrequency.Value;
		}
		foreach (KeyValuePair<BeardDef, StyleItemSpawningProperties> beardFrequency in beardFrequencies)
		{
			other.beardFrequencies[beardFrequency.Key] = beardFrequency.Value;
		}
		foreach (KeyValuePair<TattooDef, StyleItemSpawningProperties> tattooFrequency in tattooFrequencies)
		{
			other.tattooFrequencies[tattooFrequency.Key] = tattooFrequency.Value;
		}
		other.styleForThingDef.Clear();
		other.styleForThingDef.AddRange(styleForThingDef);
		other.cachedDisplayHairDef = cachedDisplayHairDef;
		other.cachedDisplayTattooDef = cachedDisplayTattooDef;
		other.RecacheStyleItemCounts();
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref hairFrequencies, "hairFrequencies");
		Scribe_Deep.Look(ref beardFrequencies, "beardFrequencies");
		Scribe_Deep.Look(ref tattooFrequencies, "tattooFrequencies");
		Scribe_Collections.Look(ref styleForThingDef, "styleForThingDef", LookMode.Def, LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars && NeedToRecacheStyleItems())
		{
			RecalculateAvailableStyleItems();
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (styleForThingDef == null)
		{
			styleForThingDef = new Dictionary<ThingDef, StyleCategoryPair>();
			return;
		}
		styleForThingDef.RemoveAll((KeyValuePair<ThingDef, StyleCategoryPair> x) => x.Key == null || x.Value == null);
	}
}
