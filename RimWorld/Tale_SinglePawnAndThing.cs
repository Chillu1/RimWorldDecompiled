using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_SinglePawnAndThing : Tale_SinglePawn
	{
		public TaleData_Thing thingData;

		public Tale_SinglePawnAndThing()
		{
		}

		public Tale_SinglePawnAndThing(Pawn pawn, Thing item)
			: base(pawn)
		{
			thingData = TaleData_Thing.GenerateFrom(item);
		}

		public override bool Concerns(Thing th)
		{
			if (!base.Concerns(th))
			{
				return th.thingIDNumber == thingData.thingID;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref thingData, "thingData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			foreach (Rule item in base.SpecialTextGenerationRules())
			{
				yield return item;
			}
			foreach (Rule rule in thingData.GetRules("THING"))
			{
				yield return rule;
			}
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			thingData = TaleData_Thing.GenerateRandom();
		}
	}
}
