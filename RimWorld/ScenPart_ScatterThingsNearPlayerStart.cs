using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenPart_ScatterThingsNearPlayerStart : ScenPart_ScatterThings
	{
		protected override bool NearPlayerStart => true;

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PlayerStartsWith")
			{
				yield return GenLabel.ThingLabel(thingDef, stuff, count).CapitalizeFirst();
			}
		}
	}
}
