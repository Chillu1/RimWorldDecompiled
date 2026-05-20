using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ScenPart_StartingThing_Defined : ScenPart_ThingCount
{
	public const string PlayerStartWithTag = "PlayerStartsWith";

	public static string PlayerStartWithIntro => "ScenPart_StartWith".Translate();

	public override string Summary(Scenario scen)
	{
		return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", PlayerStartWithIntro);
	}

	public override IEnumerable<string> GetSummaryListEntries(string tag)
	{
		if (tag == "PlayerStartsWith")
		{
			yield return GenLabel.ThingLabel(thingDef, stuff, count).CapitalizeFirst();
		}
	}

	public override IEnumerable<Thing> PlayerStartingThings()
	{
		if (thingDef.stackLimit == 1)
		{
			for (int i = 0; i < count; i++)
			{
				yield return GenerateThing(1);
			}
		}
		else
		{
			yield return GenerateThing(count);
		}
	}

	private Thing GenerateThing(int num)
	{
		Thing thing = ThingMaker.MakeThing(thingDef, stuff);
		if (thing.TryGetComp(out CompQuality comp))
		{
			comp.SetQuality(quality ?? QualityCategory.Normal, ArtGenerationContext.Outsider);
		}
		if (thingDef.Minifiable)
		{
			thing = thing.MakeMinified();
		}
		if (thingDef.IsIngestible && thingDef.ingestible.IsMeal)
		{
			FoodUtility.GenerateGoodIngredients(thing, Faction.OfPlayer.ideos.PrimaryIdeo);
		}
		thing.stackCount = num;
		return thing;
	}
}
