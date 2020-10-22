using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_VisitorGroup : IncidentWorker_NeutralGroup
	{
		private const float TraderChance = 0.75f;

		private static readonly SimpleCurve PointsCurve = new SimpleCurve
		{
			new CurvePoint(45f, 0f),
			new CurvePoint(50f, 1f),
			new CurvePoint(100f, 1f),
			new CurvePoint(200f, 0.25f),
			new CurvePoint(300f, 0.1f),
			new CurvePoint(500f, 0f)
		};

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryResolveParms(parms))
			{
				return false;
			}
			List<Pawn> list = SpawnPawns(parms);
			if (list.Count == 0)
			{
				return false;
			}
			RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out var result);
			LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, result);
			LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
			bool flag = false;
			if (Rand.Value < 0.75f)
			{
				flag = TryConvertOnePawnToSmallTrader(list, parms.faction, map);
			}
			Pawn pawn = list.Find((Pawn x) => parms.faction.leader == x);
			TaggedString letterLabel;
			TaggedString letterText;
			if (list.Count == 1)
			{
				TaggedString value = (flag ? ("\n\n" + "SingleVisitorArrivesTraderInfo".Translate(list[0].Named("PAWN")).AdjustedFor(list[0])) : ((TaggedString)""));
				TaggedString value2 = ((pawn != null) ? ("\n\n" + "SingleVisitorArrivesLeaderInfo".Translate(list[0].Named("PAWN")).AdjustedFor(list[0])) : ((TaggedString)""));
				letterLabel = "LetterLabelSingleVisitorArrives".Translate();
				letterText = "SingleVisitorArrives".Translate(list[0].story.Title, parms.faction.NameColored, list[0].Name.ToStringFull, value, value2, list[0].Named("PAWN")).AdjustedFor(list[0]);
			}
			else
			{
				TaggedString value3 = (flag ? ("\n\n" + "GroupVisitorsArriveTraderInfo".Translate()) : TaggedString.Empty);
				TaggedString value4 = ((pawn != null) ? ("\n\n" + "GroupVisitorsArriveLeaderInfo".Translate(pawn.LabelShort, pawn)) : TaggedString.Empty);
				letterLabel = "LetterLabelGroupVisitorsArrive".Translate();
				letterText = "GroupVisitorsArrive".Translate(parms.faction.NameColored, value3, value4);
			}
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			SendStandardLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, parms, list[0]);
			return true;
		}

		protected override void ResolveParmsPoints(IncidentParms parms)
		{
			if (!(parms.points >= 0f))
			{
				parms.points = Rand.ByCurve(PointsCurve);
			}
		}

		private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction, Map map)
		{
			if (faction.def.visitorTraderKinds.NullOrEmpty())
			{
				return false;
			}
			Pawn pawn = pawns.RandomElement();
			Lord lord = pawn.GetLord();
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
			TraderKindDef traderKindDef = faction.def.visitorTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality);
			pawn.trader.traderKind = traderKindDef;
			pawn.inventory.DestroyAll();
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = traderKindDef;
			parms.tile = map.Tile;
			parms.makingFaction = faction;
			foreach (Thing item in ThingSetMakerDefOf.TraderStock.root.Generate(parms))
			{
				Pawn pawn2 = item as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.Faction != pawn.Faction)
					{
						pawn2.SetFaction(pawn.Faction);
					}
					IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5);
					GenSpawn.Spawn(pawn2, loc, map);
					lord.AddPawn(pawn2);
				}
				else if (!pawn.inventory.innerContainer.TryAdd(item))
				{
					item.Destroy();
				}
			}
			PawnInventoryGenerator.GiveRandomFood(pawn);
			return true;
		}
	}
}
