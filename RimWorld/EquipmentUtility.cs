using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class EquipmentUtility
{
	private static readonly SimpleCurve RecoilCurveAxisX = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 0.02f),
		new CurvePoint(2f, 0.03f)
	};

	private static readonly SimpleCurve RecoilCurveAxisY = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 0.05f),
		new CurvePoint(2f, 0.075f)
	};

	private static readonly SimpleCurve RecoilCurveRotation = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 3f),
		new CurvePoint(2f, 4f)
	};

	private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

	public static bool CanEquip(Thing thing, Pawn pawn)
	{
		string cantReason;
		return CanEquip(thing, pawn, out cantReason);
	}

	public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
	{
		cantReason = null;
		CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon != null && compBladelinkWeapon.Biocodable && compBladelinkWeapon.CodedPawn != null && compBladelinkWeapon.CodedPawn != pawn)
		{
			cantReason = "BladelinkBondedToSomeoneElse".Translate();
			return false;
		}
		if (CompBiocodable.IsBiocoded(thing) && !CompBiocodable.IsBiocodedFor(thing, pawn))
		{
			cantReason = "BiocodedCodedForSomeoneElse".Translate();
			return false;
		}
		if (checkBonded && AlreadyBondedToWeapon(thing, pawn))
		{
			cantReason = "BladelinkAlreadyBondedMessage".Translate(pawn.Named("PAWN"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
			return false;
		}
		if (RolePreventsFromUsing(pawn, thing, out cantReason))
		{
			return false;
		}
		if (thing.def.IsApparel && !thing.def.apparel.developmentalStageFilter.Has(pawn.DevelopmentalStage))
		{
			cantReason = "WrongDevelopmentalStageForClothing".Translate(pawn.DevelopmentalStage.ToString().Translate(), Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(thing.def.apparel.developmentalStageFilter.ToCommaListOr()));
			return false;
		}
		return true;
	}

	public static bool AlreadyBondedToWeapon(Thing thing, Pawn pawn)
	{
		CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon == null || !compBladelinkWeapon.Biocodable)
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
		if (compBladelinkWeapon != null && compBladelinkWeapon.Biocodable && compBladelinkWeapon.CodedPawn != p)
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

	public static bool IsBondedTo(Thing thing, Pawn pawn)
	{
		CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon != null)
		{
			return compBladelinkWeapon.CodedPawn == pawn;
		}
		return false;
	}

	public static bool QuestLodgerCanEquip(Thing thing, Pawn pawn)
	{
		if (pawn.equipment.Primary != null && !QuestLodgerCanUnequip(pawn.equipment.Primary, pawn))
		{
			return false;
		}
		if (CompBiocodable.IsBiocodedFor(thing, pawn))
		{
			return true;
		}
		if (AlreadyBondedToWeapon(thing, pawn))
		{
			return true;
		}
		return thing.def.IsWeapon;
	}

	public static bool RolePreventsFromUsing(Pawn pawn, Thing thing, out string reason)
	{
		if (ModsConfig.IdeologyActive && pawn.Ideo != null)
		{
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (role != null && !role.CanEquip(pawn, thing, out reason))
			{
				return true;
			}
		}
		reason = null;
		return false;
	}

	public static bool QuestLodgerCanUnequip(Thing thing, Pawn pawn)
	{
		if (CompBiocodable.IsBiocodedFor(thing, pawn))
		{
			return false;
		}
		if (IsBondedTo(thing, pawn))
		{
			return false;
		}
		return true;
	}

	public static Verb_LaunchProjectile GetRecoilVerb(List<Verb> allWeaponVerbs)
	{
		Verb_LaunchProjectile verb_LaunchProjectile = null;
		foreach (Verb allWeaponVerb in allWeaponVerbs)
		{
			if (allWeaponVerb is Verb_LaunchProjectile verb_LaunchProjectile2 && (verb_LaunchProjectile == null || verb_LaunchProjectile.LastShotTick < verb_LaunchProjectile2.LastShotTick))
			{
				verb_LaunchProjectile = verb_LaunchProjectile2;
			}
		}
		return verb_LaunchProjectile;
	}

	public static void Recoil(ThingDef weaponDef, Verb_LaunchProjectile shootVerb, out Vector3 drawOffset, out float angleOffset, float aimAngle)
	{
		drawOffset = Vector3.zero;
		angleOffset = 0f;
		if (!(weaponDef.recoilPower > 0f) || shootVerb == null)
		{
			return;
		}
		Rand.PushState(shootVerb.LastShotTick);
		try
		{
			int num = Find.TickManager.TicksGame - shootVerb.LastShotTick;
			if ((float)num < weaponDef.recoilRelaxation)
			{
				float num2 = Mathf.Clamp01((float)num / weaponDef.recoilRelaxation);
				float num3 = Mathf.Lerp(weaponDef.recoilPower, 0f, num2);
				drawOffset = new Vector3((float)Rand.Sign * RecoilCurveAxisX.Evaluate(num2), 0f, 0f - RecoilCurveAxisY.Evaluate(num2)) * num3;
				angleOffset = (float)Rand.Sign * RecoilCurveRotation.Evaluate(num2) * num3;
				aimAngle += angleOffset;
				drawOffset = drawOffset.RotatedBy(aimAngle);
			}
		}
		finally
		{
			Rand.PopState();
		}
	}

	public static bool TryGenerateWeaponByTag(string weaponTag, out Thing weapon, float marketValue = 99999f)
	{
		workingWeapons.Clear();
		weapon = null;
		foreach (ThingStuffPair allWeaponPair in PawnWeaponGenerator.AllWeaponPairs)
		{
			if (!(allWeaponPair.Price > marketValue) && allWeaponPair.thing.weaponTags != null && allWeaponPair.thing.weaponTags.Contains(weaponTag))
			{
				workingWeapons.Add(allWeaponPair);
			}
		}
		if (workingWeapons.Count == 0)
		{
			return false;
		}
		if (!workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out var result))
		{
			return false;
		}
		weapon = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
		return true;
	}
}
