using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class SettlementProximityGoodwillUtility
	{
		private static List<Pair<Settlement, int>> tmpGoodwillOffsets = new List<Pair<Settlement, int>>();

		public static int MaxDist => Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Last().x);

		public static void CheckSettlementProximityGoodwillChange()
		{
			if (Find.TickManager.TicksGame == 0 || Find.TickManager.TicksGame % 900000 != 0)
			{
				return;
			}
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			tmpGoodwillOffsets.Clear();
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Faction == Faction.OfPlayer)
				{
					AppendProximityGoodwillOffsets(settlement.Tile, tmpGoodwillOffsets, ignoreIfAlreadyMinGoodwill: true, ignorePermanentlyHostile: false);
				}
			}
			if (!tmpGoodwillOffsets.Any())
			{
				return;
			}
			SortProximityGoodwillOffsets(tmpGoodwillOffsets);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			bool flag = false;
			TaggedString text = "LetterFactionBaseProximity".Translate() + "\n\n" + ProximityGoodwillOffsetsToString(tmpGoodwillOffsets);
			for (int j = 0; j < allFactionsListForReading.Count; j++)
			{
				Faction faction = allFactionsListForReading[j];
				if (faction == Faction.OfPlayer)
				{
					continue;
				}
				int num = 0;
				for (int k = 0; k < tmpGoodwillOffsets.Count; k++)
				{
					if (tmpGoodwillOffsets[k].First.Faction == faction)
					{
						num += tmpGoodwillOffsets[k].Second;
					}
				}
				FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
				if (faction.TryAffectGoodwillWith(Faction.OfPlayer, num, canSendMessage: false, canSendHostilityLetter: false))
				{
					flag = true;
					faction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, faction.PlayerRelationKind);
				}
			}
			if (flag)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseProximity".Translate(), text, LetterDefOf.NegativeEvent);
			}
		}

		public static void AppendProximityGoodwillOffsets(int tile, List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
		{
			int maxDist = MaxDist;
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Faction == null || settlement.Faction == Faction.OfPlayer || (ignorePermanentlyHostile && settlement.Faction.def.permanentEnemy) || (ignoreIfAlreadyMinGoodwill && settlement.Faction.PlayerGoodwill == -100))
				{
					continue;
				}
				int num = Find.WorldGrid.TraversalDistanceBetween(tile, settlement.Tile, passImpassable: false, maxDist);
				if (num != int.MaxValue)
				{
					int num2 = Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Evaluate(num));
					if (num2 != 0)
					{
						outOffsets.Add(new Pair<Settlement, int>(settlement, num2));
					}
				}
			}
		}

		public static void SortProximityGoodwillOffsets(List<Pair<Settlement, int>> offsets)
		{
			offsets.SortBy((Pair<Settlement, int> x) => x.First.Faction.loadID, (Pair<Settlement, int> x) => -Mathf.Abs(x.Second));
		}

		public static string ProximityGoodwillOffsetsToString(List<Pair<Settlement, int>> offsets)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < offsets.Count; i++)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + offsets[i].First.LabelCap + ": " + "ProximitySingleGoodwillChange".Translate(offsets[i].Second.ToStringWithSign(), offsets[i].First.Faction.Name));
			}
			return stringBuilder.ToString();
		}

		public static void CheckConfirmSettle(int tile, Action settleAction)
		{
			tmpGoodwillOffsets.Clear();
			AppendProximityGoodwillOffsets(tile, tmpGoodwillOffsets, ignoreIfAlreadyMinGoodwill: false, ignorePermanentlyHostile: true);
			if (tmpGoodwillOffsets.Any())
			{
				SortProximityGoodwillOffsets(tmpGoodwillOffsets);
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSettleNearFactionBase".Translate(MaxDist - 1, 15) + "\n\n" + ProximityGoodwillOffsetsToString(tmpGoodwillOffsets), settleAction));
			}
			else
			{
				settleAction();
			}
		}
	}
}
