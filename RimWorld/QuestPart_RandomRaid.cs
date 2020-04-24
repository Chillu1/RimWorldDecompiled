using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_RandomRaid : QuestPart
	{
		public string inSignal;

		public MapParent mapParent;

		public FloatRange pointsRange;

		public Faction faction;

		public bool useCurrentThreatPoints;

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
			if (signal.tag == inSignal && mapParent != null && mapParent.HasMap)
			{
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.forced = true;
				incidentParms.quest = quest;
				incidentParms.target = mapParent.Map;
				incidentParms.points = (useCurrentThreatPoints ? StorytellerUtility.DefaultThreatPointsNow(mapParent.Map) : pointsRange.RandomInRange);
				incidentParms.faction = faction;
				IncidentDef incidentDef = (faction != null && !faction.HostileTo(Faction.OfPlayer)) ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;
				if (incidentDef.Worker.CanFireNow(incidentParms, forced: true))
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
}
