using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Faction : IExposable, ILoadReferenceable, ICommunicable
{
	public FactionDef def;

	private string name;

	public int loadID = -1;

	public int randomKey;

	public float colorFromSpectrum = -999f;

	private List<FactionRelation> relations = new List<FactionRelation>();

	public Pawn leader;

	public KidnappedPawnsTracker kidnapped;

	private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

	public bool defeated;

	public int lastTraderRequestTick = -9999999;

	public int lastOrbitalTraderRequestTick = -9999999;

	public int lastMilitaryAidRequestTick = -9999999;

	public int lastExecutionTick = -9999999;

	private int naturalGoodwillTimer;

	public bool allowRoyalFavorRewards = true;

	public bool allowGoodwillRewards = true;

	public List<string> questTags;

	public Color? color;

	public Color? allegianceColor;

	public bool? hidden;

	public bool temporary;

	public bool factionHostileOnHarmByPlayer;

	public bool neverFlee;

	public FactionIdeosTracker ideos;

	public bool deactivated;

	private List<Map> avoidGridsBasicKeysWorkingList;

	private List<ByteGrid> avoidGridsBasicValuesWorkingList;

	private List<Map> avoidGridsSmartKeysWorkingList;

	private List<ByteGrid> avoidGridsSmartValuesWorkingList;

	private const int MinGoodwillForGainFriendlyExit = 4;

	private static List<PawnKindDef> allPawnKinds = new List<PawnKindDef>();

	private List<Pawn> tmpPrisonersRequiringBloodfeeders = new List<Pawn>();

	public const int RoyalThingUseViolationGoodwillImpact = 4;

	public string Name
	{
		get
		{
			if (HasName)
			{
				return name;
			}
			return def.LabelCap;
		}
		set
		{
			name = value;
		}
	}

	public bool HasName => name != null;

	public bool IsPlayer => def.isPlayer;

	public int PlayerGoodwill => GoodwillWith(OfPlayer);

	public FactionRelationKind PlayerRelationKind => RelationKindWith(OfPlayer);

	public int NaturalGoodwill => Find.GoodwillSituationManager.GetNaturalGoodwill(this);

	public Color Color
	{
		get
		{
			if (color.HasValue)
			{
				return color.Value;
			}
			if (def.colorSpectrum.NullOrEmpty())
			{
				return Color.white;
			}
			return ColorsFromSpectrum.Get(def.colorSpectrum, colorFromSpectrum);
		}
	}

	public Color AllegianceColor
	{
		get
		{
			Color valueOrDefault = allegianceColor.GetValueOrDefault();
			if (!allegianceColor.HasValue)
			{
				valueOrDefault = Color;
				allegianceColor = valueOrDefault;
			}
			return allegianceColor.Value;
		}
		set
		{
			allegianceColor = value;
		}
	}

	public string LeaderTitle
	{
		get
		{
			if (ideos == null || ideos.PrimaryIdeo == null || ideos.PrimaryIdeo.leaderTitleMale.NullOrEmpty())
			{
				if (leader != null && leader.gender == Gender.Female && !def.leaderTitleFemale.NullOrEmpty())
				{
					return def.leaderTitleFemale;
				}
				return def.leaderTitle;
			}
			if (leader != null && leader.gender == Gender.Female)
			{
				return ideos.PrimaryIdeo.leaderTitleFemale;
			}
			return ideos.PrimaryIdeo.leaderTitleMale;
		}
	}

	private bool ShouldHaveLeader
	{
		get
		{
			if (!IsPlayer && !Hidden && !temporary)
			{
				return def.humanlikeFaction;
			}
			return false;
		}
	}

	public TaggedString NameColored
	{
		get
		{
			if (HasName)
			{
				return name.CapitalizeFirst().ApplyTag(this);
			}
			return def.LabelCap;
		}
	}

	public bool CanEverGiveGoodwillRewards
	{
		get
		{
			if (!def.permanentEnemy)
			{
				return HasGoodwill;
			}
			return false;
		}
	}

	public string GetReportText => def.description + (def.HasRoyalTitles ? ("\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(this)) : "");

	public bool Hidden => hidden ?? def.hidden;

	public bool HasGoodwill
	{
		get
		{
			if (!Hidden)
			{
				return !temporary;
			}
			return false;
		}
	}

	public static Faction OfPlayer
	{
		get
		{
			Faction ofPlayerSilentFail = OfPlayerSilentFail;
			if (ofPlayerSilentFail == null)
			{
				Log.Error("Could not find player faction.");
			}
			return ofPlayerSilentFail;
		}
	}

	public static Faction OfMechanoids => Find.FactionManager.OfMechanoids;

	public static Faction OfInsects => Find.FactionManager.OfInsects;

	public static Faction OfAncients => Find.FactionManager.OfAncients;

	public static Faction OfAncientsHostile => Find.FactionManager.OfAncientsHostile;

	public static Faction OfEmpire => Find.FactionManager.OfEmpire;

	public static Faction OfPirates => Find.FactionManager.OfPirates;

	public static Faction OfHoraxCult => Find.FactionManager.OfHoraxCult;

	public static Faction OfEntities => Find.FactionManager.OfEntities;

	public static Faction OfTradersGuild => Find.FactionManager.OfTradersGuild;

	public static Faction OfSalvagers => Find.FactionManager.OfSalvagers;

	public static Faction OfPlayerSilentFail
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				GameInitData gameInitData = Find.GameInitData;
				if (gameInitData != null && gameInitData.playerFaction != null)
				{
					return gameInitData.playerFaction;
				}
			}
			return Find.FactionManager.OfPlayer;
		}
	}

	public Faction()
	{
		randomKey = Rand.Range(0, int.MaxValue);
		kidnapped = new KidnappedPawnsTracker(this);
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref leader, "leader");
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref randomKey, "randomKey", 0);
		Scribe_Values.Look(ref colorFromSpectrum, "colorFromSpectrum", 0f);
		Scribe_Collections.Look(ref relations, "relations", LookMode.Deep);
		Scribe_Deep.Look(ref kidnapped, "kidnapped", this);
		Scribe_Deep.Look(ref ideos, "ideos", this);
		Scribe_Collections.Look(ref predatorThreats, "predatorThreats", LookMode.Deep);
		Scribe_Values.Look(ref defeated, "defeated", defaultValue: false);
		Scribe_Values.Look(ref lastTraderRequestTick, "lastTraderRequestTick", -9999999);
		Scribe_Values.Look(ref lastOrbitalTraderRequestTick, "lastOrbitalTraderRequestTick", -9999999);
		Scribe_Values.Look(ref lastMilitaryAidRequestTick, "lastMilitaryAidRequestTick", -9999999);
		Scribe_Values.Look(ref lastExecutionTick, "lastExecutionTick", -9999999);
		Scribe_Values.Look(ref naturalGoodwillTimer, "naturalGoodwillTimer", 0);
		Scribe_Values.Look(ref allowRoyalFavorRewards, "allowRoyalFavorRewards", defaultValue: true);
		Scribe_Values.Look(ref allowGoodwillRewards, "allowGoodwillRewards", defaultValue: true);
		Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
		Scribe_Values.Look(ref hidden, "hidden");
		Scribe_Values.Look(ref temporary, "temporary", defaultValue: false);
		Scribe_Values.Look(ref factionHostileOnHarmByPlayer, "factionHostileOnHarmByPlayer", defaultValue: false);
		Scribe_Values.Look(ref color, "color");
		Scribe_Values.Look(ref neverFlee, "neverFlee", defaultValue: false);
		Scribe_Values.Look(ref allegianceColor, "mechColor");
		Scribe_Values.Look(ref deactivated, "deactivated", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			predatorThreats.RemoveAll((PredatorThreat x) => x.predator == null);
		}
		BackCompatibility.PostExposeData(this);
	}

	public void FactionTick()
	{
		CheckReachNaturalGoodwill();
		kidnapped.KidnappedPawnsTrackerTick();
		for (int num = predatorThreats.Count - 1; num >= 0; num--)
		{
			PredatorThreat predatorThreat = predatorThreats[num];
			if (predatorThreat.Expired)
			{
				predatorThreats.RemoveAt(num);
				if (predatorThreat.predator.Spawned)
				{
					predatorThreat.predator.Map.attackTargetsCache.UpdateTarget(predatorThreat.predator);
				}
			}
		}
		if (Find.TickManager.TicksGame % 1000 == 200 && IsPlayer)
		{
			if (NamePlayerFactionAndSettlementUtility.CanNameFactionNow())
			{
				Settlement settlement = Find.WorldObjects.Settlements.Find((Settlement x) => NamePlayerFactionAndSettlementUtility.CanNameSettlementSoon(x));
				if (settlement != null)
				{
					Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement));
				}
				else
				{
					Find.WindowStack.Add(new Dialog_NamePlayerFaction());
				}
			}
			else
			{
				Settlement settlement2 = Find.WorldObjects.Settlements.Find((Settlement x) => NamePlayerFactionAndSettlementUtility.CanNameSettlementNow(x));
				if (settlement2 != null)
				{
					if (NamePlayerFactionAndSettlementUtility.CanNameFactionSoon())
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement2));
					}
					else
					{
						Find.WindowStack.Add(new Dialog_NamePlayerSettlement(settlement2));
					}
				}
			}
		}
		if (ShouldHaveLeader && leader == null)
		{
			Log.ErrorOnce("Faction leader for " + Name + " is null.", loadID ^ 0x6BDDD);
		}
	}

	private void CheckReachNaturalGoodwill()
	{
		if (IsPlayer || !HasGoodwill || def.permanentEnemy)
		{
			return;
		}
		int num = BaseGoodwillWith(OfPlayer);
		IntRange intRange = new IntRange(NaturalGoodwill - 50, NaturalGoodwill + 50);
		if (intRange.Includes(num))
		{
			naturalGoodwillTimer = 0;
			return;
		}
		naturalGoodwillTimer++;
		if (num < intRange.min)
		{
			int num2 = 3000000;
			if (naturalGoodwillTimer >= num2)
			{
				TryAffectGoodwillWith(OfPlayer, Mathf.Min(10, intRange.min - num), canSendMessage: true, reason: HistoryEventDefOf.ReachNaturalGoodwill, canSendHostilityLetter: !temporary);
				naturalGoodwillTimer = 0;
			}
		}
		else if (num > intRange.max)
		{
			int num3 = 3000000;
			if (naturalGoodwillTimer >= num3)
			{
				TryAffectGoodwillWith(OfPlayer, -Mathf.Min(10, num - intRange.max), canSendMessage: true, reason: HistoryEventDefOf.ReachNaturalGoodwill, canSendHostilityLetter: !temporary);
				naturalGoodwillTimer = 0;
			}
		}
	}

	public void TryMakeInitialRelationsWith(Faction other)
	{
		if (RelationWith(other, allowNull: true) == null)
		{
			int a = GetInitialGoodwill(this, other);
			int b = GetInitialGoodwill(other, this);
			int num = Mathf.Min(a, b);
			FactionRelationKind kind = ((num > -10) ? ((num < 75) ? FactionRelationKind.Neutral : FactionRelationKind.Ally) : FactionRelationKind.Hostile);
			FactionRelation factionRelation = new FactionRelation();
			factionRelation.other = other;
			factionRelation.baseGoodwill = num;
			factionRelation.kind = kind;
			relations.Add(factionRelation);
			FactionRelation factionRelation2 = new FactionRelation();
			factionRelation2.other = this;
			factionRelation2.baseGoodwill = num;
			factionRelation2.kind = kind;
			other.relations.Add(factionRelation2);
		}
		static int GetInitialGoodwill(Faction faction, Faction faction2)
		{
			if (faction.def.permanentEnemy)
			{
				return -100;
			}
			if (faction.def.permanentEnemyToEveryoneExceptPlayer && !faction2.IsPlayer)
			{
				return -100;
			}
			if (faction.def.permanentEnemyToEveryoneExcept != null && !faction.def.permanentEnemyToEveryoneExcept.Contains(faction2.def))
			{
				return -100;
			}
			if (faction.def.naturalEnemy)
			{
				return -80;
			}
			return 0;
		}
	}

	public void SetRelation(FactionRelation relation)
	{
		if (relation.other == this)
		{
			Log.Error("Tried to set relation between faction " + this?.ToString() + " and itself.");
			return;
		}
		if (relation.other == null)
		{
			Log.Error("Relation is missing faction.");
			return;
		}
		relations.RemoveAll((FactionRelation r) => r.other == relation.other);
		relation.other.relations.RemoveAll((FactionRelation r) => r.other == this);
		relations.Add(relation);
		FactionRelation factionRelation = new FactionRelation();
		factionRelation.other = this;
		factionRelation.kind = relation.kind;
		relation.other.relations.Add(factionRelation);
	}

	public PawnKindDef RandomPawnKind()
	{
		allPawnKinds.Clear();
		if (def.pawnGroupMakers != null)
		{
			for (int i = 0; i < def.pawnGroupMakers.Count; i++)
			{
				List<PawnGenOption> options = def.pawnGroupMakers[i].options;
				for (int j = 0; j < options.Count; j++)
				{
					if (options[j].kind.RaceProps.Humanlike)
					{
						allPawnKinds.Add(options[j].kind);
					}
				}
			}
		}
		if (!allPawnKinds.Any())
		{
			return def.basicMemberKind;
		}
		PawnKindDef result = allPawnKinds.RandomElement();
		allPawnKinds.Clear();
		return result;
	}

	public bool IsPlayerGoodwillMinimum()
	{
		if (!def.permanentEnemy)
		{
			return PlayerGoodwill <= -100;
		}
		return true;
	}

	public FactionRelation RelationWith(Faction other, bool allowNull = false)
	{
		if (other == this)
		{
			Log.Error("Tried to get relation between faction " + this?.ToString() + " and itself.");
			return new FactionRelation();
		}
		for (int i = 0; i < relations.Count; i++)
		{
			if (relations[i].other == other)
			{
				return relations[i];
			}
		}
		if (!allowNull)
		{
			Log.Error("Faction " + name + " has null relation with " + other?.ToString() + ". Returning dummy relation.");
			return new FactionRelation();
		}
		return null;
	}

	public int BaseGoodwillWith(Faction other)
	{
		return RelationWith(other).baseGoodwill;
	}

	public int GoodwillWith(Faction other)
	{
		int num = BaseGoodwillWith(other);
		if (IsPlayer)
		{
			num = Mathf.Min(num, Find.GoodwillSituationManager.GetMaxGoodwill(other));
		}
		else if (other.IsPlayer)
		{
			num = Mathf.Min(num, Find.GoodwillSituationManager.GetMaxGoodwill(this));
		}
		return num;
	}

	public FactionRelationKind RelationKindWith(Faction other)
	{
		return RelationWith(other).kind;
	}

	public bool CanChangeGoodwillFor(Faction other, int goodwillChange)
	{
		if (!HasGoodwill || !other.HasGoodwill || def.permanentEnemy || other.def.permanentEnemy || defeated || other.defeated || other == this || (def.permanentEnemyToEveryoneExceptPlayer && !other.IsPlayer) || (other.def.permanentEnemyToEveryoneExceptPlayer && !IsPlayer) || (def.permanentEnemyToEveryoneExcept != null && !def.permanentEnemyToEveryoneExcept.Contains(other.def)) || (other.def.permanentEnemyToEveryoneExcept != null && !other.def.permanentEnemyToEveryoneExcept.Contains(def)))
		{
			return false;
		}
		if (goodwillChange > 0 && ((IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(other)) || (other.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(this))))
		{
			return false;
		}
		if (QuestUtility.IsGoodwillLockedByQuest(this, other))
		{
			return false;
		}
		return true;
	}

	public int GoodwillToMakeHostile(Faction other)
	{
		if (this.HostileTo(other))
		{
			return 0;
		}
		return -75 - GoodwillWith(other);
	}

	public int CalculateAdjustedGoodwillChange(Faction other, int goodwillChange)
	{
		int num = GoodwillWith(other);
		if (IsPlayer || other.IsPlayer)
		{
			int num2 = (IsPlayer ? other.NaturalGoodwill : NaturalGoodwill);
			if ((num2 < num && goodwillChange < 0) || (num2 > num && goodwillChange > 0))
			{
				int num3 = Mathf.Min(Mathf.Abs(num - num2), Mathf.Abs(goodwillChange));
				int num4 = Mathf.RoundToInt(0.25f * (float)num3);
				if (goodwillChange < 0)
				{
					num4 = -num4;
				}
				goodwillChange += num4;
			}
		}
		return goodwillChange;
	}

	public bool TryAffectGoodwillWith(Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, HistoryEventDef reason = null, GlobalTargetInfo? lookTarget = null)
	{
		if (!CanChangeGoodwillFor(other, goodwillChange))
		{
			return false;
		}
		if (goodwillChange == 0)
		{
			return true;
		}
		int num = GoodwillWith(other);
		goodwillChange = CalculateAdjustedGoodwillChange(other, goodwillChange);
		int num2 = BaseGoodwillWith(other);
		int num3 = Mathf.Clamp(num2 + goodwillChange, -100, 100);
		if (num2 == num3)
		{
			return true;
		}
		if (reason != null && (IsPlayer || other.IsPlayer))
		{
			Faction arg = (IsPlayer ? other : this);
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(reason, arg.Named(HistoryEventArgsNames.AffectedFaction), goodwillChange.Named(HistoryEventArgsNames.CustomGoodwill)));
		}
		FactionRelation factionRelation = RelationWith(other);
		factionRelation.baseGoodwill = num3;
		factionRelation.CheckKindThresholds(this, canSendHostilityLetter, reason?.LabelCap ?? ((TaggedString)null), lookTarget ?? GlobalTargetInfo.Invalid, out var sentLetter);
		FactionRelation factionRelation2 = other.RelationWith(this);
		FactionRelationKind kind = factionRelation2.kind;
		factionRelation2.baseGoodwill = factionRelation.baseGoodwill;
		factionRelation2.kind = factionRelation.kind;
		bool sentLetter2;
		if (kind != factionRelation2.kind)
		{
			other.Notify_RelationKindChanged(this, kind, canSendHostilityLetter, reason?.LabelCap ?? ((TaggedString)null), lookTarget ?? GlobalTargetInfo.Invalid, out sentLetter2);
		}
		else
		{
			sentLetter2 = false;
		}
		int num4 = GoodwillWith(other);
		if (canSendMessage && num != num4 && !sentLetter && !sentLetter2 && Current.ProgramState == ProgramState.Playing && (IsPlayer || other.IsPlayer))
		{
			Faction faction = (IsPlayer ? other : this);
			string text = ((reason == null) ? ((string)"MessageGoodwillChanged".Translate(faction.name, num.ToString("F0"), num4.ToString("F0"))) : ((string)"MessageGoodwillChangedWithReason".Translate(faction.name, num.ToString("F0"), num4.ToString("F0"), reason.label)));
			Messages.Message(text, lookTarget ?? GlobalTargetInfo.Invalid, ((float)goodwillChange > 0f) ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NegativeEvent);
		}
		return true;
	}

	public void Notify_GoodwillSituationsChanged(Faction other, bool canSendHostilityLetter, string reason, GlobalTargetInfo? lookTarget)
	{
		FactionRelation factionRelation = RelationWith(other);
		factionRelation.CheckKindThresholds(this, canSendHostilityLetter, reason, lookTarget ?? GlobalTargetInfo.Invalid, out var sentLetter);
		FactionRelation factionRelation2 = other.RelationWith(this);
		FactionRelationKind kind = factionRelation2.kind;
		factionRelation2.kind = factionRelation.kind;
		if (kind != factionRelation2.kind)
		{
			other.Notify_RelationKindChanged(this, kind, canSendHostilityLetter, reason, lookTarget ?? GlobalTargetInfo.Invalid, out sentLetter);
		}
	}

	public void SetRelationDirect(Faction other, FactionRelationKind kind, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
	{
		if (HasGoodwill && other.HasGoodwill)
		{
			Log.Error("Tried to use SetRelationDirect for factions which use goodwill. The relation would be overriden by goodwill anyway. faction=" + this?.ToString() + ", other=" + other);
			return;
		}
		FactionRelation factionRelation = RelationWith(other);
		if (factionRelation.kind != kind)
		{
			FactionRelationKind kind2 = factionRelation.kind;
			factionRelation.kind = kind;
			Notify_RelationKindChanged(other, kind2, canSendHostilityLetter, reason, lookTarget ?? GlobalTargetInfo.Invalid, out var sentLetter);
			other.RelationWith(this).kind = kind;
			other.Notify_RelationKindChanged(this, kind2, canSendHostilityLetter, reason, lookTarget ?? GlobalTargetInfo.Invalid, out sentLetter);
		}
	}

	public void RemoveAllRelations()
	{
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			if (item != this)
			{
				item.relations.RemoveAll((FactionRelation x) => x.other == this);
			}
		}
		relations.Clear();
	}

	public void TryAppendRelationKindChangedInfo(StringBuilder text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
	{
		TaggedString text2 = null;
		TryAppendRelationKindChangedInfo(ref text2, previousKind, newKind, reason);
		if (!text2.NullOrEmpty())
		{
			text.AppendLine();
			text.AppendLine();
			text.AppendTagged(text2);
		}
	}

	public void TryAppendRelationKindChangedInfo(ref TaggedString text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
	{
		if (previousKind == newKind)
		{
			return;
		}
		if (!text.NullOrEmpty())
		{
			text += "\n\n";
		}
		switch (newKind)
		{
		case FactionRelationKind.Hostile:
			text += "LetterRelationsChange_Hostile".Translate(NameColored);
			if (HasGoodwill)
			{
				string key = (def.hideGiftingInHostilityText ? "LetterRelationsChange_HostileGoodwillDescription_NoGifting" : "LetterRelationsChange_HostileGoodwillDescription");
				text += "\n\n" + key.Translate(PlayerGoodwill.ToStringWithSign(), (-75).ToStringWithSign(), 0.ToStringWithSign());
			}
			if (!reason.NullOrEmpty())
			{
				text += "\n\n" + "FinalStraw".Translate(reason.CapitalizeFirst());
			}
			break;
		case FactionRelationKind.Ally:
			text += "LetterRelationsChange_Ally".Translate(NameColored);
			if (HasGoodwill)
			{
				text += "\n\n" + "LetterRelationsChange_AllyGoodwillDescription".Translate(PlayerGoodwill.ToStringWithSign(), 75.ToStringWithSign(), 0.ToStringWithSign());
			}
			if (!reason.NullOrEmpty())
			{
				text += "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
			}
			break;
		case FactionRelationKind.Neutral:
			if (previousKind == FactionRelationKind.Hostile)
			{
				text += "LetterRelationsChange_NeutralFromHostile".Translate(NameColored);
				if (HasGoodwill)
				{
					text += "\n\n" + "LetterRelationsChange_NeutralFromHostileGoodwillDescription".Translate(NameColored, PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
				}
				if (!reason.NullOrEmpty())
				{
					text += "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
				}
			}
			else
			{
				text += "LetterRelationsChange_NeutralFromAlly".Translate(NameColored);
				if (HasGoodwill)
				{
					text += "\n\n" + "LetterRelationsChange_NeutralFromAllyGoodwillDescription".Translate(NameColored, PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
				}
				if (!reason.NullOrEmpty())
				{
					text += "\n\n" + "Reason".Translate() + ": " + reason.CapitalizeFirst();
				}
			}
			break;
		}
	}

	public void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
	{
		if (dinfo.Instigator == null || IsPlayer)
		{
			return;
		}
		Pawn pawn = dinfo.Instigator as Pawn;
		if (pawn != null && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PredatorHunt)
		{
			TookDamageFromPredator(pawn);
		}
		if (dinfo.Instigator.Faction != null && dinfo.Def.ExternalViolenceFor(member) && !this.HostileTo(dinfo.Instigator.Faction))
		{
			if (factionHostileOnHarmByPlayer && dinfo.Instigator.Faction == OfPlayer)
			{
				SetRelationDirect(OfPlayer, FactionRelationKind.Hostile);
			}
			if (!member.InAggroMentalState && (pawn == null || !pawn.InAggroMentalState) && (!member.InMentalState || !member.MentalStateDef.IsExtreme || member.MentalStateDef.category != MentalStateCategory.Malicious || PlayerRelationKind != FactionRelationKind.Ally) && (dinfo.Instigator.Faction != OfPlayer || (!PrisonBreakUtility.IsPrisonBreaking(member) && !member.IsQuestHelper())) && (pawn == null || !SlaveRebellionUtility.IsRebelling(pawn)) && dinfo.Instigator.Faction == OfPlayer && !IsMutuallyHostileCrossfire(dinfo) && !member.IsSlaveOfColony)
			{
				float num = Mathf.Min(100f, dinfo.Amount);
				int goodwillChange = (int)(-1.3f * num);
				OfPlayer.TryAffectGoodwillWith(this, goodwillChange, canSendMessage: true, !temporary, HistoryEventDefOf.AttackedMember, member);
			}
		}
	}

	public void Notify_BuildingRemoved(Building building, Pawn deconstructor)
	{
		if (!IsPlayer && factionHostileOnHarmByPlayer && deconstructor != null && deconstructor.Faction == OfPlayer)
		{
			SetRelationDirect(OfPlayer, FactionRelationKind.Hostile);
		}
	}

	public void Notify_BuildingTookDamage(Building building, DamageInfo dinfo)
	{
		if (dinfo.Instigator != null && !IsPlayer && dinfo.Instigator.Faction != null && dinfo.Def.ExternalViolenceFor(building) && !this.HostileTo(dinfo.Instigator.Faction))
		{
			if (factionHostileOnHarmByPlayer && dinfo.Instigator.Faction == OfPlayer)
			{
				SetRelationDirect(OfPlayer, FactionRelationKind.Hostile);
			}
			if (dinfo.Instigator.Faction == OfPlayer && !IsMutuallyHostileCrossfire(dinfo))
			{
				float num = Mathf.Min(100f, dinfo.Amount);
				int goodwillChange = (int)(-1f * num);
				OfPlayer.TryAffectGoodwillWith(this, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.AttackedBuilding, building);
			}
		}
	}

	public void Notify_MemberCaptured(Pawn member, Faction violator)
	{
		if (ideos != null)
		{
			ideos.Notify_MemberGainedOrLost();
		}
		if (violator != this && !temporary && !member.IsSlaveOfColony)
		{
			OfPlayer.TryAffectGoodwillWith(this, OfPlayer.GoodwillToMakeHostile(this), canSendMessage: true, !temporary, HistoryEventDefOf.MemberCaptured, member);
		}
	}

	public void Notify_MemberStripped(Pawn member, Faction violator)
	{
		if (violator != this && !Hidden && !member.Dead && violator == OfPlayer && RelationKindWith(violator) != FactionRelationKind.Hostile)
		{
			OfPlayer.TryAffectGoodwillWith(this, -40, canSendMessage: true, !temporary, HistoryEventDefOf.MemberStripped, member);
		}
	}

	public void Notify_MemberDied(Pawn member, DamageInfo? dinfo, bool wasWorldPawn, bool wasGuilty, Map map)
	{
		if (ideos != null)
		{
			ideos.Notify_MemberGainedOrLost();
		}
		if (IsPlayer)
		{
			if (ModsConfig.BiotechActive && map != null && member.IsBloodfeeder())
			{
				CheckForPrisonersAssignedToBloodfeederInteractionMode(map, member);
			}
			return;
		}
		if (!wasWorldPawn && !PawnGenerator.IsBeingGenerated(member) && Current.ProgramState == ProgramState.Playing && map != null && !member.IsSubhuman && !member.IsSlaveOfColony && map.IsPlayerHome && !this.HostileTo(OfPlayer))
		{
			if (dinfo.HasValue && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
			{
				bool canSendMessage = MessagesRepeatAvoider.MessageShowAllowed("FactionRelationAdjustmentCrushed-" + Name, 5f);
				int goodwillChange = (member.RaceProps.Humanlike ? (-25) : (-15));
				OfPlayer.TryAffectGoodwillWith(this, goodwillChange, canSendMessage, !temporary, HistoryEventDefOf.MemberCrushed, new TargetInfo(member.Position, map));
			}
			else if (dinfo.HasValue && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == null))
			{
				if (!(dinfo.Value.Instigator is Pawn pawn) || !pawn.RaceProps.Animal || pawn.mindState?.mentalStateHandler.CurStateDef != MentalStateDefOf.ManhunterPermanent)
				{
					int goodwillChange2 = (member.RaceProps.Humanlike ? (-5) : (-3));
					OfPlayer.TryAffectGoodwillWith(this, goodwillChange2, canSendMessage: true, !temporary, HistoryEventDefOf.MemberNeutrallyDied, member);
				}
			}
			else if ((member.kindDef.factionHostileOnDeath || (member.kindDef.factionHostileOnKill && dinfo.HasValue && dinfo.Value.Instigator != null && dinfo.Value.Instigator.Faction == OfPlayer)) && !wasGuilty)
			{
				OfPlayer.TryAffectGoodwillWith(this, OfPlayer.GoodwillToMakeHostile(this), canSendMessage: true, !temporary, HistoryEventDefOf.MemberKilled);
			}
		}
		if (member == leader)
		{
			Notify_LeaderDied();
		}
	}

	private void CheckForPrisonersAssignedToBloodfeederInteractionMode(Map map, Pawn member)
	{
		List<Pawn> freeColonistsAndPrisoners = map.mapPawns.FreeColonistsAndPrisoners;
		foreach (Pawn item in freeColonistsAndPrisoners)
		{
			if (item.IsPrisonerOfColony && item.guest.HasInteractionWith((PrisonerInteractionModeDef interaction) => interaction.hideIfNoBloodfeeders))
			{
				tmpPrisonersRequiringBloodfeeders.Add(item);
			}
		}
		if (!tmpPrisonersRequiringBloodfeeders.Any())
		{
			tmpPrisonersRequiringBloodfeeders.Clear();
			return;
		}
		foreach (Pawn item2 in freeColonistsAndPrisoners)
		{
			if (!item2.IsPrisonerOfColony && item2.IsBloodfeeder())
			{
				tmpPrisonersRequiringBloodfeeders.Clear();
				return;
			}
		}
		Messages.Message("MessageNoBloodfeedersPrisonerInteractionReset".Translate(), tmpPrisonersRequiringBloodfeeders, MessageTypeDefOf.NeutralEvent);
		foreach (Pawn tmpPrisonersRequiringBloodfeeder in tmpPrisonersRequiringBloodfeeders)
		{
			tmpPrisonersRequiringBloodfeeder.guest.ToggleNonExclusiveInteraction(PrisonerInteractionModeDefOf.Bloodfeed, enabled: false);
		}
		tmpPrisonersRequiringBloodfeeders.Clear();
	}

	public void Notify_PawnJoined(Pawn p)
	{
		if (ideos != null)
		{
			ideos.Notify_MemberGainedOrLost();
		}
		if (p.RaceProps.Humanlike && !def.humanlikeFaction && !p.IsSubhuman && !p.IsCreepJoiner && !p.IsDuplicate && !p.Dead)
		{
			Log.Error("Humanlike pawn " + p.LabelShort + " was added to non-humanlike faction " + def.label);
		}
	}

	public void Notify_LeaderDied()
	{
		Pawn pawn = leader;
		string text = "LetterLeadersDeathLabel".Translate(name, LeaderTitle).Resolve().CapitalizeFirst();
		string text2 = "LetterLeadersDeath".Translate(NameColored, LeaderTitle, pawn.Named("OLDLEADER")).Resolve().CapitalizeFirst();
		if (TryGenerateNewLeader())
		{
			string text3 = "LetterNewLeader".Translate(LeaderTitle, leader.Named("NEWLEADER")).Resolve().CapitalizeFirst();
			if (!temporary)
			{
				Find.LetterStack.ReceiveLetter(text, text2 + "\n\n" + text3, LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "NoLongerFactionLeader", pawn.Named("SUBJECT"), leader.Named("NEWFACTIONLEADER"));
		}
		else
		{
			if (!temporary)
			{
				Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "NoLongerFactionLeader", pawn.Named("SUBJECT"));
		}
	}

	public void Notify_LeaderLost()
	{
		Pawn pawn = leader;
		if (TryGenerateNewLeader())
		{
			if (!temporary)
			{
				Find.LetterStack.ReceiveLetter("LetterLeaderChangedLabel".Translate(name, LeaderTitle).Resolve().CapitalizeFirst(), "LetterLeaderChanged".Translate(NameColored, LeaderTitle, pawn.Named("OLDLEADER")).Resolve().CapitalizeFirst() + "\n\n" + "LetterNewLeader".Translate(LeaderTitle, leader.Named("NEWLEADER")).Resolve().CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "NoLongerFactionLeader", pawn.Named("SUBJECT"), leader.Named("NEWFACTIONLEADER"));
		}
		else
		{
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "NoLongerFactionLeader", pawn.Named("SUBJECT"));
		}
	}

	public void Notify_RelationKindChanged(Faction other, FactionRelationKind previousKind, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
	{
		if (Current.ProgramState != ProgramState.Playing || other != OfPlayer)
		{
			canSendLetter = false;
		}
		sentLetter = false;
		ColoredText.ClearCache();
		FactionRelationKind factionRelationKind = RelationKindWith(other);
		if (factionRelationKind == FactionRelationKind.Hostile)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList())
				{
					if ((item.Faction == this && item.HostFaction == other) || (item.Faction == other && item.HostFaction == this))
					{
						item.guest.SetGuestStatus(item.HostFaction, GuestStatus.Prisoner);
					}
				}
			}
			if (other == OfPlayer)
			{
				QuestUtility.SendQuestTargetSignals(questTags, "BecameHostileToPlayer", this.Named("SUBJECT"));
			}
		}
		if (other == OfPlayer && !this.HostileTo(OfPlayer))
		{
			List<Site> list = new List<Site>();
			List<Site> sites = Find.WorldObjects.Sites;
			for (int i = 0; i < sites.Count; i++)
			{
				if (sites[i].factionMustRemainHostile && sites[i].Faction == this && !sites[i].HasMap)
				{
					list.Add(sites[i]);
				}
			}
			if (list.Any())
			{
				string text;
				string text2;
				if (list.Count == 1)
				{
					text = "LetterLabelSiteNoLongerHostile".Translate();
					text2 = "LetterSiteNoLongerHostile".Translate(NameColored, list[0].Label);
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < list.Count; j++)
					{
						if (stringBuilder.Length != 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append("  - " + list[j].LabelCap);
						ImportantPawnComp component = list[j].GetComponent<ImportantPawnComp>();
						if (component != null && component.pawn.Any)
						{
							stringBuilder.Append(" (" + component.pawn[0].LabelCap + ")");
						}
					}
					text = "LetterLabelSiteNoLongerHostileMulti".Translate();
					text2 = string.Concat("LetterSiteNoLongerHostileMulti".Translate(NameColored) + ":\n\n", stringBuilder?.ToString());
				}
				Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.NeutralEvent, new LookTargets(list.Select((Site x) => new GlobalTargetInfo(x.Tile))));
				for (int num = 0; num < list.Count; num++)
				{
					list[num].Destroy();
				}
			}
		}
		if (other == OfPlayer && this.HostileTo(OfPlayer))
		{
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int num2 = 0; num2 < allWorldObjects.Count; num2++)
			{
				if (allWorldObjects[num2].Faction == this)
				{
					TradeRequestComp component2 = allWorldObjects[num2].GetComponent<TradeRequestComp>();
					if (component2 != null && component2.ActiveRequest)
					{
						component2.Disable();
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				map.passingShipManager.RemoveAllShipsOfFaction(this);
			}
		}
		if (canSendLetter)
		{
			TaggedString text3 = "";
			TryAppendRelationKindChangedInfo(ref text3, previousKind, factionRelationKind, reason);
			switch (factionRelationKind)
			{
			case FactionRelationKind.Hostile:
				Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Hostile".Translate(Name), text3, LetterDefOf.NegativeEvent, lookTarget, this);
				sentLetter = true;
				break;
			case FactionRelationKind.Ally:
				Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Ally".Translate(Name), text3, LetterDefOf.PositiveEvent, lookTarget, this);
				sentLetter = true;
				break;
			case FactionRelationKind.Neutral:
				if (previousKind == FactionRelationKind.Hostile)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromHostile".Translate(Name), text3, LetterDefOf.PositiveEvent, lookTarget, this);
					sentLetter = true;
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromAlly".Translate(Name), text3, LetterDefOf.NeutralEvent, lookTarget, this);
					sentLetter = true;
				}
				break;
			}
		}
		if (Current.ProgramState != ProgramState.Playing)
		{
			return;
		}
		List<Map> maps = Find.Maps;
		for (int num3 = 0; num3 < maps.Count; num3++)
		{
			maps[num3].attackTargetsCache.Notify_FactionHostilityChanged(this, other);
			LordManager lordManager = maps[num3].lordManager;
			for (int num4 = 0; num4 < lordManager.lords.Count; num4++)
			{
				Lord lord = lordManager.lords[num4];
				if (lord.faction == other)
				{
					lord.Notify_FactionRelationsChanged(this, previousKind);
				}
				else if (lord.faction == this)
				{
					lord.Notify_FactionRelationsChanged(other, previousKind);
				}
			}
		}
	}

	public void Notify_PlayerTraded(float marketValueSentByPlayer, Pawn playerNegotiator)
	{
		int goodwillChange = (int)(marketValueSentByPlayer / 600f);
		OfPlayer.TryAffectGoodwillWith(this, goodwillChange, canSendMessage: true, !temporary, HistoryEventDefOf.Traded, playerNegotiator);
	}

	public void Notify_MemberExitedMap(Pawn member, bool freed)
	{
		bool num = freed && (member.HostFaction == OfPlayer || member.Faction == OfPlayer);
		bool flag = !freed && !member.Faction.HostileTo(OfPlayer);
		if (num || flag)
		{
			bool isHealthy;
			bool isInMentalState;
			int goodwillGainForExit = GetGoodwillGainForExit(member, freed, out isHealthy, out isInMentalState);
			if (member.mindState.AvailableForGoodwillReward && (freed || goodwillGainForExit >= 4))
			{
				HistoryEventDef reason = (freed ? HistoryEventDefOf.MemberExitedMapHealthy : HistoryEventDefOf.FriendlyExitedMapTended);
				OfPlayer.TryAffectGoodwillWith(this, goodwillGainForExit, canSendMessage: true, !temporary, reason);
			}
		}
		member.mindState.timesGuestTendedToByPlayer = 0;
	}

	public int GetGoodwillGainForExit(Pawn member, bool freed, out bool isHealthy, out bool isInMentalState)
	{
		isHealthy = false;
		isInMentalState = member.InMentalState;
		float num = 0f;
		bool flag = !freed && !member.Faction.HostileTo(OfPlayer) && member.mindState.timesGuestTendedToByPlayer > 0;
		if (member.health.hediffSet.BleedRateTotal < 0.001f && (freed || flag))
		{
			isHealthy = true;
			if (!isInMentalState)
			{
				num += 12f;
				if (PawnUtility.IsFactionLeader(member))
				{
					num += 40f;
				}
			}
		}
		return (int)(num + (float)Mathf.Min(member.mindState.timesGuestTendedToByPlayer, 10) * 1f);
	}

	public void Notify_MemberLeftExtraFaction(Pawn member)
	{
		if (member.HomeFaction != this && leader == member)
		{
			Notify_LeaderLost();
		}
	}

	[Obsolete("Will be removed in the future")]
	public void GenerateNewLeader()
	{
		TryGenerateNewLeader();
	}

	public bool TryGenerateNewLeader()
	{
		Pawn pawn = leader;
		leader = null;
		if (def.generateNewLeaderFromMapMembersOnly)
		{
			for (int i = 0; i < Find.Maps.Count; i++)
			{
				Map map = Find.Maps[i];
				for (int j = 0; j < map.mapPawns.AllPawnsCount; j++)
				{
					if (map.mapPawns.AllPawns[j] != pawn && !map.mapPawns.AllPawns[j].Destroyed && map.mapPawns.AllPawns[j].HomeFaction == this)
					{
						leader = map.mapPawns.AllPawns[j];
					}
				}
			}
		}
		else if (def.pawnGroupMakers != null)
		{
			List<PawnKindDef> list = new List<PawnKindDef>();
			foreach (PawnGroupMaker item in def.pawnGroupMakers.Where((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat))
			{
				foreach (PawnGenOption option in item.options)
				{
					if (option.kind.factionLeader)
					{
						list.Add(option.kind);
					}
				}
			}
			if (def.fixedLeaderKinds != null)
			{
				list.AddRange(def.fixedLeaderKinds);
			}
			if (list.TryRandomElement(out var result))
			{
				PawnKindDef kind = result;
				bool leaderForceGenerateNewPawn = def.leaderForceGenerateNewPawn;
				PawnGenerationRequest request = new PawnGenerationRequest(kind, this, PawnGenerationContext.NonPlayer, null, leaderForceGenerateNewPawn);
				Gender supremeGender = ideos.PrimaryIdeo.SupremeGender;
				if (supremeGender != Gender.None)
				{
					request.FixedGender = supremeGender;
				}
				leader = PawnGenerator.GeneratePawn(request);
				if (leader.RaceProps.IsFlesh)
				{
					leader.relations.everSeenByPlayer = true;
				}
				if (!Find.WorldPawns.Contains(leader))
				{
					Find.WorldPawns.PassToWorld(leader, PawnDiscardDecideMode.KeepForever);
				}
			}
		}
		return leader != null;
	}

	public string GetCallLabel()
	{
		return name;
	}

	public string GetInfoText()
	{
		return (string)def.LabelCap + ("\n" + "goodwill".Translate().CapitalizeFirst() + ": " + PlayerGoodwill.ToStringWithSign());
	}

	Faction ICommunicable.GetFaction()
	{
		return this;
	}

	public void TryOpenComms(Pawn negotiator)
	{
		Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this), radioMode: true);
		dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
		Find.WindowStack.Add(dialog_Negotiation);
	}

	private bool LeaderIsAvailableToTalk()
	{
		if (leader == null)
		{
			return false;
		}
		if (leader.Spawned && (leader.Downed || leader.IsPrisoner || !leader.Awake() || leader.InMentalState))
		{
			return false;
		}
		return true;
	}

	public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
	{
		if (IsPlayer)
		{
			return null;
		}
		string text = "CallOnRadio".Translate(GetCallLabel());
		text = text + " (" + PlayerRelationKind.GetLabelCap() + ", " + PlayerGoodwill.ToStringWithSign() + ")";
		if (!LeaderIsAvailableToTalk())
		{
			string text2 = ((leader == null) ? ((string)"LeaderUnavailableNoLeader".Translate()) : ((string)"LeaderUnavailable".Translate(leader.LabelShort, leader)));
			return new FloatMenuOption(text + " (" + text2 + ")", null, def.FactionIcon, Color);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
		{
			console.GiveUseCommsJob(negotiator, this);
		}, def.FactionIcon, Color, MenuOptionPriority.InitiateSocial), negotiator, console);
	}

	private void TookDamageFromPredator(Pawn predator)
	{
		for (int i = 0; i < predatorThreats.Count; i++)
		{
			if (predatorThreats[i].predator == predator)
			{
				predatorThreats[i].lastAttackTicks = Find.TickManager.TicksGame;
				return;
			}
		}
		PredatorThreat predatorThreat = new PredatorThreat();
		predatorThreat.predator = predator;
		predatorThreat.lastAttackTicks = Find.TickManager.TicksGame;
		predatorThreats.Add(predatorThreat);
	}

	public bool HasPredatorRecentlyAttackedAnyone(Pawn predator)
	{
		for (int i = 0; i < predatorThreats.Count; i++)
		{
			if (predatorThreats[i].predator == predator)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsMutuallyHostileCrossfire(DamageInfo dinfo)
	{
		if (dinfo.Instigator != null && dinfo.IntendedTarget != null)
		{
			if (!dinfo.IntendedTarget.HostileTo(dinfo.Instigator) || !dinfo.IntendedTarget.HostileTo(this))
			{
				if (dinfo.IntendedTarget.Faction.HostileTo(dinfo.Instigator.Faction))
				{
					return dinfo.IntendedTarget.Faction.HostileTo(this);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void Notify_RoyalThingUseViolation(Def implantOrWeapon, Pawn pawn, string violationSourceName, float detectionChance, int violationSourceLevel = 0)
	{
		if (!this.HostileTo(OfPlayer))
		{
			RoyalTitleDef minTitleToUse = ThingRequiringRoyalPermissionUtility.GetMinTitleToUse(implantOrWeapon, this, violationSourceLevel);
			string arg = ((minTitleToUse == null) ? ((string)"None".Translate()) : minTitleToUse.GetLabelCapFor(pawn));
			OfPlayer.TryAffectGoodwillWith(this, -4, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedForbiddenThing, pawn);
			Find.LetterStack.ReceiveLetter("LetterLawViolationDetectedLabel".Translate(pawn.Named("PAWN")).CapitalizeFirst(), "LetterLawViolationDetectedForbiddenThingUse".Translate(arg.Named("TITLE"), pawn.Named("PAWN"), violationSourceName.Named("CULPRIT"), this.Named("FACTION"), 4.ToString().Named("GOODWILL"), detectionChance.ToStringPercent().Named("CHANCE")), LetterDefOf.NegativeEvent, pawn);
		}
	}

	public RoyalTitleDef GetMinTitleForImplant(HediffDef implantDef, int level = 0)
	{
		if (def.royalImplantRules == null || def.royalImplantRules.Count == 0)
		{
			return null;
		}
		foreach (RoyalImplantRule royalImplantRule in def.royalImplantRules)
		{
			if (royalImplantRule.implantHediff == implantDef && (royalImplantRule.maxLevel >= level || level == 0))
			{
				return royalImplantRule.minTitle;
			}
		}
		return null;
	}

	public RoyalImplantRule GetMaxAllowedImplantLevel(HediffDef implantDef, RoyalTitleDef title)
	{
		if (def.royalImplantRules == null || def.royalImplantRules.Count == 0)
		{
			return null;
		}
		if (title == null)
		{
			return null;
		}
		int myTitleIdx = def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(title);
		return def.royalImplantRules.Where(delegate(RoyalImplantRule r)
		{
			if (r.implantHediff == implantDef)
			{
				RoyalTitleDef minTitleForImplant = GetMinTitleForImplant(implantDef, r.maxLevel);
				int num = def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(minTitleForImplant);
				if (myTitleIdx != -1)
				{
					return myTitleIdx >= num;
				}
				return true;
			}
			return false;
		}).Last();
	}

	public static Pair<Faction, RoyalTitleDef> GetMinTitleForImplantAllFactions(HediffDef implantDef)
	{
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			RoyalTitleDef minTitleForImplant = item.GetMinTitleForImplant(implantDef);
			if (minTitleForImplant != null)
			{
				return new Pair<Faction, RoyalTitleDef>(item, minTitleForImplant);
			}
		}
		return default(Pair<Faction, RoyalTitleDef>);
	}

	public void ChangeGoodwill_Debug(Faction other, int goodwillChange)
	{
		int baseGoodwill = Mathf.Clamp(BaseGoodwillWith(other) + goodwillChange, -100, 100);
		FactionRelation factionRelation = RelationWith(other);
		factionRelation.baseGoodwill = baseGoodwill;
		factionRelation.CheckKindThresholds(this, canSendLetter: false, null, GlobalTargetInfo.Invalid, out var _);
		FactionRelation factionRelation2 = other.RelationWith(this);
		factionRelation2.baseGoodwill = factionRelation.baseGoodwill;
		factionRelation2.kind = factionRelation.kind;
	}

	public string GetUniqueLoadID()
	{
		return "Faction_" + loadID;
	}

	public override string ToString()
	{
		if (name != null)
		{
			return name;
		}
		if (def != null)
		{
			return def.defName;
		}
		return "[faction of no def]";
	}
}
