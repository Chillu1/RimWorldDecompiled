using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class MechRepairUtility
{
	private static Hediff GetHediffToHeal(Pawn mech)
	{
		Hediff hediff = null;
		float num = float.PositiveInfinity;
		foreach (Hediff hediff2 in mech.health.hediffSet.hediffs)
		{
			if (hediff2 is Hediff_Injury && hediff2.Severity < num)
			{
				num = hediff2.Severity;
				hediff = hediff2;
			}
		}
		if (hediff != null)
		{
			return hediff;
		}
		foreach (Hediff hediff3 in mech.health.hediffSet.hediffs)
		{
			if (hediff3 is Hediff_MissingPart)
			{
				return hediff3;
			}
		}
		return null;
	}

	public static bool CanRepair(Pawn mech)
	{
		if (mech.TryGetComp<CompMechRepairable>() == null)
		{
			return false;
		}
		if (GetHediffToHeal(mech) == null)
		{
			return IsMissingWeapon(mech);
		}
		return true;
	}

	public static void RepairTick(Pawn mech, int delta)
	{
		Hediff hediffToHeal = GetHediffToHeal(mech);
		if (hediffToHeal != null)
		{
			if (hediffToHeal is Hediff_MissingPart hediff)
			{
				mech.health.RemoveHediff(hediff);
			}
			else
			{
				hediffToHeal.Heal(delta);
			}
		}
		else
		{
			GenerateWeapon(mech);
		}
	}

	public static bool IsMissingWeapon(Pawn mech)
	{
		List<string> weaponTags = mech.kindDef.weaponTags;
		if (weaponTags.NullOrEmpty())
		{
			return false;
		}
		List<ThingWithComps> allEquipmentListForReading = mech.equipment.AllEquipmentListForReading;
		for (int i = 0; i < allEquipmentListForReading.Count; i++)
		{
			if (allEquipmentListForReading[i].def.weaponTags.Any((string t) => weaponTags.Contains(t)))
			{
				return false;
			}
		}
		return true;
	}

	public static void GenerateWeapon(Pawn mech)
	{
		if (IsMissingWeapon(mech))
		{
			PawnWeaponGenerator.TryGenerateWeaponFor(mech, default(PawnGenerationRequest));
		}
	}
}
