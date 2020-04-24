using Verse;

namespace RimWorld
{
	public static class EquipmentUtility
	{
		public static bool CanEquip(Thing thing, Pawn pawn)
		{
			string cantReason;
			return CanEquip(thing, pawn, out cantReason);
		}

		public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason)
		{
			cantReason = null;
			CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn != null && compBladelinkWeapon.bondedPawn != pawn)
			{
				cantReason = "BladelinkBondedToSomeoneElse".Translate();
				return false;
			}
			if (IsBiocoded(thing) && !IsBiocodedFor(thing, pawn))
			{
				cantReason = "BiocodedCodedForSomeoneElse".Translate();
				return false;
			}
			return true;
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

		public static bool QuestLodgerCanEquip(Thing thing, Pawn pawn)
		{
			if (IsBiocodedFor(thing, pawn))
			{
				return true;
			}
			if (thing.def.IsWeapon)
			{
				return pawn.equipment.Primary == null;
			}
			return false;
		}
	}
}
