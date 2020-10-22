using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class EquipmentUtility
	{
		public static bool CanEquip(Thing thing, Pawn pawn)
		{
			string cantReason;
			return CanEquip_NewTmp(thing, pawn, out cantReason);
		}

		[Obsolete("Only used for mod compatibility")]
		public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason)
		{
			return CanEquip_NewTmp(thing, pawn, out cantReason);
		}

		public static bool CanEquip_NewTmp(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
		{
			cantReason = null;
			CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null && compBladelinkWeapon.Bondable && compBladelinkWeapon.bondedPawn != null && compBladelinkWeapon.bondedPawn != pawn)
			{
				cantReason = "BladelinkBondedToSomeoneElse".Translate();
				return false;
			}
			if (IsBiocoded(thing) && !IsBiocodedFor(thing, pawn))
			{
				cantReason = "BiocodedCodedForSomeoneElse".Translate();
				return false;
			}
			if (checkBonded && AlreadyBondedToWeapon(thing, pawn))
			{
				cantReason = "BladelinkAlreadyBondedMessage".Translate(pawn.Named("PAWN"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
				return false;
			}
			return true;
		}

		public static bool AlreadyBondedToWeapon(Thing thing, Pawn pawn)
		{
			CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon == null || !compBladelinkWeapon.Bondable)
			{
				return false;
			}
			if (pawn.equipment.bondedWeapon != null)
			{
				return pawn.equipment.bondedWeapon != thing;
			}
			return false;
		}

		public static string GetPersonaWeaponConfirmationText(Thing item, Pawn p)
		{
			CompBladelinkWeapon compBladelinkWeapon = item.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null && compBladelinkWeapon.Bondable && compBladelinkWeapon.bondedPawn != p)
			{
				TaggedString taggedString = "BladelinkEquipWarning".Translate();
				List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
				if (!traitsListForReading.NullOrEmpty())
				{
					taggedString += "\n\n" + "BladelinkEquipWarningTraits".Translate() + ":";
					for (int i = 0; i < traitsListForReading.Count; i++)
					{
						taggedString += "\n\n" + traitsListForReading[i].LabelCap + ": " + traitsListForReading[i].description;
					}
				}
				taggedString += "\n\n" + "RoyalWeaponEquipConfirmation".Translate();
				return taggedString;
			}
			return null;
		}

		public static bool IsBiocoded(Thing thing)
		{
			return thing.TryGetComp<CompBiocodable>()?.Biocoded ?? false;
		}

		public static bool IsBiocodedFor(Thing thing, Pawn pawn)
		{
			CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
			if (compBiocodable != null)
			{
				return compBiocodable.CodedPawn == pawn;
			}
			return false;
		}

		public static bool IsBondedTo(Thing thing, Pawn pawn)
		{
			CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null)
			{
				return compBladelinkWeapon.bondedPawn == pawn;
			}
			return false;
		}

		public static bool QuestLodgerCanEquip(Thing thing, Pawn pawn)
		{
			if (pawn.equipment.Primary != null && !QuestLodgerCanUnequip(pawn.equipment.Primary, pawn))
			{
				return false;
			}
			if (IsBiocodedFor(thing, pawn))
			{
				return true;
			}
			if (AlreadyBondedToWeapon(thing, pawn))
			{
				return true;
			}
			return thing.def.IsWeapon;
		}

		public static bool QuestLodgerCanUnequip(Thing thing, Pawn pawn)
		{
			if (IsBiocodedFor(thing, pawn))
			{
				return false;
			}
			if (IsBondedTo(thing, pawn))
			{
				return false;
			}
			return true;
		}
	}
}
