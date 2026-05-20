using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompStyleable : ThingComp
{
	private Precept_ThingStyle sourcePrecept;

	public ThingStyleDef styleDef;

	public bool everSeenByPlayer;

	[Unsaved(false)]
	public StyleCategoryDef cachedStyleCategoryDef;

	public Ideo Ideo => SourcePrecept?.ideo;

	public StyleCategoryDef StyleCategoryDef
	{
		get
		{
			if (cachedStyleCategoryDef == null)
			{
				bool flag = false;
				foreach (StyleCategoryDef allDef in DefDatabase<StyleCategoryDef>.AllDefs)
				{
					for (int i = 0; i < allDef.thingDefStyles.Count; i++)
					{
						if (allDef.thingDefStyles[i].StyleDef == styleDef && allDef.thingDefStyles[i].ThingDef == parent.def)
						{
							cachedStyleCategoryDef = allDef;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			return cachedStyleCategoryDef;
		}
	}

	public Precept_ThingStyle SourcePrecept
	{
		get
		{
			return sourcePrecept;
		}
		set
		{
			if (sourcePrecept == value)
			{
				return;
			}
			sourcePrecept = value;
			if (!(parent.def.randomStyleChance > 0f))
			{
				BuildableDef buildableDef = (parent.def.IsBlueprint ? parent.def.entityDefToBuild : parent.def);
				StyleCategoryPair styleCategoryPair = sourcePrecept?.ideo.style.StyleForThingDef((ThingDef)buildableDef, sourcePrecept);
				cachedStyleCategoryDef = styleCategoryPair?.category;
				if (cachedStyleCategoryDef != null)
				{
					styleDef = styleCategoryPair.styleDef;
				}
			}
		}
	}

	public override string TransformLabel(string label)
	{
		if (sourcePrecept != null)
		{
			label = sourcePrecept.TransformThingLabel(label);
		}
		return label;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (sourcePrecept == null)
		{
			return text;
		}
		string text2 = sourcePrecept.InspectStringExtra(parent);
		if (text2.NullOrEmpty())
		{
			return text;
		}
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + text2;
	}

	public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
	{
		StyleCategoryPair styleCategoryPair = forFaction?.ideos?.PrimaryIdeo?.style?.StyleForThingDef(parent.def);
		if (styleCategoryPair != null)
		{
			parent.SetStyleDef(styleCategoryPair.styleDef);
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (StyleCategoryDef != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Stat_Thing_StyleLabel".Translate(), StyleCategoryDef.LabelCap, "Stat_Thing_StyleDesc".Translate(), 1108);
		}
		if (SourcePrecept == null)
		{
			yield break;
		}
		foreach (StatDrawEntry item in SourcePrecept.SpecialDisplayStats(parent))
		{
			yield return item;
		}
	}

	public override void PostExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving && sourcePrecept != null && (sourcePrecept.ideo == null || !Find.IdeoManager.IdeosListForReading.Contains(sourcePrecept.ideo)))
		{
			sourcePrecept = null;
		}
		Scribe_References.Look(ref sourcePrecept, "sourcePrecept");
		Scribe_Defs.Look(ref styleDef, "styleDef");
		Scribe_Values.Look(ref everSeenByPlayer, "everSeenByPlayer", defaultValue: false);
	}
}
