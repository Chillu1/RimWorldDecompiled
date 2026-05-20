using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class ArmorUtility
{
	public const float MaxArmorRating = 2f;

	public const float DeflectThresholdFactor = 0.5f;

	public static float GetPostArmorDamage(Pawn pawn, float amount, float armorPenetration, BodyPartRecord part, ref DamageDef damageDef, out bool deflectedByMetalArmor, out bool diminishedByMetalArmor)
	{
		deflectedByMetalArmor = false;
		diminishedByMetalArmor = false;
		if (damageDef.armorCategory == null)
		{
			return amount;
		}
		StatDef armorRatingStat = damageDef.armorCategory.armorRatingStat;
		if (pawn.apparel != null)
		{
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				Apparel apparel = wornApparel[num];
				if (apparel.def.apparel.CoversBodyPart(part))
				{
					float num2 = amount;
					ApplyArmor(ref amount, armorPenetration, apparel.GetStatValue(armorRatingStat), apparel, ref damageDef, pawn, out var metalArmor);
					if (amount < 0.001f)
					{
						deflectedByMetalArmor = metalArmor;
						return 0f;
					}
					if (amount < num2 && metalArmor)
					{
						diminishedByMetalArmor = true;
					}
				}
			}
		}
		float num3 = amount;
		ApplyArmor(ref amount, armorPenetration, pawn.GetStatValue(armorRatingStat), null, ref damageDef, pawn, out var metalArmor2);
		if (amount < 0.001f)
		{
			deflectedByMetalArmor = metalArmor2;
			return 0f;
		}
		if (amount < num3 && metalArmor2)
		{
			diminishedByMetalArmor = true;
		}
		return amount;
	}

	private static void ApplyArmor(ref float damAmount, float armorPenetration, float armorRating, Thing armorThing, ref DamageDef damageDef, Pawn pawn, out bool metalArmor)
	{
		if (armorThing != null)
		{
			metalArmor = armorThing.def.apparel.useDeflectMetalEffect || (armorThing.Stuff != null && armorThing.Stuff.IsMetal);
		}
		else
		{
			metalArmor = pawn.RaceProps.IsMechanoid;
		}
		if (armorThing != null)
		{
			float f = damAmount * 0.25f;
			armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(f)));
		}
		float num = Mathf.Max(armorRating - armorPenetration, 0f);
		float value = Rand.Value;
		float num2 = num * 0.5f;
		float num3 = num;
		if (value < num2)
		{
			damAmount = 0f;
		}
		else if (value < num3)
		{
			damAmount = GenMath.RoundRandom(damAmount / 2f);
			if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
			{
				damageDef = DamageDefOf.Blunt;
			}
		}
	}
}
