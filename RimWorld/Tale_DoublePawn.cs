using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Tale_DoublePawn : Tale
{
	public TaleData_Pawn firstPawnData;

	public TaleData_Pawn secondPawnData;

	public override Pawn DominantPawn => firstPawnData.pawn;

	public override string ShortSummary
	{
		get
		{
			string text = string.Concat(def.LabelCap + ": ", firstPawnData.name?.ToString());
			if (secondPawnData != null)
			{
				text = text + ", " + secondPawnData.name;
			}
			return text;
		}
	}

	public Tale_DoublePawn()
	{
	}

	public Tale_DoublePawn(Pawn firstPawn, Pawn secondPawn)
	{
		firstPawnData = TaleData_Pawn.GenerateFrom(firstPawn);
		if (secondPawn != null)
		{
			secondPawnData = TaleData_Pawn.GenerateFrom(secondPawn);
		}
		if (firstPawn.SpawnedOrAnyParentSpawned)
		{
			surroundings = TaleData_Surroundings.GenerateFrom(firstPawn.PositionHeld, firstPawn.MapHeld);
		}
	}

	public override bool Concerns(Thing th)
	{
		if (secondPawnData != null && secondPawnData.pawn == th)
		{
			return true;
		}
		if (!base.Concerns(th))
		{
			return firstPawnData.pawn == th;
		}
		return true;
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (firstPawnData.faction == faction)
		{
			firstPawnData.faction = null;
		}
		if (secondPawnData != null && secondPawnData.faction == faction)
		{
			secondPawnData.faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref firstPawnData, "firstPawnData");
		Scribe_Deep.Look(ref secondPawnData, "secondPawnData");
	}

	protected override IEnumerable<Rule> SpecialTextGenerationRules(Dictionary<string, string> outConstants = null)
	{
		if (def.firstPawnSymbol.NullOrEmpty() || def.secondPawnSymbol.NullOrEmpty())
		{
			Log.Error(def?.ToString() + " uses DoublePawn tale class but firstPawnSymbol and secondPawnSymbol are not both set");
		}
		foreach (Rule rule in firstPawnData.GetRules("ANYPAWN"))
		{
			yield return rule;
		}
		foreach (Rule rule2 in firstPawnData.GetRules(def.firstPawnSymbol, outConstants))
		{
			yield return rule2;
		}
		if (secondPawnData == null)
		{
			yield break;
		}
		foreach (Rule rule3 in firstPawnData.GetRules("ANYPAWN"))
		{
			yield return rule3;
		}
		foreach (Rule rule4 in secondPawnData.GetRules(def.secondPawnSymbol, outConstants))
		{
			yield return rule4;
		}
	}

	public override void GenerateTestData()
	{
		base.GenerateTestData();
		firstPawnData = TaleData_Pawn.GenerateRandom();
		secondPawnData = TaleData_Pawn.GenerateRandom();
	}
}
