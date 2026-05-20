using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class GuestUtility
	{
		public static Texture2D SlaveSuppressionFillTex = SolidColorMaterials.NewSolidColorTexture(new Color32(245, 209, 66, byte.MaxValue));

		public static readonly Texture2D RansomIcon = ContentFinder<Texture2D>.Get("UI/Icons/Ransom");

		private static Texture2D slaveIcon;

		private static List<WorkTypeDef> tmpDisabledWorkTypes = new List<WorkTypeDef>();

		public static Texture2D SlaveIcon
		{
			get
			{
				if (slaveIcon == null)
				{
					slaveIcon = ContentFinder<Texture2D>.Get("UI/Icons/Slavery");
				}
				return slaveIcon;
			}
		}

		public static Texture2D GetGuestIcon(GuestStatus guestStatus)
		{
			if (guestStatus == GuestStatus.Slave)
			{
				return SlaveIcon;
			}
			return null;
		}

		public static List<WorkTypeDef> GetDisabledWorkTypes(this Pawn_GuestTracker guest)
		{
			tmpDisabledWorkTypes.Clear();
			if (guest.IsSlave)
			{
				foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
				{
					if (allDef.disabledForSlaves)
					{
						tmpDisabledWorkTypes.Add(allDef);
					}
				}
			}
			return tmpDisabledWorkTypes;
		}

		public static bool IsSellingToSlavery(Pawn slave, Faction slaveOwner)
		{
			if (slave.IsSlave || slave.IsPrisoner)
			{
				return slave.HomeFaction != slaveOwner;
			}
			return false;
		}

		public static void GetExtraFactionsFromGuestStatus(Pawn pawn, List<ExtraFaction> outExtraFactions)
		{
			if (pawn.SlaveFaction != null)
			{
				outExtraFactions.Add(new ExtraFaction(pawn.SlaveFaction, ExtraFactionType.HomeFaction));
			}
		}

		public static void Notify_PrisonerEscaped(Pawn prisoner)
		{
			ThoughtToAddToAll thoughtToAddToAll = new ThoughtToAddToAll(ThoughtDefOf.ColonyPrisonerEscaped, prisoner);
			foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive_Colonist != prisoner)
				{
					thoughtToAddToAll.Add(allMapsCaravansAndTravellingTransporters_Alive_Colonist);
				}
			}
		}

		public static bool PrisonerCanReturnToCell(Pawn pawn)
		{
			if (pawn.IsPrisoner)
			{
				return pawn.guest.ShouldWaitInsteadOfEscaping;
			}
			return false;
		}
	}
}
