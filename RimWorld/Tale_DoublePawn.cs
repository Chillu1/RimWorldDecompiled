using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawn : Tale
	{
		public TaleData_Pawn firstPawnData;

		public TaleData_Pawn secondPawnData;

		public override Pawn DominantPawn => firstPawnData.pawn;

		public override string ShortSummary
		{
			get
			{
				string text = (string)(def.LabelCap + ": ") + firstPawnData.name;
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref firstPawnData, "firstPawnData");
			Scribe_Deep.Look(ref secondPawnData, "secondPawnData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			if (def.firstPawnSymbol.NullOrEmpty() || def.secondPawnSymbol.NullOrEmpty())
			{
				Log.Error(def + " uses DoublePawn tale class but firstPawnSymbol and secondPawnSymbol are not both set");
			}
			foreach (Rule rule in firstPawnData.GetRules("ANYPAWN"))
			{
				yield return rule;
			}
			foreach (Rule rule2 in firstPawnData.GetRules(def.firstPawnSymbol))
			{
				yield return rule2;
			}
			if (secondPawnData != null)
			{
				foreach (Rule rule3 in firstPawnData.GetRules("ANYPAWN"))
				{
					yield return rule3;
				}
				foreach (Rule rule4 in secondPawnData.GetRules(def.secondPawnSymbol))
				{
					yield return rule4;
				}
			}
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			firstPawnData = TaleData_Pawn.GenerateRandom();
			secondPawnData = TaleData_Pawn.GenerateRandom();
		}
	}
}
