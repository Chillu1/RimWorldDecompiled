using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ThingSetMaker_TraderStock : ThingSetMaker
{
	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		TraderKindDef traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement();
		Faction makingFaction = parms.makingFaction;
		PlanetTile forTile = (parms.tile.HasValue ? parms.tile.Value : ((Find.AnyPlayerHomeMap != null) ? Find.AnyPlayerHomeMap.Tile : ((Find.CurrentMap == null) ? PlanetTile.Invalid : Find.CurrentMap.Tile)));
		for (int i = 0; i < traderKindDef.stockGenerators.Count; i++)
		{
			foreach (Thing item in traderKindDef.stockGenerators[i].GenerateThings(forTile, parms.makingFaction))
			{
				if (!item.def.tradeability.TraderCanSell())
				{
					Log.Error(traderKindDef?.ToString() + " generated carrying " + item?.ToString() + " which can't be sold by traders. Ignoring...");
					continue;
				}
				item.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
				outThings.Add(item);
			}
		}
	}

	public float DebugAverageTotalStockValue(TraderKindDef td)
	{
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			traderDef = td,
			tile = PlanetTile.Invalid
		};
		float num = 0f;
		for (int i = 0; i < 50; i++)
		{
			foreach (Thing item in Generate(parms))
			{
				num += item.MarketValue * (float)item.stackCount;
			}
		}
		return num / 50f;
	}

	public string DebugGenerationDataFor(TraderKindDef td)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(td.defName);
		stringBuilder.AppendLine("Average total market value:" + DebugAverageTotalStockValue(td).ToString("F0"));
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			traderDef = td,
			tile = PlanetTile.Invalid
		};
		Find.FactionManager.AllFactionsListForReading.Where((Faction x) => x.def.baseTraderKinds.Contains(td) || x.def.visitorTraderKinds.Contains(td) || x.def.caravanTraderKinds.Contains(td)).TryRandomElement(out parms.makingFaction);
		stringBuilder.AppendLine("Example generated stock:\n\n");
		foreach (Thing item in Generate(parms))
		{
			Thing thing = ((!(item is MinifiedThing minifiedThing)) ? item : minifiedThing.InnerThing);
			string text = thing.LabelCap;
			if (thing is Book book)
			{
				text = "[" + book.def.defName + "]: " + text;
			}
			text = text + " [" + (thing.MarketValue * (float)thing.stackCount).ToString("F0") + "]";
			stringBuilder.AppendLine(text);
		}
		return stringBuilder.ToString();
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		if (parms.traderDef == null)
		{
			yield break;
		}
		for (int i = 0; i < parms.traderDef.stockGenerators.Count; i++)
		{
			StockGenerator stock = parms.traderDef.stockGenerators[i];
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.tradeability.TraderCanSell() && stock.HandlesThingDef(x)))
			{
				yield return item;
			}
		}
	}
}
