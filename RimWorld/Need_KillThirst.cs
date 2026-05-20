using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Need_KillThirst : Need
{
	public const float FallPerDay = 1f / 30f;

	private const float MinAgeForNeed = 13f;

	protected override bool IsFrozen
	{
		get
		{
			if ((float)pawn.ageTracker.AgeBiologicalYears < 13f)
			{
				return true;
			}
			return base.IsFrozen;
		}
	}

	public override bool ShowOnNeedList
	{
		get
		{
			if ((float)pawn.ageTracker.AgeBiologicalYears < 13f)
			{
				return false;
			}
			return base.ShowOnNeedList;
		}
	}

	public Need_KillThirst(Pawn newPawn)
		: base(newPawn)
	{
		threshPercents = new List<float> { 0.3f };
	}

	public override void NeedInterval()
	{
		if (!IsFrozen)
		{
			CurLevel -= 8.333333E-05f;
		}
	}

	public void Notify_KilledPawn(DamageInfo? dinfo)
	{
		if (dinfo.HasValue && (dinfo?.WeaponBodyPartGroup != null || dinfo?.WeaponLinkedHediff != null || (dinfo.Value.Weapon != null && dinfo.Value.Weapon.IsMeleeWeapon)))
		{
			CurLevel = 1f;
		}
	}
}
