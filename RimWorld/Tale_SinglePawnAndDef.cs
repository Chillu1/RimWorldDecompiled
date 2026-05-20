using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Tale_SinglePawnAndDef : Tale_SinglePawn
{
	public TaleData_Def defData;

	public Tale_SinglePawnAndDef()
	{
	}

	public Tale_SinglePawnAndDef(Pawn pawn, Def def)
		: base(pawn)
	{
		defData = TaleData_Def.GenerateFrom(def);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref defData, "defData");
	}

	protected override IEnumerable<Rule> SpecialTextGenerationRules(Dictionary<string, string> outConstants)
	{
		if (def.defSymbol.NullOrEmpty())
		{
			Log.Error(def?.ToString() + " uses tale type with def but defSymbol is not set.");
		}
		foreach (Rule item in base.SpecialTextGenerationRules(outConstants))
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
