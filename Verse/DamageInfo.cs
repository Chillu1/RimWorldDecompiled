using UnityEngine;

namespace Verse
{
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

		private BodyPartRecord hitPartInt;

		private BodyPartHeight heightInt;

		private BodyPartDepth depthInt;

		private ThingDef weaponInt;

		private BodyPartGroupDef weaponBodyPartGroupInt;

		private HediffDef weaponHediffInt;

		private bool instantPermanentInjuryInt;

		private bool allowDamagePropagationInt;

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

		public bool InstantPermanentInjury => instantPermanentInjuryInt;

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

		public bool IgnoreArmor => ignoreArmorInt;

		public DamageInfo(DamageDef def, float amount, float armorPenetration = 0f, float angle = -1f, Thing instigator = null, BodyPartRecord hitPart = null, ThingDef weapon = null, SourceCategory category = SourceCategory.ThingOrUnknown, Thing intendedTarget = null)
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
			instantPermanentInjuryInt = false;
			allowDamagePropagationInt = true;
			ignoreArmorInt = false;
			intendedTargetInt = intendedTarget;
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
			instantPermanentInjuryInt = cloneSource.instantPermanentInjuryInt;
			allowDamagePropagationInt = cloneSource.allowDamagePropagationInt;
			intendedTargetInt = cloneSource.intendedTargetInt;
			ignoreArmorInt = cloneSource.ignoreArmorInt;
		}

		public void SetAmount(float newAmount)
		{
			amountInt = newAmount;
		}

		public void SetIgnoreArmor(bool ignoreArmor)
		{
			ignoreArmorInt = ignoreArmor;
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

		public void SetAllowDamagePropagation(bool val)
		{
			allowDamagePropagationInt = val;
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
			return "(def=" + defInt + ", amount= " + amountInt + ", instigator=" + ((instigatorInt != null) ? instigatorInt.ToString() : categoryInt.ToString()) + ", angle=" + angleInt.ToString("F1") + ")";
		}
	}
}
