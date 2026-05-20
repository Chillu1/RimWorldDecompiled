using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_RandomRaid : QuestPart
{
	public string inSignal;

	public MapParent mapParent;

	public FloatRange pointsRange;

	public Faction faction;

	public bool useCurrentThreatPoints;

	public float currentThreatPointsFactor = 1f;

	public PawnsArrivalModeDef arrivalMode;

	public RaidStrategyDef raidStrategy;

	public string customLetterLabel;

	public string customLetterText;

	public List<Thing> attackTargets;

	public bool generateFightersOnly;

	public bool fallbackToPlayerHomeMap;

	public bool sendLetter = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
		}
	}

	public override IEnumerable<Faction> InvolvedFactions
	{
		get
		{
			foreach (Faction involvedFaction in base.InvolvedFactions)
			{
				yield return involvedFaction;
			}
			if (faction != null)
			{
				yield return faction;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (fallbackToPlayerHomeMap && (this.mapParent == null || !this.mapParent.HasMap || !quest.IsParentSuitableForQuest(this.mapParent)))
		{
			MapParent mapParent = quest.TryFindNewSuitableMapParentForRetarget() ?? Find.AnyPlayerHomeMap.Parent;
			if (mapParent != null)
			{
				this.mapParent = mapParent;
			}
		}
		if (signal.tag == inSignal && this.mapParent != null && this.mapParent.HasMap)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.forced = true;
			incidentParms.quest = quest;
			incidentParms.target = this.mapParent.Map;
			incidentParms.points = (useCurrentThreatPoints ? (StorytellerUtility.DefaultThreatPointsNow(this.mapParent.Map) * currentThreatPointsFactor) : pointsRange.RandomInRange);
			incidentParms.faction = faction;
			incidentParms.customLetterLabel = signal.args.GetFormattedText(customLetterLabel);
			incidentParms.customLetterText = signal.args.GetFormattedText(customLetterText).Resolve();
			incidentParms.attackTargets = attackTargets;
			incidentParms.generateFightersOnly = generateFightersOnly;
			incidentParms.sendLetter = sendLetter;
			if (arrivalMode != null)
			{
				incidentParms.raidArrivalMode = arrivalMode;
			}
			IncidentDef incidentDef = ((faction != null && !faction.HostileTo(Faction.OfPlayer)) ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy);
			if (raidStrategy != null)
			{
				incidentParms.raidStrategy = raidStrategy;
			}
			if (faction != null)
			{
				incidentParms.points = Mathf.Max(incidentParms.points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			}
			if (incidentDef.Worker.CanFireNow(incidentParms))
			{
				incidentDef.Worker.TryExecute(incidentParms);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref pointsRange, "pointsRange");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref useCurrentThreatPoints, "useCurrentThreatPoints", defaultValue: false);
		Scribe_Values.Look(ref currentThreatPointsFactor, "currentThreatPointsFactor", 1f);
		Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Defs.Look(ref raidStrategy, "raidStrategy");
		Scribe_Collections.Look(ref attackTargets, "attackTargets", LookMode.Reference);
		Scribe_Values.Look(ref generateFightersOnly, "generateFightersOnly", defaultValue: false);
		Scribe_Values.Look(ref sendLetter, "sendLetter", defaultValue: true);
		Scribe_Values.Look(ref fallbackToPlayerHomeMap, "fallbackToPlayerHomeMap", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && attackTargets != null)
		{
			attackTargets.RemoveAll((Thing x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			mapParent = Find.RandomPlayerHomeMap.Parent;
			pointsRange = new FloatRange(500f, 1500f);
		}
	}
}
