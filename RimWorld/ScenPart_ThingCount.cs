using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class ScenPart_ThingCount : ScenPart
{
	protected ThingDef thingDef;

	protected ThingDef stuff;

	protected int count = 1;

	protected QualityCategory? quality;

	private string countBuf;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref count, "count", 1);
		Scribe_Values.Look(ref quality, "quality");
	}

	public override void Randomize()
	{
		thingDef = PossibleThingDefs().RandomElement();
		stuff = GenStuff.RandomStuffFor(thingDef);
		if (thingDef.statBases.StatListContains(StatDefOf.MarketValue))
		{
			float num = Rand.Range(200, 2000);
			float statValueAbstract = thingDef.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
			count = Mathf.CeilToInt(num / statValueAbstract);
		}
		else
		{
			count = Rand.RangeInclusive(1, 100);
		}
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 4f);
		Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 4f);
		Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height / 4f, scenPartRect.width, scenPartRect.height / 4f);
		Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * 2f / 4f, scenPartRect.width, scenPartRect.height / 4f);
		Rect rect4 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * 3f / 4f, scenPartRect.width, scenPartRect.height / 4f);
		if (Widgets.ButtonText(rect, thingDef.LabelCap))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingDef item in from t in PossibleThingDefs()
				orderby t.label
				select t)
			{
				ThingDef localTd = item;
				list.Add(new FloatMenuOption(localTd.LabelCap, delegate
				{
					thingDef = localTd;
					stuff = GenStuff.DefaultStuffFor(localTd);
					quality = null;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		if (thingDef.MadeFromStuff && Widgets.ButtonText(rect2, stuff.LabelCap))
		{
			List<FloatMenuOption> list2 = new List<FloatMenuOption>();
			foreach (ThingDef item2 in from t in GenStuff.AllowedStuffsFor(thingDef)
				orderby t.label
				select t)
			{
				ThingDef localSd = item2;
				list2.Add(new FloatMenuOption(localSd.LabelCap, delegate
				{
					stuff = localSd;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list2));
		}
		if (thingDef.HasComp(typeof(CompQuality)))
		{
			string str = (quality.HasValue ? quality.Value.GetLabel() : "Default".Translate().ToString());
			if (Widgets.ButtonText(rect3, str.CapitalizeFirst()))
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>
				{
					new FloatMenuOption("Default".Translate().CapitalizeFirst(), delegate
					{
						quality = null;
					})
				};
				foreach (QualityCategory allQualityCategory in QualityUtility.AllQualityCategories)
				{
					QualityCategory localQ = allQualityCategory;
					list3.Add(new FloatMenuOption(allQualityCategory.GetLabel().CapitalizeFirst(), delegate
					{
						quality = localQ;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list3));
			}
		}
		Widgets.TextFieldNumeric(rect4, ref count, ref countBuf, 1f);
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_ThingCount scenPart_ThingCount && GetType() == scenPart_ThingCount.GetType() && thingDef == scenPart_ThingCount.thingDef && stuff == scenPart_ThingCount.stuff && quality == scenPart_ThingCount.quality && count >= 0 && scenPart_ThingCount.count >= 0)
		{
			count += scenPart_ThingCount.count;
			return true;
		}
		return false;
	}

	protected virtual IEnumerable<ThingDef> PossibleThingDefs()
	{
		return DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => (d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop) || (d.category == ThingCategory.Building && d.Minifiable));
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs() && thingDef != null)
		{
			if (thingDef.MadeFromStuff)
			{
				return stuff == null;
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		if (thingDef != null)
		{
			num ^= thingDef.GetHashCode();
		}
		if (stuff != null)
		{
			num ^= stuff.GetHashCode();
		}
		num ^= count;
		return num ^ quality.GetHashCode();
	}
}
