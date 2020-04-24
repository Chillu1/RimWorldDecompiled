using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Trader : TaleData
	{
		public string name;

		public int pawnID = -1;

		public Gender gender = Gender.Male;

		private bool IsPawn => pawnID >= 0;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref pawnID, "pawnID", -1);
			Scribe_Values.Look(ref gender, "gender", Gender.Male);
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			yield return new Rule_String(output: (!IsPawn) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(name) : name, keyword: prefix + "_nameFull");
			string nameShortIndefinite2 = (!IsPawn) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(name) : name;
			yield return new Rule_String(prefix + "_indefinite", nameShortIndefinite2);
			yield return new Rule_String(prefix + "_nameIndef", nameShortIndefinite2);
			nameShortIndefinite2 = ((!IsPawn) ? Find.ActiveLanguageWorker.WithDefiniteArticle(name) : name);
			yield return new Rule_String(prefix + "_definite", nameShortIndefinite2);
			yield return new Rule_String(prefix + "_nameDef", nameShortIndefinite2);
			yield return new Rule_String(prefix + "_pronoun", gender.GetPronoun());
			yield return new Rule_String(prefix + "_possessive", gender.GetPossessive());
		}

		public static TaleData_Trader GenerateFrom(ITrader trader)
		{
			TaleData_Trader taleData_Trader = new TaleData_Trader();
			taleData_Trader.name = trader.TraderName;
			Pawn pawn = trader as Pawn;
			if (pawn != null)
			{
				taleData_Trader.pawnID = pawn.thingIDNumber;
				taleData_Trader.gender = pawn.gender;
			}
			return taleData_Trader;
		}

		public static TaleData_Trader GenerateRandom()
		{
			PawnKindDef pawnKindDef = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef d) => d.trader).RandomElement();
			Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType));
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
			return GenerateFrom(pawn);
		}
	}
}
