using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_GetEnergy : ThinkNode_JobGiver
	{
		public const int DefaultEnergyLevelThreshold = 15;

		private const float MinAutoRechargeDelta = 0.1f;

		public bool forced;

		public static int GetMinAutorechargeThreshold(Pawn pawn)
		{
			int num = pawn.RaceProps.maxMechEnergy;
			MechanitorControlGroup mechControlGroup = pawn.GetMechControlGroup();
			if (UseGroupRechargeLimits(mechControlGroup))
			{
				num = Mathf.RoundToInt((float)num * mechControlGroup.mechRechargeThresholds.min);
			}
			return num;
		}

		public static float GetMaxRechargeLimit(Pawn pawn)
		{
			int num = pawn.RaceProps.maxMechEnergy;
			MechanitorControlGroup mechControlGroup = pawn.GetMechControlGroup();
			if (UseGroupRechargeLimits(mechControlGroup))
			{
				num = Mathf.RoundToInt((float)num * mechControlGroup.mechRechargeThresholds.max);
			}
			return num;
		}

		private static bool UseGroupRechargeLimits(MechanitorControlGroup controlGroup)
		{
			if (controlGroup != null && controlGroup.WorkMode != null)
			{
				return !controlGroup.WorkMode.ignoreGroupChargeLimits;
			}
			return false;
		}

		protected virtual bool ShouldAutoRecharge(Pawn pawn)
		{
			Need_MechEnergy energy = pawn.needs.energy;
			if (energy == null)
			{
				return false;
			}
			if (forced)
			{
				return true;
			}
			return energy.CurLevel + 0.1f < (float)GetMinAutorechargeThreshold(pawn);
		}

		public override float GetPriority(Pawn pawn)
		{
			if (!ShouldAutoRecharge(pawn))
			{
				return 0f;
			}
			return 9.5f;
		}
	}
}
