using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetPawn : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<bool> mustBeFactionLeader;

	public SlateRef<bool> mustBeWorldPawn;

	public SlateRef<bool> ifWorldPawnThenMustBeFree;

	public SlateRef<bool> ifWorldPawnThenMustBeFreeOrLeader;

	public SlateRef<bool> mustHaveNoFaction;

	public SlateRef<bool> mustBeFreeColonist;

	public SlateRef<bool> mustBePlayerPrisoner;

	public SlateRef<bool> mustBeNotSuspended;

	public SlateRef<bool> mustHaveRoyalTitleInCurrentFaction;

	public SlateRef<bool> mustBeNonHostileToPlayer;

	public SlateRef<bool?> allowPermanentEnemyFaction;

	public SlateRef<bool> canGeneratePawn;

	public SlateRef<bool> mustHaveSettlementOnLayer;

	public SlateRef<bool> requireResearchedBedroomFurnitureIfRoyal;

	public SlateRef<PawnKindDef> mustBeOfKind;

	public SlateRef<FloatRange> seniorityRange;

	public SlateRef<TechLevel> minTechLevel;

	public SlateRef<List<FactionDef>> excludeFactionDefs;

	public SlateRef<float?> hostileWeight;

	public SlateRef<float?> nonHostileWeight;

	public SlateRef<bool> factionMustBePermanent = true;

	public SlateRef<int> maxUsablePawnsToGenerate = 10;

	private IEnumerable<Pawn> ExistingUsablePawns(Slate slate)
	{
		return PawnsFinder.AllMapsWorldAndTemporary_Alive.Where((Pawn x) => IsGoodPawn(x, slate));
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (mustHaveNoFaction.GetValue(slate) && mustHaveRoyalTitleInCurrentFaction.GetValue(slate))
		{
			return false;
		}
		if (canGeneratePawn.GetValue(slate) && (mustBeFactionLeader.GetValue(slate) || mustBeWorldPawn.GetValue(slate) || mustBePlayerPrisoner.GetValue(slate) || mustBeFreeColonist.GetValue(slate)))
		{
			Log.Warning("QuestNode_GetPawn has incompatible flags set, when canGeneratePawn is true these flags cannot be set: mustBeFactionLeader, mustBeWorldPawn, mustBePlayerPrisoner, mustBeFreeColonist");
			return false;
		}
		if (slate.TryGet<Pawn>(storeAs.GetValue(slate), out var var) && IsGoodPawn(var, slate))
		{
			return true;
		}
		IEnumerable<Pawn> source = ExistingUsablePawns(slate);
		if (source.Count() > 0)
		{
			slate.Set(storeAs.GetValue(slate), source.RandomElement());
			return true;
		}
		if (canGeneratePawn.GetValue(slate))
		{
			if (!mustHaveNoFaction.GetValue(slate) && !TryFindFactionForPawnGeneration(slate, out var _))
			{
				return false;
			}
			FloatRange senRange = seniorityRange.GetValue(slate);
			if (mustHaveRoyalTitleInCurrentFaction.GetValue(slate) && requireResearchedBedroomFurnitureIfRoyal.GetValue(slate) && !DefDatabase<RoyalTitleDef>.AllDefsListForReading.Any((RoyalTitleDef x) => (senRange.max <= 0f || senRange.IncludesEpsilon(x.seniority)) && PlayerHasResearchedBedroomRequirementsFor(x)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool TryFindFactionForPawnGeneration(Slate slate, out Faction faction)
	{
		return Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: false).Where(delegate(Faction x)
		{
			if (excludeFactionDefs.GetValue(slate) != null && excludeFactionDefs.GetValue(slate).Contains(x.def))
			{
				return false;
			}
			if (mustHaveRoyalTitleInCurrentFaction.GetValue(slate) && !x.def.HasRoyalTitles)
			{
				return false;
			}
			if (mustBeNonHostileToPlayer.GetValue(slate) && x.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (slate.TryGet<Map>("map", out var var) && mustHaveSettlementOnLayer.GetValue(slate) && var.Tile.Valid && !Find.WorldObjects.AnyFactionSettlementOnLayer(x, var.Tile.Layer))
			{
				return false;
			}
			if (allowPermanentEnemyFaction.GetValue(slate) != true && x.def.permanentEnemy)
			{
				return false;
			}
			if ((int)x.def.techLevel < (int)minTechLevel.GetValue(slate))
			{
				return false;
			}
			return (!factionMustBePermanent.GetValue(slate) || !x.temporary) ? true : false;
		}).TryRandomElementByWeight((Faction x) => x.HostileTo(Faction.OfPlayer) ? (hostileWeight.GetValue(slate) ?? 1f) : (nonHostileWeight.GetValue(slate) ?? 1f), out faction);
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!QuestGen.slate.TryGet<Pawn>(storeAs.GetValue(slate), out var var) || !IsGoodPawn(var, slate))
		{
			IEnumerable<Pawn> source = ExistingUsablePawns(slate);
			int num = source.Count();
			var = ((!Rand.Chance(canGeneratePawn.GetValue(slate) ? Mathf.Clamp01(1f - (float)num / (float)maxUsablePawnsToGenerate.GetValue(slate)) : 0f) || (!mustHaveNoFaction.GetValue(slate) && !TryFindFactionForPawnGeneration(slate, out var _))) ? source.RandomElementByWeight((Pawn x) => (x.Faction != null && x.Faction.HostileTo(Faction.OfPlayer)) ? (hostileWeight.GetValue(slate) ?? 1f) : (nonHostileWeight.GetValue(slate) ?? 1f)) : GeneratePawn(slate));
			if (var.Faction != null && !var.Faction.Hidden)
			{
				QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
				questPart_InvolvedFactions.factions.Add(var.Faction);
				QuestGen.quest.AddPart(questPart_InvolvedFactions);
			}
			QuestGen.slate.Set(storeAs.GetValue(slate), var);
		}
	}

	private Pawn GeneratePawn(Slate slate, Faction faction = null)
	{
		PawnKindDef result = mustBeOfKind.GetValue(slate);
		if (faction == null && !mustHaveNoFaction.GetValue(slate))
		{
			if (!TryFindFactionForPawnGeneration(slate, out faction))
			{
				Log.Error("QuestNode_GetPawn tried generating pawn but couldn't find a proper faction for new pawn.");
			}
			else if (result == null)
			{
				result = faction.RandomPawnKind();
			}
		}
		RoyalTitleDef fixedTitle;
		if (mustHaveRoyalTitleInCurrentFaction.GetValue(slate))
		{
			if (!seniorityRange.TryGetValue(slate, out var senRange))
			{
				senRange = FloatRange.Zero;
			}
			IEnumerable<RoyalTitleDef> source = DefDatabase<RoyalTitleDef>.AllDefsListForReading.Where((RoyalTitleDef t) => faction.def.RoyalTitlesAllInSeniorityOrderForReading.Contains(t) && (senRange.max <= 0f || senRange.IncludesEpsilon(t.seniority)));
			if (requireResearchedBedroomFurnitureIfRoyal.GetValue(slate) && source.Any(PlayerHasResearchedBedroomRequirementsFor))
			{
				source = source.Where(PlayerHasResearchedBedroomRequirementsFor);
			}
			fixedTitle = source.RandomElementByWeight((RoyalTitleDef t) => t.commonality);
			if (mustBeOfKind.GetValue(slate) == null && !DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef k) => k.titleRequired != null && k.titleRequired == fixedTitle).TryRandomElement(out result))
			{
				DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef k) => k.titleSelectOne != null && k.titleSelectOne.Contains(fixedTitle)).TryRandomElement(out result);
			}
		}
		else
		{
			fixedTitle = null;
		}
		if (result == null)
		{
			result = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pawnKindDef) => pawnKindDef.race.race.Humanlike).RandomElement();
		}
		PawnKindDef kind = result;
		Faction faction2 = faction;
		RoyalTitleDef fixedTitle2 = fixedTitle;
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction2, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, fixedTitle2));
		Find.WorldPawns.PassToWorld(pawn);
		if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any())
		{
			QuestPart_Hyperlinks questPart_Hyperlinks = new QuestPart_Hyperlinks();
			questPart_Hyperlinks.pawns.Add(pawn);
			QuestGen.quest.AddPart(questPart_Hyperlinks);
		}
		return pawn;
	}

	private bool IsGoodPawn(Pawn pawn, Slate slate)
	{
		if (mustBeFactionLeader.GetValue(slate))
		{
			Faction faction = pawn.Faction;
			if (faction == null || faction.leader != pawn || !faction.def.humanlikeFaction || faction.defeated || faction.Hidden || faction.IsPlayer || pawn.IsPrisoner)
			{
				return false;
			}
		}
		if (pawn.Faction != null && excludeFactionDefs.GetValue(slate) != null && excludeFactionDefs.GetValue(slate).Contains(pawn.Faction.def))
		{
			return false;
		}
		if (pawn.Faction != null && (int)pawn.Faction.def.techLevel < (int)minTechLevel.GetValue(slate))
		{
			return false;
		}
		if (mustBeOfKind.GetValue(slate) != null && pawn.kindDef != mustBeOfKind.GetValue(slate))
		{
			return false;
		}
		if (mustHaveRoyalTitleInCurrentFaction.GetValue(slate) && (pawn.Faction == null || pawn.royalty == null || !pawn.royalty.HasAnyTitleIn(pawn.Faction)))
		{
			return false;
		}
		if (seniorityRange.GetValue(slate) != default(FloatRange) && (pawn.royalty?.MostSeniorTitle == null || !seniorityRange.GetValue(slate).IncludesEpsilon(pawn.royalty.MostSeniorTitle.def.seniority)))
		{
			return false;
		}
		if (factionMustBePermanent.GetValue(slate) && pawn.Faction != null && pawn.Faction.temporary)
		{
			return false;
		}
		if (mustBeWorldPawn.GetValue(slate) && !pawn.IsWorldPawn())
		{
			return false;
		}
		if (ifWorldPawnThenMustBeFree.GetValue(slate) && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free)
		{
			return false;
		}
		if (ifWorldPawnThenMustBeFreeOrLeader.GetValue(slate) && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.FactionLeader)
		{
			return false;
		}
		if (pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) == WorldPawnSituation.ReservedByQuest)
		{
			return false;
		}
		if (mustHaveNoFaction.GetValue(slate) && pawn.Faction != null)
		{
			return false;
		}
		if (mustBeFreeColonist.GetValue(slate) && !pawn.IsFreeColonist)
		{
			return false;
		}
		if (mustBePlayerPrisoner.GetValue(slate) && !pawn.IsPrisonerOfColony)
		{
			return false;
		}
		if (mustBeNotSuspended.GetValue(slate) && pawn.Suspended)
		{
			return false;
		}
		if (mustBeNonHostileToPlayer.GetValue(slate) && (pawn.HostileTo(Faction.OfPlayer) || (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.Faction.HostileTo(Faction.OfPlayer))))
		{
			return false;
		}
		bool? value = allowPermanentEnemyFaction.GetValue(slate);
		if (value.HasValue && value != true && pawn.Faction != null && pawn.Faction.def.permanentEnemy)
		{
			return false;
		}
		if (requireResearchedBedroomFurnitureIfRoyal.GetValue(slate))
		{
			RoyalTitle royalTitle = pawn.royalty.HighestTitleWithBedroomRequirements();
			if (royalTitle != null && !PlayerHasResearchedBedroomRequirementsFor(royalTitle.def))
			{
				return false;
			}
		}
		return true;
	}

	private bool PlayerHasResearchedBedroomRequirementsFor(RoyalTitleDef title)
	{
		if (title.bedroomRequirements == null)
		{
			return true;
		}
		for (int i = 0; i < title.bedroomRequirements.Count; i++)
		{
			if (!title.bedroomRequirements[i].PlayerCanBuildNow())
			{
				return false;
			}
		}
		return true;
	}
}
