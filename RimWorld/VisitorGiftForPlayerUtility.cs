using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class VisitorGiftForPlayerUtility
	{
		public static float ChanceToLeaveGift(Faction faction, Map map)
		{
			if (faction.IsPlayer)
			{
				return 0f;
			}
			return 0.25f * PlayerWealthChanceFactor(map) * FactionRelationsChanceFactor(faction);
		}

		public static List<Thing> GenerateGifts(Faction faction, Map map)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = DiplomacyTuning.VisitorGiftTotalMarketValueRangeBase * DiplomacyTuning.VisitorGiftTotalMarketValueFactorFromPlayerWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
			return ThingSetMakerDefOf.VisitorGift.root.Generate(parms);
		}

		private static float PlayerWealthChanceFactor(Map map)
		{
			return DiplomacyTuning.VisitorGiftChanceFactorFromPlayerWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
		}

		private static float FactionRelationsChanceFactor(Faction faction)
		{
			if (faction.HostileTo(Faction.OfPlayer))
			{
				return 0f;
			}
			return DiplomacyTuning.VisitorGiftChanceFactorFromGoodwillCurve.Evaluate(faction.PlayerGoodwill);
		}

		public static void GiveGift(List<Pawn> possibleGivers, Faction faction)
		{
			if (possibleGivers.NullOrEmpty())
			{
				return;
			}
			Pawn pawn = null;
			for (int i = 0; i < possibleGivers.Count; i++)
			{
				if (possibleGivers[i].RaceProps.Humanlike && possibleGivers[i].Faction == faction)
				{
					pawn = possibleGivers[i];
					break;
				}
			}
			if (pawn == null)
			{
				for (int j = 0; j < possibleGivers.Count; j++)
				{
					if (possibleGivers[j].Faction == faction)
					{
						pawn = possibleGivers[j];
						break;
					}
				}
			}
			if (pawn == null)
			{
				pawn = possibleGivers[0];
			}
			List<Thing> list = GenerateGifts(faction, pawn.Map);
			TargetInfo target = TargetInfo.Invalid;
			for (int k = 0; k < list.Count; k++)
			{
				if (GenPlace.TryPlaceThing(list[k], pawn.Position, pawn.Map, ThingPlaceMode.Near))
				{
					target = list[k];
				}
				else
				{
					list[k].Destroy();
				}
			}
			if (target.IsValid)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelVisitorsGaveGift".Translate(pawn.Faction.Name), "LetterVisitorsGaveGift".Translate(pawn.Faction.def.pawnsPlural, list.Select((Thing g) => g.LabelCap).ToLineList("   -"), pawn.Named("PAWN")).AdjustedFor(pawn), LetterDefOf.PositiveEvent, target, faction);
			}
		}

		[DebugOutput]
		private static void VisitorGiftChance()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Current wealth factor (wealth=" + Find.CurrentMap.wealthWatcher.WealthTotal.ToString("F0") + "): ");
			stringBuilder.AppendLine(PlayerWealthChanceFactor(Find.CurrentMap).ToStringPercent());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Chance per faction:");
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (!allFaction.IsPlayer && !allFaction.HostileTo(Faction.OfPlayer) && !allFaction.Hidden)
				{
					stringBuilder.Append(allFaction.Name + " (" + allFaction.PlayerGoodwill.ToStringWithSign() + ", " + allFaction.PlayerRelationKind.GetLabel() + ")");
					stringBuilder.Append(": " + ChanceToLeaveGift(allFaction, Find.CurrentMap).ToStringPercent());
					stringBuilder.AppendLine(" (rels factor: " + FactionRelationsChanceFactor(allFaction).ToStringPercent() + ")");
				}
			}
			int num = 0;
			for (int i = 0; i < 6; i++)
			{
				StorytellerUtility.DebugGetFutureIncidents(60, currentMapOnly: true, out var _, out var _, out var allIncidents, out var _);
				for (int j = 0; j < allIncidents.Count; j++)
				{
					if ((allIncidents[j].First == IncidentDefOf.VisitorGroup || allIncidents[j].First == IncidentDefOf.TraderCaravanArrival) && Rand.Chance(ChanceToLeaveGift(allIncidents[j].Second.faction ?? Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false), Find.CurrentMap)))
					{
						num++;
					}
				}
			}
			float num2 = (float)num / 6f;
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Calculated number of gifts received on average within the next 1 year");
			stringBuilder.AppendLine("(assuming current wealth and faction relations)");
			stringBuilder.Append("  = " + num2.ToString("0.##"));
			Log.Message(stringBuilder.ToString());
		}
	}
}
