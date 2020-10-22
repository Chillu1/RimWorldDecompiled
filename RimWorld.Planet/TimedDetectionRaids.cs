using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class TimedDetectionRaids : WorldObjectComp
	{
		public const float RaidThreatPointsMultiplier = 2.5f;

		private int ticksLeftToSendRaid = -1;

		private int ticksLeftTillNotifyPlayer = -1;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		public bool NextRaidCountdownActiveAndVisible
		{
			get
			{
				if (ticksLeftToSendRaid >= 0)
				{
					return ticksLeftTillNotifyPlayer == 0;
				}
				return false;
			}
		}

		public string DetectionCountdownTimeLeftString
		{
			get
			{
				if (!NextRaidCountdownActiveAndVisible)
				{
					return "";
				}
				return GetDetectionCountdownTimeLeftString(ticksLeftToSendRaid);
			}
		}

		private Faction RaidFaction => parent.Faction ?? Faction.OfMechanoids;

		public void StartDetectionCountdown(int ticks, int notifyTicks = -1)
		{
			ticksLeftToSendRaid = ticks;
			ticksLeftTillNotifyPlayer = ((notifyTicks == -1) ? Mathf.Min((int)(60000f * Rand.Range(0.8f, 1.2f)), ticks / 2) : notifyTicks);
		}

		public void ResetCountdown()
		{
			ticksLeftTillNotifyPlayer = (ticksLeftToSendRaid = -1);
		}

		public void SetNotifiedSilently()
		{
			ticksLeftTillNotifyPlayer = 0;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksLeftToSendRaid, "ticksLeftToForceExitAndRemoveMap", -1);
			Scribe_Values.Look(ref ticksLeftTillNotifyPlayer, "ticksLeftTillNotifyPlayer", -1);
		}

		public override string CompInspectStringExtra()
		{
			string text = null;
			if (NextRaidCountdownActiveAndVisible)
			{
				text += "CaravanDetectedRaidCountdown".Translate(DetectionCountdownTimeLeftString) + ".\n";
			}
			if (Prefs.DevMode)
			{
				if (ticksLeftToSendRaid != -1)
				{
					text = text + "[DEV]: Time left to send raid: " + ticksLeftToSendRaid.ToStringTicksToPeriod() + "\n";
				}
				if (ticksLeftTillNotifyPlayer != -1)
				{
					text = text + "[DEV]: Time left till notify player about incoming raid: " + ticksLeftTillNotifyPlayer.ToStringTicksToPeriod() + "\n";
				}
			}
			if (text != null)
			{
				text = text.TrimEndNewlines();
			}
			return text;
		}

		public override void CompTick()
		{
			MapParent mapParent = (MapParent)parent;
			if (mapParent.HasMap)
			{
				if (ticksLeftTillNotifyPlayer > 0 && --ticksLeftTillNotifyPlayer == 0)
				{
					NotifyPlayer();
				}
				if (ticksLeftToSendRaid > 0)
				{
					ticksLeftToSendRaid--;
					if (ticksLeftToSendRaid == 0)
					{
						IncidentParms incidentParms = new IncidentParms();
						incidentParms.target = mapParent.Map;
						incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
						incidentParms.faction = RaidFaction;
						IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
						ticksLeftToSendRaid = (int)(Rand.Range(18f, 24f) * 2500f);
						Messages.Message("MessageCaravanDetectedRaidArrived".Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction, ticksLeftToSendRaid.ToStringTicksToDays()), MessageTypeDefOf.ThreatBig);
					}
				}
			}
			else
			{
				ResetCountdown();
			}
		}

		private void NotifyPlayer()
		{
			Find.LetterStack.ReceiveLetter("LetterLabelSiteCountdownStarted".Translate(), "LetterTextSiteCountdownStarted".Translate(ticksLeftToSendRaid.ToStringTicksToDays(), RaidFaction.def.pawnsPlural, RaidFaction), LetterDefOf.ThreatBig, parent);
		}

		public static string GetDetectionCountdownTimeLeftString(int ticksLeft)
		{
			if (ticksLeft < 0)
			{
				return "";
			}
			return ticksLeft.ToStringTicksToPeriod();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Dev: Set raid timer to 1 hour";
				command_Action.action = delegate
				{
					ticksLeftToSendRaid = 2500;
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "Dev: Set notify raid timer to 1 hour";
				command_Action2.action = delegate
				{
					ticksLeftTillNotifyPlayer = 2500;
				};
				yield return command_Action2;
			}
		}

		public void CopyFrom(TimedDetectionRaids other)
		{
			ticksLeftToSendRaid = other.ticksLeftToSendRaid;
			ticksLeftTillNotifyPlayer = other.ticksLeftTillNotifyPlayer;
		}
	}
}
