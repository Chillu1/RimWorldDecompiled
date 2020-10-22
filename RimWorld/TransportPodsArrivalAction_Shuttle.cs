using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class TransportPodsArrivalAction_Shuttle : TransportPodsArrivalAction
	{
		public MapParent mapParent;

		public WorldObject missionShuttleTarget;

		public WorldObject missionShuttleHome;

		public Quest sendAwayIfQuestFinished;

		public List<string> questTags;

		public TransportPodsArrivalAction_Shuttle()
		{
		}

		public TransportPodsArrivalAction_Shuttle(MapParent mapParent)
		{
			this.mapParent = mapParent;
		}

		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !mapParent.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			bool flag = !mapParent.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
			Settlement settlement;
			if ((settlement = orGenerateMap.Parent as Settlement) != null && settlement.Faction != Faction.OfPlayer)
			{
				TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
				TaggedString letterText = "LetterShuttleLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
				SettlementUtility.AffectRelationsOnAttacked_NewTmp(settlement, ref letterText);
				if (flag)
				{
					Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
				}
				Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget, settlement.Faction);
			}
			foreach (ActiveDropPodInfo pod in pods)
			{
				pod.missionShuttleHome = missionShuttleHome;
				pod.missionShuttleTarget = missionShuttleTarget;
				pod.sendAwayIfQuestFinished = sendAwayIfQuestFinished;
				pod.questTags = questTags;
			}
			PawnsArrivalModeDefOf.Shuttle.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
			Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref missionShuttleTarget, "missionShuttleTarget");
			Scribe_References.Look(ref missionShuttleHome, "missionShuttleHome");
			Scribe_References.Look(ref sendAwayIfQuestFinished, "sendAwayIfQuestFinished");
			Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
		}
	}
}
