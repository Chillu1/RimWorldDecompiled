using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class StatPart_WeaponTraitsMarketValueOffset : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (!req.HasThing)
		{
			return;
		}
		CompBladelinkWeapon compBladelinkWeapon = req.Thing.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon == null)
		{
			return;
		}
		List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
		if (!traitsListForReading.NullOrEmpty())
		{
			for (int i = 0; i < traitsListForReading.Count; i++)
			{
				val += traitsListForReading[i].marketValueOffset;
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing)
		{
			CompBladelinkWeapon compBladelinkWeapon = req.Thing.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
				if (!traitsListForReading.NullOrEmpty())
				{
					for (int i = 0; i < traitsListForReading.Count; i++)
					{
						if (traitsListForReading[i].marketValueOffset != 0f)
						{
							stringBuilder.AppendLine(traitsListForReading[i].LabelCap + ": " + traitsListForReading[i].marketValueOffset.ToStringByStyle(ToStringStyle.Money, ToStringNumberSense.Offset));
						}
					}
				}
				return stringBuilder.ToString();
			}
		}
		return null;
	}
}
