using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Animals : StockGenerator
	{
		[NoTranslate]
		private List<string> tradeTagsSell = new List<string>();

		[NoTranslate]
		private List<string> tradeTagsBuy = new List<string>();

		private IntRange kindCountRange = new IntRange(1, 1);

		private float minWildness;

		private float maxWildness = 1f;

		private bool checkTemperature = true;

		private static readonly SimpleCurve SelectionChanceFromWildnessCurve = new SimpleCurve
		{
			new CurvePoint(0f, 100f),
			new CurvePoint(0.25f, 60f),
			new CurvePoint(0.5f, 30f),
			new CurvePoint(0.75f, 12f),
			new CurvePoint(1f, 2f)
		};

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			int randomInRange = kindCountRange.RandomInRange;
			int count = countRange.RandomInRange;
			List<PawnKindDef> kinds = new List<PawnKindDef>();
			for (int j = 0; j < randomInRange; j++)
			{
				if (!DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => !kinds.Contains(k) && PawnKindAllowed(k, forTile)).TryRandomElementByWeight((PawnKindDef k) => SelectionChance(k), out var result))
				{
					break;
				}
				kinds.Add(result);
			}
			for (int i = 0; i < count; i++)
			{
				if (!kinds.TryRandomElement(out var result2))
				{
					break;
				}
				PawnGenerationRequest request = new PawnGenerationRequest(result2, null, PawnGenerationContext.NonPlayer, forTile);
				yield return PawnGenerator.GeneratePawn(request);
			}
		}

		private float SelectionChance(PawnKindDef k)
		{
			return SelectionChanceFromWildnessCurve.Evaluate(k.RaceProps.wildness);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.category == ThingCategory.Pawn && thingDef.race.Animal && thingDef.tradeability != 0)
			{
				if (!tradeTagsSell.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag)))
				{
					return tradeTagsBuy.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag));
				}
				return true;
			}
			return false;
		}

		private bool PawnKindAllowed(PawnKindDef kind, int forTile)
		{
			if (!kind.RaceProps.Animal || kind.RaceProps.wildness < minWildness || kind.RaceProps.wildness > maxWildness || kind.RaceProps.wildness > 1f)
			{
				return false;
			}
			if (checkTemperature)
			{
				int num = forTile;
				if (num == -1 && Find.AnyPlayerHomeMap != null)
				{
					num = Find.AnyPlayerHomeMap.Tile;
				}
				if (num != -1 && !Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(num, kind.race))
				{
					return false;
				}
			}
			if (kind.race.tradeTags == null)
			{
				return false;
			}
			if (!tradeTagsSell.Any((string x) => kind.race.tradeTags.Contains(x)))
			{
				return false;
			}
			if (!kind.race.tradeability.TraderCanSell())
			{
				return false;
			}
			return true;
		}

		public void LogAnimalChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(allDef.defName + ": " + SelectionChance(allDef).ToString("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		private static void StockGenerationAnimals()
		{
			StockGenerator_Animals stockGenerator_Animals = new StockGenerator_Animals();
			stockGenerator_Animals.tradeTagsSell = new List<string>();
			stockGenerator_Animals.tradeTagsSell.Add("AnimalCommon");
			stockGenerator_Animals.tradeTagsSell.Add("AnimalUncommon");
			stockGenerator_Animals.LogAnimalChances();
		}
	}
}
