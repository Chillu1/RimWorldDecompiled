using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld.Planet;

public class SitePartWorker_PrisonerWillingToJoin : SitePartWorker
{
	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		Pawn pawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(part.site.Tile, part.site.Faction);
		part.things = new ThingOwner<Pawn>(part, oneStackOnly: true);
		part.things.TryAdd(pawn);
		PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle(pawn), out var pawnRelationsInfo, informEvenIfSeenBefore: true, writeSeenPawnsNames: false);
		string output = (pawnRelationsInfo.NullOrEmpty() ? "" : ((string)("\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate(pawn.LabelShort, pawn) + "\n\n" + pawnRelationsInfo)));
		slate.Set("prisoner", pawn);
		outExtraDescriptionRules.Add(new Rule_String("prisonerFullRelationInfo", output));
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		string text = base.GetPostProcessedThreatLabel(site, sitePart);
		if (sitePart.things != null && sitePart.things.Any)
		{
			text = text + ": " + sitePart.things[0].LabelShortCap;
		}
		if (site.HasWorldObjectTimeout)
		{
			text += " (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
		}
		return text;
	}
}
