using System.Collections.Generic;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompEquippableAbilityReloadable : CompEquippableAbility, IReloadableComp, ICompWithCharges
{
	private int replenishInTicks = -1;

	public CompProperties_EquippableAbilityReloadable Props => props as CompProperties_EquippableAbilityReloadable;

	public Thing ReloadableThing => parent;

	public ThingDef AmmoDef => Props.ammoDef;

	public int MaxCharges => Props.maxCharges;

	public int BaseReloadTicks => Props.baseReloadTicks;

	public string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

	public bool AbilityUsesCharges => base.AbilityForReading?.UsesCharges ?? false;

	public int RemainingCharges
	{
		get
		{
			return base.AbilityForReading?.RemainingCharges ?? 0;
		}
		set
		{
			if (base.AbilityForReading != null)
			{
				base.AbilityForReading.RemainingCharges = value;
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref replenishInTicks, "replenishInTicks", -1);
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		Notify_PropsChanged();
	}

	public void Notify_PropsChanged()
	{
		if (base.AbilityForReading != null)
		{
			base.AbilityForReading.maxCharges = MaxCharges;
			RemainingCharges = MaxCharges;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (base.AbilityForReading != null && Props.replenishAfterCooldown && RemainingCharges == 0)
		{
			if (replenishInTicks > 0)
			{
				replenishInTicks--;
			}
			else
			{
				RemainingCharges = MaxCharges;
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		if (base.AbilityForReading == null)
		{
			return null;
		}
		if (!AbilityUsesCharges)
		{
			return null;
		}
		return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
		if (enumerable != null)
		{
			foreach (StatDrawEntry item in enumerable)
			{
				yield return item;
			}
		}
		if (base.AbilityForReading != null && base.AbilityForReading.UsesCharges)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 5440);
		}
	}

	public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetEquippedGizmosExtra())
		{
			yield return item;
		}
		if (base.AbilityForReading != null && DebugSettings.ShowDevGizmos && NeedsReload(allowForcedReload: false))
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Reload to full";
			command_Action.action = delegate
			{
				RemainingCharges = MaxCharges;
			};
			yield return command_Action;
		}
	}

	public bool NeedsReload(bool allowForcedReload)
	{
		if (base.AbilityForReading == null)
		{
			return false;
		}
		if (Props.ammoDef == null)
		{
			return false;
		}
		if (Props.ammoCountToRefill != 0)
		{
			if (!allowForcedReload)
			{
				return RemainingCharges == 0;
			}
			return RemainingCharges != MaxCharges;
		}
		return RemainingCharges != MaxCharges;
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
			RemainingCharges = MaxCharges;
		}
		else
		{
			if (ammo.stackCount < Props.ammoCountPerCharge)
			{
				return;
			}
			int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, MaxCharges - RemainingCharges);
			ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
			RemainingCharges += num;
		}
		if (Props.soundReload != null)
		{
			Props.soundReload.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
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
		return Props.ammoCountPerCharge * (MaxCharges - RemainingCharges);
	}

	public int MaxAmmoAmount()
	{
		if (base.AbilityForReading == null)
		{
			return 0;
		}
		if (Props.ammoDef == null)
		{
			return 0;
		}
		if (Props.ammoCountToRefill == 0)
		{
			return Props.ammoCountPerCharge * MaxCharges;
		}
		return Props.ammoCountToRefill;
	}

	public override void UsedOnce()
	{
		if (base.AbilityForReading != null && Props.replenishAfterCooldown && RemainingCharges == 0)
		{
			replenishInTicks = Props.baseReloadTicks;
		}
	}

	public bool CanBeUsed(out string reason)
	{
		reason = null;
		if (base.AbilityForReading == null)
		{
			return false;
		}
		if (RemainingCharges <= 0)
		{
			reason = DisabledReason(MinAmmoNeeded(allowForcedReload: false), MaxAmmoNeeded(allowForcedReload: false));
			return false;
		}
		return true;
	}

	public string DisabledReason(int minNeeded, int maxNeeded)
	{
		if (base.AbilityForReading == null)
		{
			return null;
		}
		if (Props.replenishAfterCooldown)
		{
			return "CommandReload_Cooldown".Translate(Props.CooldownVerbArgument, replenishInTicks.ToStringTicksToPeriod().Named("TIME"));
		}
		if (Props.ammoDef == null)
		{
			return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
		}
		return TranslatorFormattedStringExtensions.Translate(arg3: ((Props.ammoCountToRefill == 0) ? ((minNeeded == maxNeeded) ? minNeeded.ToString() : $"{minNeeded}-{maxNeeded}") : Props.ammoCountToRefill.ToString()).Named("COUNT"), key: "CommandReload_NoAmmo", arg1: Props.ChargeNounArgument, arg2: NamedArgumentUtility.Named(Props.ammoDef, "AMMO"));
	}
}
