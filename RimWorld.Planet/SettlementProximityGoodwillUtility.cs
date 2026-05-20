using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class SettlementProximityGoodwillUtility
{
	private static readonly List<Pair<Settlement, int>> tmpGoodwillOffsets = new List<Pair<Settlement, int>>();

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
		TaggedString text = "LetterFactionBaseProximity".Translate() + "\n\n" + ProximityGoodwillOffsetsToString(tmpGoodwillOffsets).ToLineList(" - ");
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
			if (num != 0)
			{
				FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
				Faction.OfPlayer.TryAffectGoodwillWith(faction, num, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.SettlementProximity);
				flag = true;
				faction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, faction.PlayerRelationKind);
			}
		}
		if (flag)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseProximity".Translate(), text, LetterDefOf.NegativeEvent);
		}
	}

	public static void AppendProximityGoodwillOffsets(PlanetTile tile, List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
	{
		int maxDist = MaxDist;
		List<Settlement> settlements = Find.WorldObjects.Settlements;
		for (int i = 0; i < settlements.Count; i++)
		{
			Settlement settlement = settlements[i];
			if (settlement.Faction == null || settlement.Faction == Faction.OfPlayer || (ignorePermanentlyHostile && settlement.Faction.def.permanentEnemy) || (ignoreIfAlreadyMinGoodwill && settlement.Faction.PlayerGoodwill == -100) || tile == settlement.Tile)
			{
				continue;
			}
			int num = Find.WorldGrid.TraversalDistanceBetween(tile, settlement.Tile, passImpassable: false, maxDist);
			if (num != int.MaxValue)
			{
				float x = (float)num * settlement.Tile.LayerDef.rangeDistanceFactor;
				int num2 = Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Evaluate(x));
				if (num2 != 0)
				{
					outOffsets.Add(new Pair<Settlement, int>(settlement, num2));
				}
			}
		}
	}

	private static void AppendGravshipAttackOffsets(PlanetTile tile, List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
	{
		Settlement settlement = Find.WorldObjects.SettlementAt(tile);
		if (settlement != null && !settlement.Faction.IsPlayer && (!ignorePermanentlyHostile || !settlement.Faction.def.permanentEnemy) && (!ignoreIfAlreadyMinGoodwill || settlement.Faction.PlayerGoodwill != -100))
		{
			outOffsets.Add(new Pair<Settlement, int>(settlement, Faction.OfPlayer.GoodwillToMakeHostile(settlement.Faction)));
		}
	}

	public static void SortProximityGoodwillOffsets(List<Pair<Settlement, int>> offsets)
	{
		offsets.SortBy((Pair<Settlement, int> x) => x.First.Faction.loadID, (Pair<Settlement, int> x) => -Mathf.Abs(x.Second));
	}

	public static IEnumerable<string> ProximityGoodwillOffsetsToString(List<Pair<Settlement, int>> offsets)
	{
		for (int i = 0; i < offsets.Count; i++)
		{
			yield return offsets[i].First.LabelCap + ": " + "ProximitySingleGoodwillChange".Translate(offsets[i].Second.ToStringWithSign(), offsets[i].First.Faction.Name);
		}
	}

	private static IEnumerable<TaggedString> GetConfirmationDescriptions(PlanetTile tile, Building_GravEngine gravEngine = null)
	{
		Settlement settlement = Find.WorldObjects.SettlementAt(tile);
		if (ModsConfig.OdysseyActive && gravEngine != null && settlement != null && !settlement.Faction.IsPlayer)
		{
			if (settlement.Faction.HostileTo(Faction.OfPlayer))
			{
				yield return "ConfirmLandOnHostileFactionBase".Translate(settlement.Faction);
			}
			else
			{
				yield return "ConfirmLandOnNeutralFactionBase".Translate(settlement.Faction);
			}
		}
		tmpGoodwillOffsets.Clear();
		AppendProximityGoodwillOffsets(tile, tmpGoodwillOffsets, ignoreIfAlreadyMinGoodwill: false, ignorePermanentlyHostile: true);
		if (tmpGoodwillOffsets.Any())
		{
			yield return "ConfirmSettleNearFactionBase".Translate(MaxDist - 1, 15);
		}
		if (ModsConfig.BiotechActive && NoxiousHazeUtility.TryGetNoxiousHazeMTB(tile, out var mtb))
		{
			yield return "ConfirmSettleNearPollution".Translate(mtb);
		}
		foreach (BiomeDef biome in tile.Tile.Biomes)
		{
			if (!biome.settleWarning.NullOrEmpty())
			{
				yield return biome.settleWarning;
			}
		}
		if (!ModsConfig.OdysseyActive || tile.LayerDef != PlanetLayerDefOf.Orbit)
		{
			yield break;
		}
		yield return "OrbitalWarning".Translate();
		if (gravEngine == null)
		{
			yield break;
		}
		yield return "OrbitalWarning_Gravship".Translate();
		foreach (TaggedString orbitalWarning in gravEngine.GetOrbitalWarnings())
		{
			yield return orbitalWarning;
		}
	}

	private static IEnumerable<string> GetConfirmationEffects(PlanetTile tile, bool gravship)
	{
		tmpGoodwillOffsets.Clear();
		if (gravship && ModsConfig.OdysseyActive)
		{
			AppendGravshipAttackOffsets(tile, tmpGoodwillOffsets, ignoreIfAlreadyMinGoodwill: false, ignorePermanentlyHostile: true);
		}
		AppendProximityGoodwillOffsets(tile, tmpGoodwillOffsets, ignoreIfAlreadyMinGoodwill: false, ignorePermanentlyHostile: true);
		SortProximityGoodwillOffsets(tmpGoodwillOffsets);
		foreach (string item in ProximityGoodwillOffsetsToString(tmpGoodwillOffsets))
		{
			yield return item;
		}
	}

	public static void CheckConfirmSettle(PlanetTile tile, Action settleAction, Action cancelAction = null, Building_GravEngine gravEngine = null)
	{
		IEnumerable<TaggedString> confirmationDescriptions = GetConfirmationDescriptions(tile, gravEngine);
		if (confirmationDescriptions.Any())
		{
			TaggedString text = new TaggedString("");
			foreach (TaggedString item in confirmationDescriptions)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += item;
			}
			if (gravEngine != null)
			{
				text += "\n\n" + "ConfirmLand".Translate();
			}
			else
			{
				text += "\n\n" + "ConfirmSettle".Translate();
			}
			IEnumerable<string> confirmationEffects = GetConfirmationEffects(tile, gravEngine != null);
			if (confirmationEffects.Any())
			{
				text += "\n\n" + confirmationEffects.ToLineList(" - ");
			}
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, settleAction, cancelAction));
		}
		else
		{
			settleAction();
		}
	}
}
