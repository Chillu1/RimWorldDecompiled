using UnityEngine;
using Verse;

namespace RimWorld;

public class CompGasOnDamage : ThingComp
{
	private CompProperties_GasOnDamage Props => (CompProperties_GasOnDamage)props;

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		base.PostPreApplyDamage(ref dinfo, out absorbed);
		if (dinfo.Def.ExternalViolenceFor(parent))
		{
			float num = Mathf.Min(dinfo.Amount, parent.HitPoints) * Props.damageFactor;
			if (Props.useStackCountAsFactor)
			{
				num *= (float)parent.stackCount;
			}
			GasUtility.AddGas(parent.PositionHeld, parent.MapHeld, Props.type, (int)num);
		}
	}
}
