using Verse;

namespace RimWorld;

public class ThoughtWorker_WeaponTrait : ThoughtWorker
{
	public override string PostProcessDescription(Pawn p, string description)
	{
		return base.PostProcessDescription(p, description.Formatted(WeaponName(p).Named("WEAPON")));
	}

	public override string PostProcessLabel(Pawn p, string label)
	{
		return base.PostProcessLabel(p, label.Formatted(WeaponName(p).Named("WEAPON")));
	}

	protected string WeaponName(Pawn pawn)
	{
		if (pawn.equipment.bondedWeapon == null)
		{
			return string.Empty;
		}
		if (pawn.equipment.bondedWeapon.StyleSourcePrecept != null)
		{
			return pawn.equipment.bondedWeapon.StyleSourcePrecept.Label;
		}
		CompGeneratedNames compGeneratedNames = pawn.equipment.bondedWeapon.TryGetComp<CompGeneratedNames>();
		if (compGeneratedNames != null)
		{
			return compGeneratedNames.Name;
		}
		return pawn.equipment.bondedWeapon.LabelNoCount;
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.equipment?.bondedWeapon == null || p.equipment.bondedWeapon.Destroyed)
		{
			return ThoughtState.Inactive;
		}
		CompBladelinkWeapon compBladelinkWeapon = p.equipment.bondedWeapon.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon == null || compBladelinkWeapon.TraitsListForReading.NullOrEmpty())
		{
			return ThoughtState.Inactive;
		}
		return true;
	}
}
