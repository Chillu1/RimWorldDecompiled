using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetFaction : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<bool> allowEnemy;

	public SlateRef<bool> allowNeutral;

	public SlateRef<bool> allowAlly;

	public SlateRef<bool> allowAskerFaction;

	public SlateRef<bool?> allowPermanentEnemy;

	public SlateRef<bool> mustBePermanentEnemy;

	public SlateRef<bool> playerCantBeAttackingCurrently;

	public SlateRef<bool> peaceTalksCantExist;

	public SlateRef<bool> leaderMustBeSafe;

	public SlateRef<bool> mustHaveGoodwillRewardsEnabled;

	public SlateRef<Pawn> ofPawn;

	public SlateRef<Thing> mustBeHostileToFactionOf;

	public SlateRef<IEnumerable<Faction>> exclude;

	public SlateRef<IEnumerable<Faction>> allowedHiddenFactions;

	protected override bool TestRunInt(Slate slate)
	{
		if (slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) && IsGoodFaction(var, slate))
		{
			return true;
		}
		if (TryFindFaction(out var, slate))
		{
			slate.Set(storeAs.GetValue(slate), var);
			return true;
		}
		return false;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if ((!QuestGen.slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) || !IsGoodFaction(var, QuestGen.slate)) && TryFindFaction(out var, QuestGen.slate))
		{
			QuestGen.slate.Set(storeAs.GetValue(slate), var);
			if (!var.Hidden)
			{
				QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
				questPart_InvolvedFactions.factions.Add(var);
				QuestGen.quest.AddPart(questPart_InvolvedFactions);
			}
		}
	}

	private bool TryFindFaction(out Faction faction, Slate slate)
	{
		return (from x in Find.FactionManager.GetFactions(allowHidden: true)
			where IsGoodFaction(x, slate)
			select x).TryRandomElement(out faction);
	}

	private bool IsGoodFaction(Faction faction, Slate slate)
	{
		if (faction.Hidden && (allowedHiddenFactions.GetValue(slate) == null || !allowedHiddenFactions.GetValue(slate).Contains(faction)))
		{
			return false;
		}
		if (ofPawn.GetValue(slate) != null && faction != ofPawn.GetValue(slate).Faction)
		{
			return false;
		}
		if (exclude.GetValue(slate) != null && exclude.GetValue(slate).Contains(faction))
		{
			return false;
		}
		if (mustBePermanentEnemy.GetValue(slate) && !faction.def.permanentEnemy)
		{
			return false;
		}
		if (!allowEnemy.GetValue(slate) && faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (!allowNeutral.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Neutral)
		{
			return false;
		}
		if (!allowAlly.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Ally)
		{
			return false;
		}
		bool? value = allowPermanentEnemy.GetValue(slate);
		if (value.HasValue && value != true && faction.def.permanentEnemy)
		{
			return false;
		}
		if (playerCantBeAttackingCurrently.GetValue(slate) && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
		{
			return false;
		}
		if (mustHaveGoodwillRewardsEnabled.GetValue(slate) && !faction.allowGoodwillRewards)
		{
			return false;
		}
		if (peaceTalksCantExist.GetValue(slate))
		{
			if (PeaceTalksExist(faction))
			{
				return false;
			}
			string tag = QuestNode_QuestUnique.GetProcessedTag("PeaceTalks", faction);
			if (Find.QuestManager.questsInDisplayOrder.Any((Quest q) => q.tags.Contains(tag)))
			{
				return false;
			}
		}
		if (leaderMustBeSafe.GetValue(slate) && (faction.leader == null || faction.leader.Spawned || faction.leader.IsPrisoner))
		{
			return false;
		}
		Thing value2 = mustBeHostileToFactionOf.GetValue(slate);
		if (value2 != null && value2.Faction != null && (value2.Faction == faction || !faction.HostileTo(value2.Faction)))
		{
			return false;
		}
		return true;
	}

	private bool PeaceTalksExist(Faction faction)
	{
		List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
		for (int i = 0; i < peaceTalks.Count; i++)
		{
			if (peaceTalks[i].Faction == faction)
			{
				return true;
			}
		}
		return false;
	}
}
