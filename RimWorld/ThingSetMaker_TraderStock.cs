using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_TraderStock : ThingSetMaker
	{
		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			TraderKindDef traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement();
			Faction makingFaction = parms.makingFaction;
			int forTile = parms.tile.HasValue ? parms.tile.Value : ((Find.AnyPlayerHomeMap != null) ? Find.AnyPlayerHomeMap.Tile : ((Find.CurrentMap == null) ? (-1) : Find.CurrentMap.Tile));
			for (int i = 0; i < traderKindDef.stockGenerators.Count; i++)
			{
				foreach (Thing item in traderKindDef.stockGenerators[i].GenerateThings(forTile, parms.makingFaction))
				{
					if (!item.def.tradeability.TraderCanSell())
					{
						Log.Error(string.Concat(traderKindDef, " generated carrying ", item, " which can't be sold by traders. Ignoring..."));
					}
					else
					{
						item.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
						outThings.Add(item);
					}
				}
			}
		}

		public float DebugAverageTotalStockValue(TraderKindDef td)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = td;
			parms.tile = -1;
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
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = td;
			parms.tile = -1;
			Find.FactionManager.AllFactionsListForReading.Where((Faction x) => x.def.baseTraderKinds.Contains(td) || x.def.visitorTraderKinds.Contains(td) || x.def.caravanTraderKinds.Contains(td)).TryRandomElement(out parms.makingFaction);
			stringBuilder.AppendLine("Example generated stock:\n\n");
			foreach (Thing item in Generate(parms))
			{
				MinifiedThing minifiedThing = item as MinifiedThing;
				Thing thing = (minifiedThing == null) ? item : minifiedThing.InnerThing;
				string labelCap = thing.LabelCap;
				labelCap = labelCap + " [" + (thing.MarketValue * (float)thing.stackCount).ToString("F0") + "]";
				stringBuilder.AppendLine(labelCap);
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
}
