using RimWorld;
using UnityEngine;

namespace Verse;

public struct DamageInfo
{
	public enum SourceCategory
	{
		ThingOrUnknown,
		Collapse
	}

	private DamageDef defInt;

	private float amountInt;

	private float armorPenetrationInt;

	private float angleInt;

	private Thing instigatorInt;

	private SourceCategory categoryInt;

	public Thing intendedTargetInt;

	private bool ignoreArmorInt;

	private bool ignoreInstantKillProtectionInt;

	private BodyPartRecord hitPartInt;

	private BodyPartHeight heightInt;

	private BodyPartDepth depthInt;

	private ThingDef weaponInt;

	private BodyPartGroupDef weaponBodyPartGroupInt;

	private HediffDef weaponHediffInt;

	private Tool tool;

	private QualityCategory weaponQuality;

	private bool instantPermanentInjuryInt;

	private bool allowDamagePropagationInt;

	private bool preventCascadeInt;

	private bool instigatorGuilty;

	private bool spawnFilth;

	private bool checkForJobOverride;

	private bool applyAllDamage;

	private IntRange damagePropagationPartsRange;

	public DamageDef Def
	{
		get
		{
			return defInt;
		}
		set
		{
			defInt = value;
		}
	}

	public float Amount
	{
		get
		{
			if (!DebugSettings.enableDamage)
			{
				return 0f;
			}
			return amountInt;
		}
	}

	public float ArmorPenetrationInt => armorPenetrationInt;

	public Thing Instigator => instigatorInt;

	public SourceCategory Category => categoryInt;

	public Thing IntendedTarget => intendedTargetInt;

	public float Angle => angleInt;

	public BodyPartRecord HitPart => hitPartInt;

	public BodyPartHeight Height => heightInt;

	public BodyPartDepth Depth => depthInt;

	public ThingDef Weapon => weaponInt;

	public BodyPartGroupDef WeaponBodyPartGroup => weaponBodyPartGroupInt;

	public HediffDef WeaponLinkedHediff => weaponHediffInt;

	public Tool Tool => tool;

	public bool InstantPermanentInjury => instantPermanentInjuryInt;

	public bool InstigatorGuilty => instigatorGuilty;

	public bool SpawnFilth => spawnFilth;

	public bool CheckForJobOverride => checkForJobOverride;

	public QualityCategory WeaponQuality => weaponQuality;

	public IntRange DamagePropagationPartsRange => damagePropagationPartsRange;

	public bool AllowDamagePropagation
	{
		get
		{
			if (InstantPermanentInjury)
			{
				return false;
			}
			return allowDamagePropagationInt;
		}
	}

	public bool PreventCascade => preventCascadeInt;

	public bool ApplyAllDamage => applyAllDamage;

	public bool IgnoreArmor => ignoreArmorInt;

	public bool IgnoreInstantKillProtection => ignoreInstantKillProtectionInt;

	public DamageInfo(DamageDef def, float amount, float armorPenetration = 0f, float angle = -1f, Thing instigator = null, BodyPartRecord hitPart = null, ThingDef weapon = null, SourceCategory category = SourceCategory.ThingOrUnknown, Thing intendedTarget = null, bool instigatorGuilty = true, bool spawnFilth = true, QualityCategory weaponQuality = QualityCategory.Normal, bool checkForJobOverride = true, bool preventCascade = false)
	{
		defInt = def;
		amountInt = amount;
		armorPenetrationInt = armorPenetration;
		if (angle < 0f)
		{
			angleInt = Rand.RangeInclusive(0, 359);
		}
		else
		{
			angleInt = angle;
		}
		instigatorInt = instigator;
		categoryInt = category;
		hitPartInt = hitPart;
		heightInt = BodyPartHeight.Undefined;
		depthInt = BodyPartDepth.Undefined;
		weaponInt = weapon;
		weaponBodyPartGroupInt = null;
		weaponHediffInt = null;
		tool = null;
		this.weaponQuality = weaponQuality;
		instantPermanentInjuryInt = false;
		allowDamagePropagationInt = true;
		preventCascadeInt = preventCascade;
		ignoreArmorInt = false;
		ignoreInstantKillProtectionInt = false;
		this.instigatorGuilty = instigatorGuilty;
		intendedTargetInt = intendedTarget;
		this.spawnFilth = spawnFilth;
		this.checkForJobOverride = checkForJobOverride;
		damagePropagationPartsRange = new IntRange(2, 4);
		applyAllDamage = false;
	}

	public DamageInfo(DamageInfo cloneSource)
	{
		defInt = cloneSource.defInt;
		amountInt = cloneSource.amountInt;
		armorPenetrationInt = cloneSource.armorPenetrationInt;
		angleInt = cloneSource.angleInt;
		instigatorInt = cloneSource.instigatorInt;
		categoryInt = cloneSource.categoryInt;
		hitPartInt = cloneSource.hitPartInt;
		heightInt = cloneSource.heightInt;
		depthInt = cloneSource.depthInt;
		weaponInt = cloneSource.weaponInt;
		weaponBodyPartGroupInt = cloneSource.weaponBodyPartGroupInt;
		weaponHediffInt = cloneSource.weaponHediffInt;
		tool = cloneSource.tool;
		weaponQuality = cloneSource.weaponQuality;
		instantPermanentInjuryInt = cloneSource.instantPermanentInjuryInt;
		allowDamagePropagationInt = cloneSource.allowDamagePropagationInt;
		preventCascadeInt = cloneSource.preventCascadeInt;
		intendedTargetInt = cloneSource.intendedTargetInt;
		ignoreArmorInt = cloneSource.ignoreArmorInt;
		ignoreInstantKillProtectionInt = cloneSource.ignoreInstantKillProtectionInt;
		instigatorGuilty = cloneSource.instigatorGuilty;
		spawnFilth = cloneSource.spawnFilth;
		checkForJobOverride = cloneSource.checkForJobOverride;
		damagePropagationPartsRange = cloneSource.damagePropagationPartsRange;
		applyAllDamage = cloneSource.applyAllDamage;
	}

	public void SetAmount(float newAmount)
	{
		amountInt = newAmount;
	}

	public void SetIgnoreArmor(bool ignoreArmor)
	{
		ignoreArmorInt = ignoreArmor;
	}

	public void SetIgnoreInstantKillProtection(bool ignore)
	{
		ignoreInstantKillProtectionInt = ignore;
	}

	public void SetBodyRegion(BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined)
	{
		heightInt = height;
		depthInt = depth;
	}

	public void SetHitPart(BodyPartRecord forceHitPart)
	{
		hitPartInt = forceHitPart;
	}

	public void SetInstantPermanentInjury(bool val)
	{
		instantPermanentInjuryInt = val;
	}

	public void SetWeaponBodyPartGroup(BodyPartGroupDef gr)
	{
		weaponBodyPartGroupInt = gr;
	}

	public void SetWeaponHediff(HediffDef hd)
	{
		weaponHediffInt = hd;
	}

	public void SetTool(Tool tool)
	{
		this.tool = tool;
	}

	public void SetWeaponQuality(QualityCategory weaponQuality)
	{
		this.weaponQuality = weaponQuality;
	}

	public void SetAllowDamagePropagation(bool val)
	{
		allowDamagePropagationInt = val;
	}

	public void SetAllowDamagePropagation(bool val, IntRange partsRange)
	{
		allowDamagePropagationInt = val;
		damagePropagationPartsRange = partsRange;
	}

	public void SetApplyAllDamage(bool value)
	{
		allowDamagePropagationInt = value;
		applyAllDamage = value;
	}

	public void SetAngle(Vector3 vec)
	{
		if (vec.x != 0f || vec.z != 0f)
		{
			angleInt = Quaternion.LookRotation(vec).eulerAngles.y;
		}
		else
		{
			angleInt = Rand.RangeInclusive(0, 359);
		}
	}

	public override string ToString()
	{
		return "(def=" + defInt?.ToString() + ", amount= " + amountInt + ", instigator=" + ((instigatorInt != null) ? instigatorInt.ToString() : categoryInt.ToString()) + ", angle=" + angleInt.ToString("F1") + ")";
	}
}
