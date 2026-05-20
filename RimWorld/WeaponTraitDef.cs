using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class WeaponTraitDef : Def
{
	public Type workerClass = typeof(WeaponTraitWorker);

	public WeaponCategoryDef weaponCategory;

	public List<string> exclusionTags;

	public float commonality;

	public bool canGenerateAlone = true;

	public DamageDef damageDefOverride;

	public List<ExtraDamage> extraDamages;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;

	public List<StatModifier> equippedStatOffsets;

	public float marketValueOffset;

	public float burstShotSpeedMultiplier = 1f;

	public float burstShotCountMultiplier = 1f;

	public float additionalStoppingPower;

	public bool ignoresAccuracyMaluses;

	public ColorDef forcedColor;

	[MustTranslate]
	public List<string> traitAdjectives = new List<string>();

	public List<HediffDef> equippedHediffs;

	public List<HediffDef> bondedHediffs;

	public ThoughtDef bondedThought;

	public ThoughtDef killThought;

	public bool neverBond;

	public CompProperties_EquippableAbilityReloadable abilityProps;

	private WeaponTraitWorker worker;

	public WeaponTraitWorker Worker
	{
		get
		{
			if (!ModLister.CheckRoyaltyOrOdyssey("Weapon traits"))
			{
				return null;
			}
			if (worker == null)
			{
				worker = (WeaponTraitWorker)Activator.CreateInstance(workerClass);
				worker.def = this;
			}
			return worker;
		}
	}

	public bool Overlaps(WeaponTraitDef other)
	{
		if (other == this)
		{
			return true;
		}
		if (exclusionTags.NullOrEmpty() || other.exclusionTags.NullOrEmpty())
		{
			return false;
		}
		return exclusionTags.Any((string x) => other.exclusionTags.Contains(x));
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (!typeof(WeaponTraitWorker).IsAssignableFrom(workerClass))
		{
			yield return $"WeaponTraitDef {defName} has worker class {workerClass}, which is not deriving from {typeof(WeaponTraitWorker).FullName}";
		}
		if (commonality <= 0f)
		{
			yield return $"WeaponTraitDef {defName} has a commonality <= 0.";
		}
	}
}
