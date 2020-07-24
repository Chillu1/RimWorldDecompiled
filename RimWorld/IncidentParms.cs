using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentParms : IExposable
	{
		public IIncidentTarget target;

		public float points = -1f;

		public Faction faction;

		public bool forced;

		public string customLetterLabel;

		public string customLetterText;

		public LetterDef customLetterDef;

		public List<ThingDef> letterHyperlinkThingDefs;

		public List<HediffDef> letterHyperlinkHediffDefs;

		public string inSignalEnd;

		public IntVec3 spawnCenter = IntVec3.Invalid;

		public Rot4 spawnRotation = Rot4.South;

		public bool generateFightersOnly;

		public bool dontUseSingleUseRocketLaunchers;

		public RaidStrategyDef raidStrategy;

		public PawnsArrivalModeDef raidArrivalMode;

		public bool raidForceOneIncap;

		public bool raidNeverFleeIndividual;

		public bool raidArrivalModeForQuickMilitaryAid;

		public float biocodeApparelChance;

		public float biocodeWeaponsChance;

		public Dictionary<Pawn, int> pawnGroups;

		public int? pawnGroupMakerSeed;

		public PawnKindDef pawnKind;

		public int pawnCount;

		public TraderKindDef traderKind;

		public int podOpenDelay = 140;

		public Quest quest;

		public QuestScriptDef questScriptDef;

		public string questTag;

		public MechClusterSketch mechClusterSketch;

		private List<Pawn> tmpPawns;

		private List<int> tmpGroups;

		public void ExposeData()
		{
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref points, "threatPoints", 0f);
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref forced, "forced", defaultValue: false);
			Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
			Scribe_Values.Look(ref customLetterText, "customLetterText");
			Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
			Scribe_Collections.Look(ref letterHyperlinkThingDefs, "letterHyperlinkThingDefs", LookMode.Def);
			Scribe_Collections.Look(ref letterHyperlinkHediffDefs, "letterHyperlinkHediffDefs", LookMode.Def);
			Scribe_Values.Look(ref inSignalEnd, "inSignalEnd");
			Scribe_Values.Look(ref spawnCenter, "spawnCenter");
			Scribe_Values.Look(ref spawnRotation, "spawnRotation");
			Scribe_Values.Look(ref generateFightersOnly, "generateFightersOnly", defaultValue: false);
			Scribe_Values.Look(ref dontUseSingleUseRocketLaunchers, "dontUseSingleUseRocketLaunchers", defaultValue: false);
			Scribe_Defs.Look(ref raidStrategy, "raidStrategy");
			Scribe_Defs.Look(ref raidArrivalMode, "raidArrivalMode");
			Scribe_Values.Look(ref raidForceOneIncap, "raidForceIncap", defaultValue: false);
			Scribe_Values.Look(ref raidNeverFleeIndividual, "raidNeverFleeIndividual", defaultValue: false);
			Scribe_Values.Look(ref raidArrivalModeForQuickMilitaryAid, "raidArrivalModeForQuickMilitaryAid", defaultValue: false);
			Scribe_Collections.Look(ref pawnGroups, "pawnGroups", LookMode.Reference, LookMode.Value, ref tmpPawns, ref tmpGroups);
			Scribe_Values.Look(ref pawnGroupMakerSeed, "pawnGroupMakerSeed");
			Scribe_Defs.Look(ref pawnKind, "pawnKind");
			Scribe_Values.Look(ref biocodeWeaponsChance, "biocodeWeaponsChance", 0f);
			Scribe_Values.Look(ref biocodeApparelChance, "biocodeApparelChance", 0f);
			Scribe_Defs.Look(ref traderKind, "traderKind");
			Scribe_Values.Look(ref podOpenDelay, "podOpenDelay", 140);
			Scribe_References.Look(ref quest, "quest");
			Scribe_Values.Look(ref questTag, "questTag");
			Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
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
			return text;
		}
	}
}
