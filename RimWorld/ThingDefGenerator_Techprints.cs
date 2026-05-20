using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class ThingDefGenerator_Techprints
{
	public const string Tag = "Techprint";

	public static IEnumerable<ThingDef> ImpliedTechprintDefs(bool hotReload = false)
	{
		if (!ModLister.RoyaltyInstalled)
		{
			yield break;
		}
		foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
		{
			if (item.TechprintCount > 0)
			{
				string defName = "Techprint_" + item.defName;
				ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? new ThingDef()) : new ThingDef());
				thingDef.resourceReadoutPriority = ResourceCountPriority.Middle;
				thingDef.drawerType = DrawerType.MapMeshOnly;
				thingDef.category = ThingCategory.Item;
				thingDef.thingClass = typeof(ThingWithComps);
				thingDef.thingCategories = new List<ThingCategoryDef>();
				thingDef.thingCategories.Add(ThingCategoryDefOf.Techprints);
				thingDef.graphicData = new GraphicData();
				thingDef.graphicData.graphicClass = typeof(Graphic_Single);
				thingDef.useHitPoints = true;
				thingDef.selectable = true;
				thingDef.thingSetMakerTags = new List<string>();
				thingDef.thingSetMakerTags.Add("Techprint");
				thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
				thingDef.SetStatBaseValue(StatDefOf.Flammability, 1f);
				thingDef.SetStatBaseValue(StatDefOf.MarketValue, item.techprintMarketValue);
				thingDef.SetStatBaseValue(StatDefOf.Mass, 0.03f);
				thingDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);
				thingDef.altitudeLayer = AltitudeLayer.Item;
				thingDef.comps.Add(new CompProperties_Forbiddable());
				thingDef.comps.Add(new CompProperties_Techprint
				{
					project = item
				});
				thingDef.tickerType = TickerType.Never;
				thingDef.alwaysHaulable = true;
				thingDef.rotatable = false;
				thingDef.pathCost = 14;
				thingDef.drawGUIOverlay = true;
				thingDef.modContentPack = item.modContentPack;
				thingDef.tradeTags = new List<string>();
				thingDef.tradeTags.Add("Techprint");
				thingDef.category = ThingCategory.Item;
				thingDef.description = "TechprintDesc".Translate(NamedArgumentUtility.Named(item, "PROJECT")) + "\n\n" + item.LabelCap + "\n\n" + item.description;
				thingDef.useHitPoints = true;
				if (thingDef.thingCategories == null)
				{
					thingDef.thingCategories = new List<ThingCategoryDef>();
				}
				thingDef.graphicData.texPath = "Things/Item/Special/TechprintUltratech";
				thingDef.defName = defName;
				thingDef.label = "TechprintLabel".Translate(NamedArgumentUtility.Named(item, "PROJECT"));
				yield return thingDef;
			}
		}
	}
}
