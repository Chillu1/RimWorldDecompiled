using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld.Planet;

public class SitePartWorker_ItemStash : SitePartWorker
{
	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		ThingDef thingDef = slate.Get<ThingDef>("itemStashSingleThing");
		IEnumerable<ThingDef> enumerable = slate.Get<IEnumerable<ThingDef>>("itemStashThings");
		List<Thing> list;
		if (thingDef != null)
		{
			list = new List<Thing>();
			list.Add(ThingMaker.MakeThing(thingDef));
		}
		else if (enumerable != null)
		{
			list = new List<Thing>();
			foreach (ThingDef item in enumerable)
			{
				list.Add(ThingMaker.MakeThing(item));
			}
		}
		else
		{
			float x = slate.Get("points", 0f);
			ThingSetMakerParams parms = new ThingSetMakerParams
			{
				totalMarketValueRange = new FloatRange(0.7f, 1.3f) * QuestTuning.PointsToRewardMarketValueCurve.Evaluate(x)
			};
			list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
		}
		part.things = new ThingOwner<Thing>(part, oneStackOnly: false);
		part.things.dontTickContents = true;
		part.things.TryAddRangeOrTransfer(list, canMergeWithExistingStacks: false);
		slate.Set("generatedItemStashThings", list);
		outExtraDescriptionRules.Add(new Rule_String("itemStashContents", GenLabel.ThingsLabel(list)));
		outExtraDescriptionRules.Add(new Rule_String("itemStashContentsValue", GenThing.GetMarketValue(list).ToStringMoney()));
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		string text = base.GetPostProcessedThreatLabel(site, sitePart);
		if (site.HasWorldObjectTimeout)
		{
			text += " (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
		}
		return text;
	}
}
