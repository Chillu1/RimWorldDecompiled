using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompApparelReloadable : CompApparelVerbOwner_Charged, IReloadableComp, ICompWithCharges
{
	private int replenishInTicks = -1;

	public new CompProperties_ApparelReloadable Props => props as CompProperties_ApparelReloadable;

	public Thing ReloadableThing => parent;

	public ThingDef AmmoDef => Props.ammoDef;

	public int BaseReloadTicks => Props.baseReloadTicks;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref replenishInTicks, "replenishInTicks", -1);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Props.replenishAfterCooldown && base.RemainingCharges == 0)
		{
			if (replenishInTicks > 0)
			{
				replenishInTicks--;
			}
			else
			{
				remainingCharges = base.MaxCharges;
			}
		}
	}

	public string DisabledReason(int minNeeded, int maxNeeded)
	{
		if (Props.replenishAfterCooldown)
		{
			return "CommandReload_Cooldown".Translate(Props.CooldownVerbArgument, replenishInTicks.ToStringTicksToPeriod().Named("TIME"));
		}
		if (AmmoDef == null)
		{
			return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
		}
		return TranslatorFormattedStringExtensions.Translate(arg3: ((Props.ammoCountToRefill == 0) ? ((minNeeded == maxNeeded) ? minNeeded.ToString() : $"{minNeeded}-{maxNeeded}") : Props.ammoCountToRefill.ToString()).Named("COUNT"), key: "CommandReload_NoAmmo", arg1: Props.ChargeNounArgument, arg2: NamedArgumentUtility.Named(AmmoDef, "AMMO"));
	}

	public bool NeedsReload(bool allowForcedReload)
	{
		if (AmmoDef == null)
		{
			return false;
		}
		if (Props.ammoCountToRefill != 0)
		{
			if (!allowForcedReload)
			{
				return remainingCharges == 0;
			}
			return base.RemainingCharges != base.MaxCharges;
		}
		return base.RemainingCharges != base.MaxCharges;
	}

	public void ReloadFrom(Thing ammo)
	{
		if (!NeedsReload(allowForcedReload: true))
		{
			return;
		}
		if (Props.ammoCountToRefill != 0)
		{
			if (ammo.stackCount < Props.ammoCountToRefill)
			{
				return;
			}
			ammo.SplitOff(Props.ammoCountToRefill).Destroy();
			remainingCharges = base.MaxCharges;
		}
		else
		{
			if (ammo.stackCount < Props.ammoCountPerCharge)
			{
				return;
			}
			int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, base.MaxCharges - base.RemainingCharges);
			ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
			remainingCharges += num;
		}
		if (Props.soundReload != null)
		{
			Props.soundReload.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
		}
	}

	public int MinAmmoNeeded(bool allowForcedReload)
	{
		if (!NeedsReload(allowForcedReload))
		{
			return 0;
		}
		if (Props.ammoCountToRefill != 0)
		{
			return Props.ammoCountToRefill;
		}
		return Props.ammoCountPerCharge;
	}

	public int MaxAmmoNeeded(bool allowForcedReload)
	{
		if (!NeedsReload(allowForcedReload))
		{
			return 0;
		}
		if (Props.ammoCountToRefill != 0)
		{
			return Props.ammoCountToRefill;
		}
		return Props.ammoCountPerCharge * (base.MaxCharges - base.RemainingCharges);
	}

	public int MaxAmmoAmount()
	{
		if (AmmoDef == null)
		{
			return 0;
		}
		if (Props.ammoCountToRefill == 0)
		{
			return Props.ammoCountPerCharge * base.MaxCharges;
		}
		return Props.ammoCountToRefill;
	}

	public override void UsedOnce()
	{
		base.UsedOnce();
		if (Props.replenishAfterCooldown && remainingCharges == 0)
		{
			replenishInTicks = Props.baseReloadTicks;
		}
	}

	public override bool CanBeUsed(out string reason)
	{
		reason = "";
		if (remainingCharges <= 0)
		{
			reason = DisabledReason(MinAmmoNeeded(allowForcedReload: false), MaxAmmoNeeded(allowForcedReload: false));
			return false;
		}
		if (!base.CanBeUsed(out reason))
		{
			return false;
		}
		return true;
	}
}
