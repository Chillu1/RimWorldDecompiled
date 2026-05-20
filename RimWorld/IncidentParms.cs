using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentParms : IExposable
{
	public IIncidentTarget target;

	public float points = -1f;

	public Faction faction;

	public bool forced;

	public string customLetterLabel;

	public string customLetterText;

	public LetterDef customLetterDef;

	public bool sendLetter = true;

	public List<ThingDef> letterHyperlinkThingDefs;

	public List<HediffDef> letterHyperlinkHediffDefs;

	public string inSignalEnd;

	public bool silent;

	public IntVec3 spawnCenter = IntVec3.Invalid;

	public Rot4 spawnRotation = Rot4.South;

	public bool generateFightersOnly;

	public bool dontUseSingleUseRocketLaunchers;

	public RaidStrategyDef raidStrategy;

	public PawnsArrivalModeDef raidArrivalMode;

	[LoadAlias("raidForceOneIncap")]
	public bool raidForceOneDowned;

	public bool raidNeverFleeIndividual;

	public bool raidArrivalModeForQuickMilitaryAid;

	public RaidAgeRestrictionDef raidAgeRestriction;

	public float biocodeWeaponsChance;

	public float biocodeApparelChance;

	public Dictionary<Pawn, int> pawnGroups;

	public int? pawnGroupMakerSeed;

	public Ideo pawnIdeo;

	public Lord lord;

	public PawnKindDef pawnKind;

	public int pawnCount;

	public PawnGroupKindDef pawnGroupKind;

	public TraderKindDef traderKind;

	public int podOpenDelay = 140;

	public Quest quest;

	public QuestScriptDef questScriptDef;

	public string questTag;

	public MechClusterSketch mechClusterSketch;

	public bool canTimeoutOrFlee = true;

	public bool canSteal = true;

	public bool canKidnap = true;

	public Pawn controllerPawn;

	public IntVec3? infestationLocOverride;

	public List<Thing> attackTargets;

	public List<Thing> gifts;

	public float totalBodySize;

	public PsychicRitualDef psychicRitualDef;

	public float pointMultiplier = 1f;

	public bool bypassStorytellerSettings;

	public bool? canRoofPunch;

	public int dropInRadius = -1;

	public List<Pawn> storeGeneratedNeutralPawns;

	private List<Pawn> tmpPawns;

	private List<int> tmpGroups;

	public int PawnGroupCount
	{
		get
		{
			Dictionary<Pawn, int> dictionary = pawnGroups;
			if (dictionary == null)
			{
				return 0;
			}
			return dictionary.Last().Value + 1;
		}
	}

	public IncidentParms ShallowCopy()
	{
		return (IncidentParms)MemberwiseClone();
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref target, "target");
		Scribe_Values.Look(ref points, "threatPoints", 0f);
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref forced, "forced", defaultValue: false);
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
		Scribe_Values.Look(ref sendLetter, "sendLetter", defaultValue: false);
		Scribe_Collections.Look(ref letterHyperlinkThingDefs, "letterHyperlinkThingDefs", LookMode.Def);
		Scribe_Collections.Look(ref letterHyperlinkHediffDefs, "letterHyperlinkHediffDefs", LookMode.Def);
		Scribe_Values.Look(ref inSignalEnd, "inSignalEnd");
		Scribe_Values.Look(ref silent, "silent", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.Saving && pawnGroups != null)
		{
			pawnGroups.RemoveAll((KeyValuePair<Pawn, int> x) => x.Key == null || x.Key.Destroyed);
		}
		Scribe_Values.Look(ref spawnCenter, "spawnCenter", IntVec3.Invalid);
		Scribe_Values.Look(ref spawnRotation, "spawnRotation");
		Scribe_Values.Look(ref generateFightersOnly, "generateFightersOnly", defaultValue: false);
		Scribe_Values.Look(ref dontUseSingleUseRocketLaunchers, "dontUseSingleUseRocketLaunchers", defaultValue: false);
		Scribe_Defs.Look(ref raidStrategy, "raidStrategy");
		Scribe_Defs.Look(ref raidArrivalMode, "raidArrivalMode");
		Scribe_Values.Look(ref raidForceOneDowned, "raidForceOneDowned", defaultValue: false);
		Scribe_Values.Look(ref raidNeverFleeIndividual, "raidNeverFleeIndividual", defaultValue: false);
		Scribe_Values.Look(ref raidArrivalModeForQuickMilitaryAid, "raidArrivalModeForQuickMilitaryAid", defaultValue: false);
		Scribe_Defs.Look(ref raidAgeRestriction, "raidAgeRestriction");
		Scribe_Values.Look(ref biocodeWeaponsChance, "biocodeWeaponsChance", 0f);
		Scribe_Values.Look(ref biocodeApparelChance, "biocodeApparelChance", 0f);
		Scribe_Collections.Look(ref pawnGroups, "pawnGroups", LookMode.Reference, LookMode.Value, ref tmpPawns, ref tmpGroups);
		Scribe_Values.Look(ref pawnGroupMakerSeed, "pawnGroupMakerSeed");
		Scribe_References.Look(ref pawnIdeo, "pawnIdeo");
		Scribe_References.Look(ref lord, "lord");
		Scribe_Defs.Look(ref pawnKind, "pawnKind");
		Scribe_Values.Look(ref pawnCount, "pawnCount", 0);
		Scribe_Defs.Look(ref pawnGroupKind, "pawnGroupKind");
		Scribe_Defs.Look(ref traderKind, "traderKind");
		Scribe_Values.Look(ref podOpenDelay, "podOpenDelay", 140);
		Scribe_References.Look(ref quest, "quest");
		Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
		Scribe_Values.Look(ref questTag, "questTag");
		Scribe_Deep.Look(ref mechClusterSketch, "mechClusterSketch");
		Scribe_Values.Look(ref canTimeoutOrFlee, "canTimeoutOrFlee", defaultValue: true);
		Scribe_Values.Look(ref canSteal, "canSteal", defaultValue: true);
		Scribe_Values.Look(ref canKidnap, "canKidnap", defaultValue: true);
		Scribe_References.Look(ref controllerPawn, "controllerPawn");
		Scribe_Values.Look(ref infestationLocOverride, "infestationLocOverride");
		Scribe_Collections.Look(ref attackTargets, "attackTargets", LookMode.Reference);
		Scribe_Collections.Look(ref gifts, "gifts", LookMode.Deep);
		Scribe_Values.Look(ref totalBodySize, "totalBodySize", 0f);
		Scribe_Defs.Look(ref psychicRitualDef, "psychicRitualDef");
		Scribe_Values.Look(ref pointMultiplier, "pointMultiplier", 1f);
		Scribe_Values.Look(ref bypassStorytellerSettings, "bypassStorytellerSettings", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			gifts?.RemoveAll((Thing x) => x == null);
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && attackTargets != null)
		{
			attackTargets.RemoveAll((Thing x) => x == null);
		}
	}

	public override string ToString()
	{
		string text = "";
		if (target != null)
		{
			text = text + "target=" + target;
		}
		if (points >= 0f)
		{
			text = text + ", points=" + points;
		}
		if (faction != null)
		{
			text = text + ", faction=" + faction.GetUniqueLoadID() + " (" + faction.def.defName + ")";
		}
		if (generateFightersOnly)
		{
			text = text + ", generateFightersOnly=" + generateFightersOnly;
		}
		if (raidStrategy != null)
		{
			text = text + ", raidStrategy=" + raidStrategy.defName;
		}
		if (questScriptDef != null)
		{
			text = text + ", questScriptDef=" + questScriptDef;
		}
		if (raidAgeRestriction != null)
		{
			text = text + ", raidAgeRestriction=" + raidAgeRestriction;
		}
		return text;
	}
}
