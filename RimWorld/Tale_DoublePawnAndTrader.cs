using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawnAndTrader : Tale_DoublePawn
	{
		public TaleData_Trader traderData;

		public Tale_DoublePawnAndTrader()
		{
		}

		public Tale_DoublePawnAndTrader(Pawn firstPawn, Pawn secondPawn, ITrader trader)
			: base(firstPawn, secondPawn)
		{
			traderData = TaleData_Trader.GenerateFrom(trader);
		}

		public override bool Concerns(Thing th)
		{
			if (!base.Concerns(th))
			{
				return traderData.pawnID == th.thingIDNumber;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref traderData, "traderData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			foreach (Rule item in base.SpecialTextGenerationRules())
			{
				yield return item;
			}
			foreach (Rule rule in traderData.GetRules("TRADER"))
			{
				yield return rule;
			}
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			traderData = TaleData_Trader.GenerateRandom();
		}
	}
}
