using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

[Obsolete]
public class TimedForcedExit : WorldObjectComp
{
	private int ticksLeftToForceExitAndRemoveMap = -1;

	private static List<Pawn> tmpPawns = new List<Pawn>();

	public bool ForceExitAndRemoveMapCountdownActive => ticksLeftToForceExitAndRemoveMap >= 0;

	public string ForceExitAndRemoveMapCountdownTimeLeftString
	{
		get
		{
			if (!ForceExitAndRemoveMapCountdownActive)
			{
				return "";
			}
			return GetForceExitAndRemoveMapCountdownTimeLeftString(ticksLeftToForceExitAndRemoveMap);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ticksLeftToForceExitAndRemoveMap, "ticksLeftToForceExitAndRemoveMap", -1);
	}

	public void ResetForceExitAndRemoveMapCountdown()
	{
		ticksLeftToForceExitAndRemoveMap = -1;
	}

	public void StartForceExitAndRemoveMapCountdown(int duration)
	{
		ticksLeftToForceExitAndRemoveMap = duration;
	}

	public override string CompInspectStringExtra()
	{
		if (ForceExitAndRemoveMapCountdownActive)
		{
			return "ForceExitAndRemoveMapCountdown".Translate(ForceExitAndRemoveMapCountdownTimeLeftString) + ".";
		}
		return null;
	}

	public override void CompTickInterval(int delta)
	{
		MapParent mapParent = (MapParent)parent;
		if (!ForceExitAndRemoveMapCountdownActive)
		{
			return;
		}
		if (mapParent.HasMap)
		{
			ticksLeftToForceExitAndRemoveMap--;
			if (ticksLeftToForceExitAndRemoveMap <= 0)
			{
				ForceReform(mapParent);
			}
		}
		else
		{
			ticksLeftToForceExitAndRemoveMap = -1;
		}
	}

	public static string GetForceExitAndRemoveMapCountdownTimeLeftString(int ticksLeft)
	{
		if (ticksLeft < 0)
		{
			return "";
		}
		return ticksLeft.ToStringTicksToPeriod();
	}

	public static void ForceReform(MapParent mapParent)
	{
		if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, reform: true).Any((Pawn x) => x.IsColonist))
		{
			Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent);
			Current.Game.CurrentMap = mapParent.Map;
			Dialog_FormCaravan window = new Dialog_FormCaravan(mapParent.Map, reform: true, delegate
			{
				if (mapParent.HasMap)
				{
					mapParent.Destroy();
				}
			}, mapAboutToBeRemoved: true);
			Find.WindowStack.Add(window);
			return;
		}
		tmpPawns.Clear();
		tmpPawns.AddRange(mapParent.Map.mapPawns.AllPawns.Where((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer));
		if (tmpPawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
		{
			CaravanExitMapUtility.ExitMapAndCreateCaravan(tmpPawns, Faction.OfPlayer, mapParent.Tile, mapParent.Tile, PlanetTile.Invalid);
		}
		tmpPawns.Clear();
		mapParent.Destroy();
	}
}
