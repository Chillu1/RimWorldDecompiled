using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomFactionForSite : QuestNode
{
	public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<Thing> mustBeHostileToFactionOf;

	protected override bool TestRunInt(Slate slate)
	{
		return TrySetVars(slate, test: true);
	}

	protected override void RunInt()
	{
		TrySetVars(QuestGen.slate, test: false);
	}

	private bool TrySetVars(Slate slate, bool test)
	{
		Pawn asker = slate.Get<Pawn>("asker");
		Thing mustBeHostileToFactionOfResolved = mustBeHostileToFactionOf.GetValue(slate);
		if (!SiteMakerHelper.TryFindRandomFactionFor(sitePartDefs.GetValue(slate), out var faction, disallowNonHostileFactions: true, delegate(Faction x)
		{
			if (asker != null && asker.Faction == x)
			{
				return false;
			}
			return (mustBeHostileToFactionOfResolved == null || mustBeHostileToFactionOfResolved.Faction == null || (x != mustBeHostileToFactionOfResolved.Faction && x.HostileTo(mustBeHostileToFactionOfResolved.Faction))) ? true : false;
		}))
		{
			return false;
		}
		if (!Find.Storyteller.difficulty.allowViolentQuests && faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		slate.Set(storeAs.GetValue(slate), faction);
		if (!test && faction != null && !faction.Hidden)
		{
			QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
			questPart_InvolvedFactions.factions.Add(faction);
			QuestGen.quest.AddPart(questPart_InvolvedFactions);
		}
		return true;
	}
}
