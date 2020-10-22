using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_AttackSettlement : TransportPodsArrivalAction
	{
		private Settlement settlement;

		private PawnsArrivalModeDef arrivalMode;

		public TransportPodsArrivalAction_AttackSettlement()
		{
		}

		public TransportPodsArrivalAction_AttackSettlement(Settlement settlement, PawnsArrivalModeDef arrivalMode)
		{
			this.settlement = settlement;
			this.arrivalMode = arrivalMode;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
			Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (settlement != null && settlement.Tile != destinationTile)
			{
				return false;
			}
			return CanAttack(pods, settlement);
		}

		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !settlement.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			bool num = !settlement.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
			TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
			TaggedString letterText = "LetterTransportPodsLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
			SettlementUtility.AffectRelationsOnAttacked_NewTmp(settlement, ref letterText);
			if (num)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget, settlement.Faction);
			arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
		}

		public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Attackable)
			{
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyNonDownedColonist(pods))
			{
				return false;
			}
			if (settlement.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailReasonAndMessage("EnterCooldownBlocksEntering".Translate(), "MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (representative.parent.TryGetComp<CompShuttle>() != null)
			{
				foreach (FloatMenuOption floatMenuOption in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportPodsArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.Shuttle), "AttackShuttle".Translate(settlement.Label), representative, settlement.Tile))
				{
					yield return floatMenuOption;
				}
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportPodsArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop), "AttackAndDropAtEdge".Translate(settlement.Label), representative, settlement.Tile))
			{
				yield return floatMenuOption2;
			}
			foreach (FloatMenuOption floatMenuOption3 in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportPodsArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop), "AttackAndDropInCenter".Translate(settlement.Label), representative, settlement.Tile))
			{
				yield return floatMenuOption3;
			}
		}
	}
}
