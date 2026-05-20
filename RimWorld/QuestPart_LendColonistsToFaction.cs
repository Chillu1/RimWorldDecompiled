using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_LendColonistsToFaction : QuestPartActivable
{
	public Thing shuttle;

	public Faction lendColonistsToFaction;

	public int returnLentColonistsInTicks = -1;

	public MapParent returnMap;

	public string outSignalColonistsDied;

	private int returnColonistsOnTick;

	private List<Thing> lentColonists = new List<Thing>();

	public List<Thing> LentColonistsListForReading => lentColonists;

	public int ReturnPawnsInDurationTicks => Mathf.Max(returnColonistsOnTick - GenTicks.TicksGame, 0);

	public override string DescriptionPart
	{
		get
		{
			if (base.State == QuestPartState.Disabled || lentColonists.Count == 0)
			{
				return null;
			}
			return "PawnsLent".Translate(lentColonists.Select((Thing t) => t.LabelShort).ToCommaList(useAnd: true), ReturnPawnsInDurationTicks.ToStringTicksToDays("0.0"));
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		CompTransporter compTransporter = shuttle.TryGetComp<CompTransporter>();
		if (lendColonistsToFaction == null || compTransporter == null)
		{
			return;
		}
		foreach (Thing item in (IEnumerable<Thing>)compTransporter.innerContainer)
		{
			if (item is Pawn { IsFreeColonist: not false } pawn)
			{
				lentColonists.Add(pawn);
			}
		}
		returnColonistsOnTick = GenTicks.TicksGame + returnLentColonistsInTicks;
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (ModsConfig.BiotechActive && lentColonists.Any((Thing baby) => ChildcareUtility.CanSuckle(baby as Pawn, out var _)))
		{
			foreach (Thing lentColonist in lentColonists)
			{
				if (lentColonist is Pawn pawn)
				{
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating);
					if (firstHediffOfDef != null)
					{
						firstHediffOfDef.Severity = 1f;
					}
				}
			}
		}
		if (Find.TickManager.TicksGame >= enableTick + returnLentColonistsInTicks)
		{
			Complete();
		}
	}

	protected override void Complete(SignalArgs signalArgs)
	{
		Map map = returnMap?.Map ?? quest.TryFindNewSuitableMapParentForRetarget()?.Map ?? Find.AnyPlayerHomeMap;
		if (map != null)
		{
			base.Complete(new SignalArgs(new LookTargets(lentColonists).Named("SUBJECT"), lentColonists.Select((Thing c) => c.LabelShort).ToCommaList(useAnd: true).Named("PAWNS")));
			if (lendColonistsToFaction != null && lendColonistsToFaction == Faction.OfEmpire)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Shuttle);
				thing.SetFaction(Faction.OfEmpire);
				TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, lentColonists, thing);
				transportShip.ArriveAt(DropCellFinder.GetBestShuttleLandingSpot(map, Faction.OfEmpire), map.Parent);
				transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
			}
			else
			{
				DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, lentColonists, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false, forbid: false);
			}
		}
	}

	private void ReturnDead(Corpse corpse)
	{
		Map map = quest.TryFindNewSuitableMapParentForRetarget()?.Map ?? Find.AnyPlayerHomeMap;
		if (map != null)
		{
			DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, Gen.YieldSingle(corpse), 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false, forbid: false);
		}
	}

	public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		if (lentColonists.Contains(pawn))
		{
			Building_Grave assignedGrave = null;
			if (pawn.ownership != null)
			{
				assignedGrave = pawn.ownership.AssignedGrave;
			}
			Corpse corpse = pawn.MakeCorpse(assignedGrave, null);
			lentColonists.Remove(pawn);
			ReturnDead(corpse);
			if (!outSignalColonistsDied.NullOrEmpty() && lentColonists.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalColonistsDied));
			}
		}
	}

	public override void Notify_PawnBorn(Thing baby, Thing birther, Pawn mother, Pawn father)
	{
		if (lentColonists.Contains(birther))
		{
			if (baby is Corpse corpse)
			{
				ReturnDead(corpse);
				return;
			}
			baby.SetFaction(birther.Faction);
			lentColonists.Add(baby);
		}
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		if (base.State == QuestPartState.Enabled)
		{
			Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
			if (Widgets.ButtonText(rect, "End " + ToString()))
			{
				Complete();
			}
			curY += rect.height + 4f;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_References.Look(ref lendColonistsToFaction, "lendColonistsToFaction");
		Scribe_Values.Look(ref returnLentColonistsInTicks, "returnLentColonistsInTicks", 0);
		Scribe_Values.Look(ref returnColonistsOnTick, "colonistsReturnOnTick", 0);
		Scribe_Collections.Look(ref lentColonists, "lentPawns", LookMode.Reference);
		Scribe_References.Look(ref returnMap, "returnMap");
		Scribe_Values.Look(ref outSignalColonistsDied, "outSignalColonistsDied");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			lentColonists.RemoveAll((Thing x) => x == null);
		}
	}
}
