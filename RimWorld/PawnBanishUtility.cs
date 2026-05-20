using System;
using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class PawnBanishUtility
{
	private const float DeathChanceForCaravanPawnBanishedToDie = 0.8f;

	private static readonly List<Hediff> tmpHediffs = new List<Hediff>();

	public static void Banish(Pawn pawn, bool giveThoughts = true)
	{
		Banish(pawn, PlanetTile.Invalid, giveThoughts);
	}

	public static void Banish(Pawn pawn, PlanetTile tile, bool giveThoughts = true)
	{
		if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
		{
			Log.Warning("Tried to banish " + pawn?.ToString() + " but he's neither a colonist, tame animal, nor prisoner.");
			return;
		}
		if (!tile.Valid)
		{
			tile = pawn.Tile;
		}
		bool flag = WouldBeLeftToDie(pawn, tile);
		if (!pawn.IsQuestLodger() && giveThoughts)
		{
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Banished : PawnDiedOrDownedThoughtsKind.BanishedToDie);
		}
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
			caravan.RemovePawn(pawn);
			if (flag)
			{
				if (Rand.Value < 0.8f)
				{
					pawn.Kill(null, null);
				}
				else
				{
					HealIfPossible(pawn);
				}
			}
		}
		if (pawn.guest != null)
		{
			pawn.guest.SetGuestStatus(null);
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			if (!pawn.Spawned && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, pawn.Faction != null && (int)pawn.Faction.def.techLevel >= 3))
			{
				if (pawn.Faction != faction)
				{
					pawn.SetFaction(faction);
				}
			}
			else if (pawn.Faction != null)
			{
				pawn.SetFaction(null);
			}
			Faction.OfPlayer.ideos?.RecalculateIdeosBasedOnPlayerPawns();
		}
		QuestUtility.SendQuestTargetSignals(pawn.questTags, "Banished", pawn.Named("SUBJECT"));
	}

	public static bool WouldBeLeftToDie(Pawn p, PlanetTile tile)
	{
		if (p.Downed)
		{
			return true;
		}
		if (p.health.hediffSet.BleedRateTotal > 0.4f)
		{
			return true;
		}
		if (tile.Valid)
		{
			float f = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, GenLocalDate.Twelfth(p));
			if (!p.SafeTemperatureRange().Includes(f))
			{
				return true;
			}
		}
		List<Hediff> hediffs = p.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].IsCurrentlyLifeThreatening)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetBanishPawnDialogText(Pawn banishedPawn)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = WouldBeLeftToDie(banishedPawn, banishedPawn.Tile);
		stringBuilder.Append("ConfirmBanishPawnDialog".Translate(banishedPawn.Label, banishedPawn).Resolve());
		if (flag)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("ConfirmBanishPawnDialog_LeftToDie".Translate(banishedPawn.LabelShort, banishedPawn).Resolve().CapitalizeFirst());
		}
		List<ThingWithComps> list = ((banishedPawn.equipment != null) ? banishedPawn.equipment.AllEquipmentListForReading : null);
		List<Apparel> list2 = ((banishedPawn.apparel != null) ? banishedPawn.apparel.WornApparel : null);
		ThingOwner<Thing> thingOwner = ((banishedPawn.inventory != null && WillTakeInventoryIfBanished(banishedPawn)) ? banishedPawn.inventory.innerContainer : null);
		if (!list.NullOrEmpty() || !list2.NullOrEmpty() || !thingOwner.NullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("ConfirmBanishPawnDialog_Items".Translate(banishedPawn.LabelShort, banishedPawn).Resolve().CapitalizeFirst()
				.AdjustedFor(banishedPawn));
			stringBuilder.AppendLine();
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("  - " + list[i].LabelCap);
				}
			}
			if (list2 != null)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("  - " + list2[j].LabelCap);
				}
			}
			if (thingOwner != null)
			{
				for (int k = 0; k < thingOwner.Count; k++)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("  - " + thingOwner[k].LabelCap);
				}
			}
		}
		if (!banishedPawn.IsQuestLodger() && (banishedPawn.guilt == null || !banishedPawn.guilt.IsGuilty))
		{
			PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(banishedPawn, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Banished : PawnDiedOrDownedThoughtsKind.BanishedToDie, stringBuilder, "\n\n" + "ConfirmBanishPawnDialog_IndividualThoughts".Translate(banishedPawn.LabelShort, banishedPawn), "\n\n" + "ConfirmBanishPawnDialog_AllColonistsThoughts".Translate());
		}
		return stringBuilder.ToString();
	}

	public static void ShowBanishPawnConfirmationDialog(Pawn pawn, Action onConfirm = null)
	{
		Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(GetBanishPawnDialogText(pawn), delegate
		{
			Banish(pawn);
			onConfirm?.Invoke();
		}, destructive: true);
		Find.WindowStack.Add(window);
	}

	public static string GetBanishButtonTip(Pawn pawn)
	{
		if (WouldBeLeftToDie(pawn, pawn.Tile))
		{
			return "BanishTip".Translate() + "\n\n" + "BanishTipWillDie".Translate(pawn.LabelShort, pawn).CapitalizeFirst();
		}
		return "BanishTip".Translate();
	}

	private static void HealIfPossible(Pawn p)
	{
		tmpHediffs.Clear();
		tmpHediffs.AddRange(p.health.hediffSet.hediffs);
		for (int i = 0; i < tmpHediffs.Count; i++)
		{
			if (tmpHediffs[i] is Hediff_Injury hediff_Injury && !hediff_Injury.IsPermanent())
			{
				p.health.RemoveHediff(hediff_Injury);
				continue;
			}
			ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(tmpHediffs[i].def);
			if (immunityRecord != null)
			{
				immunityRecord.immunity = 1f;
			}
		}
	}

	private static bool WillTakeInventoryIfBanished(Pawn pawn)
	{
		return !pawn.IsCaravanMember();
	}
}
