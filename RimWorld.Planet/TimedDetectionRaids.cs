using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class TimedDetectionRaids : WorldObjectComp
{
	public bool alertRaidsArrivingIn;

	public const float RaidThreatPointsMultiplier = 1.5f;

	private static readonly FloatRange DefaultDelayRangeHours = new FloatRange(18f, 24f);

	private int ticksLeftToSendRaid = -1;

	private int ticksLeftTillNotifyPlayer = -1;

	private int raidsSentCount;

	public FloatRange delayRangeHours = DefaultDelayRangeHours;

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

	private Faction RaidFaction
	{
		get
		{
			if (parent.Faction != null && !parent.Faction.IsPlayer && !parent.Faction.def.raidsForbidden)
			{
				return parent.Faction;
			}
			return Faction.OfMechanoids;
		}
	}

	public int TicksLeftToSendRaids => ticksLeftToSendRaid;

	public int RaidsSentCount => raidsSentCount;

	public bool DetectionCountdownStarted => ticksLeftToSendRaid >= 0;

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
		Scribe_Values.Look(ref alertRaidsArrivingIn, "alertRaidsArrivingIn", defaultValue: false);
		Scribe_Values.Look(ref raidsSentCount, "raidsSentCount", 0);
		Scribe_Values.Look(ref delayRangeHours, "delayRangeHours", DefaultDelayRangeHours);
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

	public override void CompTickInterval(int delta)
	{
		MapParent mapParent = (MapParent)parent;
		if (mapParent.HasMap)
		{
			if (ticksLeftTillNotifyPlayer > 0)
			{
				ticksLeftTillNotifyPlayer -= delta;
				if (ticksLeftTillNotifyPlayer <= 0)
				{
					NotifyPlayer();
				}
			}
			if (ticksLeftToSendRaid <= 0)
			{
				return;
			}
			ticksLeftToSendRaid -= delta;
			if (ticksLeftToSendRaid <= 0)
			{
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.target = mapParent.Map;
				incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 1.5f;
				incidentParms.faction = RaidFaction;
				ticksLeftToSendRaid = (int)(delayRangeHours.RandomInRange * 2500f);
				if (IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms))
				{
					Messages.Message("MessageCaravanDetectedRaidArrived".Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction, ticksLeftToSendRaid.ToStringTicksToDays()), MessageTypeDefOf.ThreatBig);
					raidsSentCount++;
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
		alertRaidsArrivingIn = true;
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
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Set raid timer to 1 hour";
			command_Action.action = delegate
			{
				ticksLeftToSendRaid = 2500;
			};
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Disable raid timer";
			command_Action2.action = delegate
			{
				ticksLeftToSendRaid = -1;
			};
			yield return command_Action2;
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "DEV: Set notify raid timer to 1 hour";
			command_Action3.action = delegate
			{
				ticksLeftTillNotifyPlayer = 2500;
			};
			yield return command_Action3;
		}
	}

	public void CopyFrom(TimedDetectionRaids other)
	{
		ticksLeftToSendRaid = other.ticksLeftToSendRaid;
		ticksLeftTillNotifyPlayer = other.ticksLeftTillNotifyPlayer;
		delayRangeHours = other.delayRangeHours;
	}
}
