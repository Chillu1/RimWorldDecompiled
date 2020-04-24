using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawnAndDef : Tale_DoublePawn
	{
		public TaleData_Def defData;

		public Tale_DoublePawnAndDef()
		{
		}

		public Tale_DoublePawnAndDef(Pawn firstPawn, Pawn secondPawn, Def def)
			: base(firstPawn, secondPawn)
		{
			defData = TaleData_Def.GenerateFrom(def);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref defData, "defData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			if (def.defSymbol.NullOrEmpty())
			{
				Log.Error(def + " uses tale type with def but defSymbol is not set.");
			}
			foreach (Rule item in base.SpecialTextGenerationRules())
			{
				yield return item;
			}
			foreach (Rule rule in defData.GetRules(def.defSymbol))
			{
				yield return rule;
			}
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			defData = TaleData_Def.GenerateFrom((Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), def.defType, "GetRandom"));
		}
	}
}
