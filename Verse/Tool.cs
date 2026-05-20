using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class Tool
{
	[Unsaved(false)]
	public string id;

	[MustTranslate]
	public string label;

	[MustTranslate]
	public string labelNoLocation;

	[Unsaved(false)]
	[TranslationHandle]
	public string untranslatedLabel;

	public bool labelUsedInLogging = true;

	public List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();

	public float power;

	public float armorPenetration = -1f;

	public float cooldownTime;

	public SurpriseAttackProps surpriseAttack;

	public HediffDef hediff;

	public float chanceFactor = 1f;

	public bool alwaysTreatAsWeapon;

	public List<ExtraDamage> extraMeleeDamages;

	public SoundDef soundMeleeHit;

	public SoundDef soundMeleeMiss;

	public BodyPartGroupDef linkedBodyPartsGroup;

	public bool ensureLinkedBodyPartsGroupAlwaysUsable;

	[Unsaved(false)]
	private string cachedLabelCap;

	public string LabelCap
	{
		get
		{
			if (cachedLabelCap == null)
			{
				cachedLabelCap = label.CapitalizeFirst();
			}
			return cachedLabelCap;
		}
	}

	public IEnumerable<ManeuverDef> Maneuvers => DefDatabase<ManeuverDef>.AllDefsListForReading.Where((ManeuverDef x) => capacities.Contains(x.requiredCapacity));

	public IEnumerable<VerbProperties> VerbsProperties => Maneuvers.Select((ManeuverDef x) => x.verb);

	public float AdjustedBaseMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
	{
		float num = power;
		if (ownerEquipment != null)
		{
			num *= ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier);
			if (ownerEquipment.Stuff != null && damageDef != null)
			{
				num *= ownerEquipment.Stuff.GetStatValueAbstract(damageDef.armorCategory.multStat);
			}
		}
		return num;
	}

	public float AdjustedBaseMeleeDamageAmount(ThingDef ownerEquipment, ThingDef ownerEquipmentStuff, DamageDef damageDef)
	{
		float num = power;
		if (ownerEquipmentStuff != null)
		{
			num *= ownerEquipment.GetStatValueAbstract(StatDefOf.MeleeWeapon_DamageMultiplier, ownerEquipmentStuff);
			if (ownerEquipmentStuff != null && damageDef != null)
			{
				num *= ownerEquipmentStuff.GetStatValueAbstract(damageDef.armorCategory.multStat);
			}
		}
		return num;
	}

	public float AdjustedCooldown(Thing ownerEquipment)
	{
		return cooldownTime * (ownerEquipment?.GetStatValue(StatDefOf.MeleeWeapon_CooldownMultiplier) ?? 1f);
	}

	public float AdjustedCooldown(ThingDef ownerEquipment, ThingDef ownerEquipmentStuff)
	{
		return cooldownTime * (ownerEquipment?.GetStatValueAbstract(StatDefOf.MeleeWeapon_CooldownMultiplier, ownerEquipmentStuff) ?? 1f);
	}

	public override string ToString()
	{
		return label;
	}

	public void PostLoad()
	{
		untranslatedLabel = label;
	}

	public IEnumerable<string> ConfigErrors()
	{
		if (id.NullOrEmpty())
		{
			yield return "tool has null id (power=" + power.ToString("0.##") + ")";
		}
	}
}
