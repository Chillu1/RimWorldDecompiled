using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class QuestPart_LendColonistsToFaction : QuestPartActivable
	{
		public Thing shuttle;

		public Faction lendColonistsToFaction;

		public int returnLentColonistsInTicks = -1;

		public MapParent returnMap;

		private int returnColonistsOnTick;

		private List<Thing> lentColonists = new List<Thing>();

		public List<Thing> LentColonistsListForReading => lentColonists;

		public override string DescriptionPart
		{
			get
			{
				if (base.State == QuestPartState.Disabled || lentColonists.Count == 0)
				{
					return null;
				}
				return "PawnsLent".Translate(lentColonists.Select((Thing t) => t.LabelShort).ToCommaList(useAnd: true), Mathf.Max(returnColonistsOnTick - GenTicks.TicksGame, 0).ToStringTicksToDays("0.0"));
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
				Pawn pawn;
				if ((pawn = (item as Pawn)) != null && pawn.IsFreeColonist)
				{
					lentColonists.Add(pawn);
				}
			}
			returnColonistsOnTick = GenTicks.TicksGame + returnLentColonistsInTicks;
		}

		public override void QuestPartTick()
		{
			base.QuestPartTick();
			if (Find.TickManager.TicksGame >= enableTick + returnLentColonistsInTicks)
			{
				Complete();
			}
		}

		protected override void Complete(SignalArgs signalArgs)
		{
			Map map = (returnMap == null) ? Find.AnyPlayerHomeMap : returnMap.Map;
			if (map != null)
			{
				base.Complete(new SignalArgs(new LookTargets(lentColonists).Named("SUBJECT"), lentColonists.Select((Thing c) => c.LabelShort).ToCommaList(useAnd: true).Named("PAWNS")));
				if (lendColonistsToFaction == Faction.Empire)
				{
					SkyfallerUtility.MakeDropoffShuttle(map, lentColonists, Faction.Empire);
				}
				else
				{
					DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, lentColonists, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: false);
				}
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
				Corpse val = pawn.MakeCorpse(assignedGrave, inBed: false, 0f);
				lentColonists.Remove(pawn);
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(anyPlayerHomeMap), anyPlayerHomeMap, Gen.YieldSingle(val), 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: false);
				}
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
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				lentColonists.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
