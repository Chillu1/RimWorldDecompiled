using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class PawnBanishUtility
	{
		private const float DeathChanceForCaravanPawnBanishedToDie = 0.8f;

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		public static void Banish(Pawn pawn, int tile = -1)
		{
			if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
			{
				Log.Warning("Tried to banish " + pawn + " but he's neither a colonist, tame animal, nor prisoner.");
				return;
			}
			if (tile == -1)
			{
				tile = pawn.Tile;
			}
			bool flag = WouldBeLeftToDie(pawn, tile);
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Banished : PawnDiedOrDownedThoughtsKind.BanishedToDie);
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
			if (pawn.Faction != Faction.OfPlayer)
			{
				return;
			}
			if (!pawn.Spawned && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction faction, pawn.Faction != null && (int)pawn.Faction.def.techLevel >= 3))
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
		}

		public static bool WouldBeLeftToDie(Pawn p, int tile)
		{
			if (p.Downed)
			{
				return true;
			}
			if (p.health.hediffSet.BleedRateTotal > 0.4f)
			{
				return true;
			}
			if (tile != -1)
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
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.lifeThreatening)
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
			List<ThingWithComps> list = (banishedPawn.equipment != null) ? banishedPawn.equipment.AllEquipmentListForReading : null;
			List<Apparel> list2 = (banishedPawn.apparel != null) ? banishedPawn.apparel.WornApparel : null;
			ThingOwner<Thing> thingOwner = (banishedPawn.inventory != null && WillTakeInventoryIfBanished(banishedPawn)) ? banishedPawn.inventory.innerContainer : null;
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
			PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(banishedPawn, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Banished : PawnDiedOrDownedThoughtsKind.BanishedToDie, stringBuilder, "\n\n" + "ConfirmBanishPawnDialog_IndividualThoughts".Translate(banishedPawn.LabelShort, banishedPawn), "\n\n" + "ConfirmBanishPawnDialog_AllColonistsThoughts".Translate());
			return stringBuilder.ToString();
		}

		public static void ShowBanishPawnConfirmationDialog(Pawn pawn)
		{
			Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(GetBanishPawnDialogText(pawn), delegate
			{
				Banish(pawn);
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
				Hediff_Injury hediff_Injury = tmpHediffs[i] as Hediff_Injury;
				if (hediff_Injury != null && !hediff_Injury.IsPermanent())
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
}
