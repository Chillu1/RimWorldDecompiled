using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Pawns
{
	public struct GetPawnParms
	{
		public bool mustBeFactionLeader;

		public bool mustBeWorldPawn;

		public bool ifWorldPawnThenMustBeFree;

		public bool ifWorldPawnThenMustBeFreeOrLeader;

		public bool mustHaveNoFaction;

		public bool mustBeFreeColonist;

		public bool mustBePlayerPrisoner;

		public bool mustBeNotSuspended;

		public bool mustHaveRoyalTitleInCurrentFaction;

		public bool mustBeNonHostileToPlayer;

		public bool? allowPermanentEnemyFaction;

		public bool canGeneratePawn;

		public bool requireResearchedBedroomFurnitureIfRoyal;

		public PawnKindDef mustBeOfKind;

		public Faction mustBeOfFaction;

		public FloatRange seniorityRange;

		public TechLevel minTechLevel;

		public List<FactionDef> excludeFactionDefs;

		public bool allowTemporaryFactions;

		public bool allowHidden;

		public bool mustBeCapableOfViolence;

		public bool redressPawn;

		public PawnGenerationRequest GenerationRequest(PawnKindDef pawnKind, Faction faction, RoyalTitleDef fixedTitle)
		{
			bool flag = mustBeCapableOfViolence;
			return new PawnGenerationRequest(pawnKind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, flag, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, fixedTitle);
		}
	}

	public const int MaxUsablePawnsToGenerate = 10;

	public static Pawn GeneratePawn(this Quest quest, PawnKindDef kindDef, Faction faction, bool allowAddictions = true, IEnumerable<TraitDef> forcedTraits = null, float biocodeWeaponChance = 0f, bool mustBeCapableOfViolence = true, Pawn extraPawnForExtraRelationChance = null, float relationWithExtraPawnChanceFactor = 0f, float biocodeApparelChance = 0f, bool ensureNonNumericName = false, bool forceGenerateNewPawn = false, DevelopmentalStage developmentalStages = DevelopmentalStage.Adult, bool allowPregnant = false)
	{
		bool allowAddictions2 = allowAddictions;
		bool mustBeCapableOfViolence2 = mustBeCapableOfViolence;
		bool forceGenerateNewPawn2 = forceGenerateNewPawn;
		bool allowPregnant2 = allowPregnant;
		PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn2, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence2, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant2, allowFood: true, allowAddictions2, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, biocodeWeaponChance, 0f, extraPawnForExtraRelationChance, relationWithExtraPawnChanceFactor, null, null, forcedTraits, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, developmentalStages);
		request.BiocodeApparelChance = biocodeApparelChance;
		return quest.GeneratePawn(request, ensureNonNumericName);
	}

	public static Pawn GeneratePawn(this Quest quest, PawnGenerationRequest request, bool ensureNonNumericName = false)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		if (ensureNonNumericName && (pawn.Name == null || pawn.Name.Numerical))
		{
			pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, null, forceNoNick: false, pawn.genes?.Xenotype);
		}
		QuestGen.AddToGeneratedPawns(pawn);
		if (!pawn.IsWorldPawn())
		{
			Find.WorldPawns.PassToWorld(pawn);
		}
		return pawn;
	}

	public static bool GetPawnTest(GetPawnParms parms, out Pawn pawn)
	{
		pawn = null;
		if (parms.mustHaveNoFaction && parms.mustHaveRoyalTitleInCurrentFaction)
		{
			return false;
		}
		if (parms.canGeneratePawn && (parms.mustBeFactionLeader || parms.mustBePlayerPrisoner || parms.mustBeFreeColonist))
		{
			Log.Warning("QuestGen_GetPawn has incompatible flags set, when canGeneratePawn is true these flags cannot be set: mustBeFactionLeader, mustBePlayerPrisoner, mustBeFreeColonist");
			return false;
		}
		IEnumerable<Pawn> source = ExistingUsablePawns(parms);
		if (source.Count() > 0)
		{
			pawn = source.RandomElement();
			return true;
		}
		if (parms.canGeneratePawn)
		{
			if (!parms.mustHaveNoFaction && !TryFindFactionForPawnGeneration(parms, out var _))
			{
				return false;
			}
			FloatRange senRange = parms.seniorityRange;
			if (parms.mustHaveRoyalTitleInCurrentFaction && parms.requireResearchedBedroomFurnitureIfRoyal && !DefDatabase<RoyalTitleDef>.AllDefsListForReading.Any((RoyalTitleDef x) => (senRange.max <= 0f || senRange.IncludesEpsilon(x.seniority)) && PlayerHasResearchedBedroomRequirementsFor(x)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static Pawn GetPawn(this Quest quest, GetPawnParms parms)
	{
		_ = QuestGen.slate;
		IEnumerable<Pawn> source = ExistingUsablePawns(parms);
		int num = source.Count();
		Pawn pawn;
		if (Rand.Chance(parms.canGeneratePawn ? Mathf.Clamp01(1f - (float)num / 10f) : 0f) && (parms.mustHaveNoFaction || TryFindFactionForPawnGeneration(parms, out var _)))
		{
			pawn = GeneratePawn(parms);
		}
		else
		{
			pawn = source.RandomElement();
			if (parms.redressPawn)
			{
				PawnGenerator.RedressPawn(pawn, parms.GenerationRequest(pawn.kindDef, pawn.Faction, null));
			}
		}
		if (pawn.Faction != null && !pawn.Faction.Hidden)
		{
			QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
			questPart_InvolvedFactions.factions.Add(pawn.Faction);
			quest.AddPart(questPart_InvolvedFactions);
		}
		QuestGen.AddToGeneratedPawns(pawn);
		return pawn;
	}

	public static IEnumerable<Pawn> ExistingUsablePawns(GetPawnParms parms)
	{
		return PawnsFinder.AllMapsWorldAndTemporary_Alive.Where((Pawn x) => IsGoodPawn(x, parms));
	}

	private static bool TryFindFactionForPawnGeneration(GetPawnParms parms, out Faction faction)
	{
		return Find.FactionManager.GetFactions(allowTemporary: parms.allowTemporaryFactions, allowHidden: parms.allowHidden, allowDefeated: false, allowNonHumanlike: false).Where(delegate(Faction x)
		{
			if (parms.mustBeOfFaction != null && x != parms.mustBeOfFaction)
			{
				return false;
			}
			if (parms.excludeFactionDefs != null && parms.excludeFactionDefs.Contains(x.def))
			{
				return false;
			}
			if (parms.mustHaveRoyalTitleInCurrentFaction && !x.def.HasRoyalTitles)
			{
				return false;
			}
			if (parms.mustBeNonHostileToPlayer && x.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (parms.allowPermanentEnemyFaction != true && x.def.permanentEnemy)
			{
				return false;
			}
			return ((int)x.def.techLevel >= (int)parms.minTechLevel) ? true : false;
		}).TryRandomElement(out faction);
	}

	private static Pawn GeneratePawn(GetPawnParms parms, Faction faction = null)
	{
		PawnKindDef result = parms.mustBeOfKind;
		if (faction == null && !parms.mustHaveNoFaction)
		{
			if (!TryFindFactionForPawnGeneration(parms, out faction))
			{
				Log.Error("QuestNode_GetPawn tried generating pawn but couldn't find a proper faction for new pawn.");
			}
			else if (result == null)
			{
				result = faction.RandomPawnKind();
			}
		}
		RoyalTitleDef fixedTitle;
		if (parms.mustHaveRoyalTitleInCurrentFaction)
		{
			FloatRange senRange = parms.seniorityRange;
			IEnumerable<RoyalTitleDef> source = DefDatabase<RoyalTitleDef>.AllDefsListForReading.Where((RoyalTitleDef t) => faction.def.RoyalTitlesAllInSeniorityOrderForReading.Contains(t) && (senRange.max <= 0f || senRange.IncludesEpsilon(t.seniority)));
			if (parms.requireResearchedBedroomFurnitureIfRoyal && source.Any((RoyalTitleDef x) => PlayerHasResearchedBedroomRequirementsFor(x)))
			{
				source = source.Where((RoyalTitleDef x) => PlayerHasResearchedBedroomRequirementsFor(x));
			}
			fixedTitle = source.RandomElementByWeight((RoyalTitleDef t) => t.commonality);
			if (parms.mustBeOfKind == null && !DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef k) => k.titleRequired != null && k.titleRequired == fixedTitle).TryRandomElement(out result))
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
			result = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef kind) => kind.race.race.Humanlike).RandomElement();
		}
		Pawn pawn = PawnGenerator.GeneratePawn(parms.GenerationRequest(result, faction, fixedTitle));
		Find.WorldPawns.PassToWorld(pawn);
		if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any())
		{
			QuestPart_Hyperlinks questPart_Hyperlinks = new QuestPart_Hyperlinks();
			questPart_Hyperlinks.pawns.Add(pawn);
			QuestGen.quest.AddPart(questPart_Hyperlinks);
		}
		return pawn;
	}

	private static bool IsGoodPawn(Pawn pawn, GetPawnParms parms)
	{
		if (parms.mustBeFactionLeader)
		{
			Faction faction = pawn.Faction;
			if (faction == null || faction.leader != pawn || !faction.def.humanlikeFaction || faction.defeated || faction.Hidden || faction.IsPlayer || pawn.IsPrisoner)
			{
				return false;
			}
		}
		if (parms.mustBeOfFaction != null && pawn.Faction != parms.mustBeOfFaction)
		{
			return false;
		}
		if (pawn.Faction != null && parms.excludeFactionDefs != null && parms.excludeFactionDefs.Contains(pawn.Faction.def))
		{
			return false;
		}
		if (pawn.Faction != null && (int)pawn.Faction.def.techLevel < (int)parms.minTechLevel)
		{
			return false;
		}
		if (parms.mustBeOfKind != null && pawn.kindDef != parms.mustBeOfKind)
		{
			return false;
		}
		if (parms.mustHaveRoyalTitleInCurrentFaction && (pawn.Faction == null || pawn.royalty == null || !pawn.royalty.HasAnyTitleIn(pawn.Faction)))
		{
			return false;
		}
		if (parms.seniorityRange != default(FloatRange) && (pawn.royalty == null || pawn.royalty.MostSeniorTitle == null || !parms.seniorityRange.IncludesEpsilon(pawn.royalty.MostSeniorTitle.def.seniority)))
		{
			return false;
		}
		if (parms.mustBeWorldPawn && !pawn.IsWorldPawn())
		{
			return false;
		}
		if (parms.ifWorldPawnThenMustBeFree && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free)
		{
			return false;
		}
		if (parms.ifWorldPawnThenMustBeFreeOrLeader && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.FactionLeader)
		{
			return false;
		}
		if (pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) == WorldPawnSituation.ReservedByQuest)
		{
			return false;
		}
		if (parms.mustHaveNoFaction && pawn.Faction != null)
		{
			return false;
		}
		if (parms.mustBeFreeColonist && !pawn.IsFreeColonist)
		{
			return false;
		}
		if (parms.mustBePlayerPrisoner && !pawn.IsPrisonerOfColony)
		{
			return false;
		}
		if (parms.mustBeNotSuspended && pawn.Suspended)
		{
			return false;
		}
		if (parms.mustBeNonHostileToPlayer && (pawn.HostileTo(Faction.OfPlayer) || (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.Faction.HostileTo(Faction.OfPlayer))))
		{
			return false;
		}
		bool? allowPermanentEnemyFaction = parms.allowPermanentEnemyFaction;
		if (allowPermanentEnemyFaction.HasValue && allowPermanentEnemyFaction != true && pawn.Faction != null && pawn.Faction.def.permanentEnemy)
		{
			return false;
		}
		if (parms.requireResearchedBedroomFurnitureIfRoyal)
		{
			RoyalTitle royalTitle = pawn.royalty.HighestTitleWithBedroomRequirements();
			if (royalTitle != null && !PlayerHasResearchedBedroomRequirementsFor(royalTitle.def))
			{
				return false;
			}
		}
		if (parms.mustBeCapableOfViolence && pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return false;
		}
		return true;
	}

	private static bool PlayerHasResearchedBedroomRequirementsFor(RoyalTitleDef title)
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

	public static QuestPart_ReservePawns ReservePawns(this Quest quest, IEnumerable<Pawn> pawns)
	{
		QuestPart_ReservePawns questPart_ReservePawns = new QuestPart_ReservePawns();
		questPart_ReservePawns.pawns.AddRange(pawns);
		quest.AddPart(questPart_ReservePawns);
		return questPart_ReservePawns;
	}

	public static QuestPart_TransporterPawns_Feed FeedPawns(this Quest quest, IEnumerable<Pawn> pawns = null, Thing pawnsInTransporter = null, string inSignal = null)
	{
		QuestPart_TransporterPawns_Feed questPart_TransporterPawns_Feed = new QuestPart_TransporterPawns_Feed();
		questPart_TransporterPawns_Feed.pawnsInTransporter = pawnsInTransporter;
		questPart_TransporterPawns_Feed.pawns = pawns?.ToList();
		questPart_TransporterPawns_Feed.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_TransporterPawns_Feed);
		return questPart_TransporterPawns_Feed;
	}

	public static QuestPart_TransporterPawns_Tend TendPawns(this Quest quest, IEnumerable<Pawn> pawns = null, Thing pawnsInTransporter = null, string inSignal = null)
	{
		QuestPart_TransporterPawns_Tend questPart_TransporterPawns_Tend = new QuestPart_TransporterPawns_Tend();
		questPart_TransporterPawns_Tend.pawnsInTransporter = pawnsInTransporter;
		questPart_TransporterPawns_Tend.pawns = pawns?.ToList();
		questPart_TransporterPawns_Tend.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_TransporterPawns_Tend);
		return questPart_TransporterPawns_Tend;
	}

	public static QuestPart_TransporterPawns_TendWithMedicine TendPawnsWithMedicine(this Quest quest, ThingDef medicineDef, bool allowSelfTend = true, IEnumerable<Pawn> pawns = null, Thing pawnsInTransporter = null, string inSignal = null)
	{
		QuestPart_TransporterPawns_TendWithMedicine questPart_TransporterPawns_TendWithMedicine = new QuestPart_TransporterPawns_TendWithMedicine();
		questPart_TransporterPawns_TendWithMedicine.pawnsInTransporter = pawnsInTransporter;
		questPart_TransporterPawns_TendWithMedicine.pawns = pawns?.ToList();
		questPart_TransporterPawns_TendWithMedicine.medicineDef = medicineDef;
		questPart_TransporterPawns_TendWithMedicine.allowSelfTend = allowSelfTend;
		questPart_TransporterPawns_TendWithMedicine.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_TransporterPawns_TendWithMedicine);
		return questPart_TransporterPawns_TendWithMedicine;
	}

	public static QuestPart_EnsureNotDowned EnsureNotDowned(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null)
	{
		QuestPart_EnsureNotDowned questPart_EnsureNotDowned = new QuestPart_EnsureNotDowned();
		questPart_EnsureNotDowned.pawns = pawns?.ToList();
		questPart_EnsureNotDowned.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_EnsureNotDowned);
		return questPart_EnsureNotDowned;
	}

	public static QuestPart_Bestowing_TargetChangedTitle Bestowing_TargetChangedTitle(this Quest quest, Pawn target, Pawn bestower, RoyalTitleDef currentTitle, string inSignal)
	{
		QuestPart_Bestowing_TargetChangedTitle questPart_Bestowing_TargetChangedTitle = new QuestPart_Bestowing_TargetChangedTitle();
		questPart_Bestowing_TargetChangedTitle.pawn = target;
		questPart_Bestowing_TargetChangedTitle.bestower = bestower;
		questPart_Bestowing_TargetChangedTitle.currentTitle = currentTitle;
		questPart_Bestowing_TargetChangedTitle.inSignal = inSignal;
		questPart_Bestowing_TargetChangedTitle.signalListenMode = QuestPart.SignalListenMode.OngoingOrNotYetAccepted;
		quest.AddPart(questPart_Bestowing_TargetChangedTitle);
		return questPart_Bestowing_TargetChangedTitle;
	}

	public static QuestPart_SetAllApparelLocked SetAllApparelLocked(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null)
	{
		QuestPart_SetAllApparelLocked questPart_SetAllApparelLocked = new QuestPart_SetAllApparelLocked();
		questPart_SetAllApparelLocked.pawns.AddRange(pawns);
		questPart_SetAllApparelLocked.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_SetAllApparelLocked);
		return questPart_SetAllApparelLocked;
	}
}
