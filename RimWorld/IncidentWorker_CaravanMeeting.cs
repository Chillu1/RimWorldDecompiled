using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_CaravanMeeting : IncidentWorker
{
	private const int MapSize = 100;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (parms.target is Map)
		{
			return true;
		}
		Faction faction;
		if (CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile))
		{
			return TryFindFaction(out faction);
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (parms.target is Map)
		{
			return IncidentDefOf.TravelerGroup.Worker.TryExecute(parms);
		}
		Caravan caravan = (Caravan)parms.target;
		if (!TryFindFaction(out var faction))
		{
			return false;
		}
		List<Pawn> list = GenerateCaravanPawns(faction);
		if (!list.Any())
		{
			Log.Error("IncidentWorker_CaravanMeeting could not generate any pawns.");
			return false;
		}
		Caravan metCaravan = CaravanMaker.MakeCaravan(list, faction, PlanetTile.Invalid, addToWorldPawnsIfNotAlready: false);
		CameraJumper.TryJumpAndSelect(caravan);
		DiaNode diaNode = new DiaNode("CaravanMeeting".Translate(caravan.Name, faction.NameColored, PawnUtility.PawnKindsToLineList(metCaravan.PawnsListForReading.Select((Pawn x) => x.kindDef), "  - ")).Resolve().CapitalizeFirst());
		Pawn bestPlayerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, metCaravan.TraderKind);
		if (metCaravan.CanTradeNow)
		{
			DiaOption diaOption = new DiaOption("CaravanMeeting_Trade".Translate());
			diaOption.action = delegate
			{
				Find.WindowStack.Add(new Dialog_Trade(bestPlayerNegotiator, metCaravan));
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(metCaravan.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithOtherCaravan".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
			};
			if (bestPlayerNegotiator == null)
			{
				if (metCaravan.TraderKind.permitRequiredForTrading != null && !caravan.pawns.Any((Pawn p) => p.royalty != null && p.royalty.HasPermit(metCaravan.TraderKind.permitRequiredForTrading, faction)))
				{
					RoyalTitleDef royalTitleDef = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First((RoyalTitleDef t) => t.permits != null && t.permits.Contains(metCaravan.TraderKind.permitRequiredForTrading));
					diaOption.Disable("CaravanMeeting_NoPermit".Translate(royalTitleDef.GetLabelForBothGenders(), faction).Resolve());
				}
				else
				{
					diaOption.Disable("CaravanMeeting_TradeIncapable".Translate());
				}
			}
			diaNode.options.Add(diaOption);
		}
		DiaOption diaOption2 = new DiaOption("CaravanMeeting_Attack".Translate());
		diaOption2.action = delegate
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				Pawn pawn = caravan.PawnsListForReading[0];
				Faction.OfPlayer.TryAffectGoodwillWith(faction, Faction.OfPlayer.GoodwillToMakeHostile(faction), canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.AttackedCaravan, pawn);
				Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(100, 1, 100), WorldObjectDefOf.AttackedNonPlayerCaravan);
				map.Parent.SetFaction(faction);
				MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out var playerSpot, out var enemySpot);
				CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(playerSpot, map, 12), CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
				List<Pawn> list2 = metCaravan.PawnsListForReading.ToList();
				CaravanEnterMapUtility.Enter(metCaravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(enemySpot, map, 12));
				LordMaker.MakeNewLord(faction, new LordJob_DefendAttackedTraderCaravan(list2[0].Position), map, list2);
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				CameraJumper.TryJumpAndSelect(pawn);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(list2, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
			}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		};
		diaOption2.resolveTree = true;
		diaNode.options.Add(diaOption2);
		DiaOption diaOption3 = new DiaOption("CaravanMeeting_MoveOn".Translate());
		diaOption3.action = delegate
		{
			RemoveAllPawnsAndPassToWorld(metCaravan);
		};
		diaOption3.resolveTree = true;
		diaNode.options.Add(diaOption3);
		string title = "CaravanMeetingTitle".Translate(caravan.Label);
		Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, faction, delayInteractivity: true, radioMode: false, title));
		Find.Archive.Add(new ArchivedDialog(diaNode.text, title, faction));
		return true;
	}

	private bool TryFindFaction(out Faction faction)
	{
		return Find.FactionManager.AllFactionsListForReading.Where((Faction x) => !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && !x.Hidden && x.def.humanlikeFaction && !x.temporary && x.def.caravanTraderKinds.Any() && !x.def.pawnGroupMakers.NullOrEmpty()).TryRandomElement(out faction);
	}

	private List<Pawn> GenerateCaravanPawns(Faction faction)
	{
		return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Trader,
			faction = faction,
			points = TraderCaravanUtility.GenerateGuardPoints(),
			dontUseSingleUseRocketLaunchers = true
		}).ToList();
	}

	private void RemoveAllPawnsAndPassToWorld(Caravan caravan)
	{
		List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
		for (int i = 0; i < pawnsListForReading.Count; i++)
		{
			Find.WorldPawns.PassToWorld(pawnsListForReading[i]);
		}
		caravan.RemoveAllPawns();
	}
}
