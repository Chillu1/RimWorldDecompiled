using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_PickUpOpportunisticWeapon : ThinkNode_JobGiver
{
	private bool preferBuildingDestroyers;

	private bool pickUpUtilityItems;

	private float MinMeleeWeaponDPSThreshold
	{
		get
		{
			List<Tool> tools = ThingDefOf.Human.tools;
			float num = 0f;
			for (int i = 0; i < tools.Count; i++)
			{
				if (tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.LeftHand || tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.RightHand)
				{
					num = tools[i].power / tools[i].cooldownTime;
					break;
				}
			}
			return num + 2f;
		}
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_PickUpOpportunisticWeapon obj = (JobGiver_PickUpOpportunisticWeapon)base.DeepCopy(resolve);
		obj.preferBuildingDestroyers = preferBuildingDestroyers;
		obj.pickUpUtilityItems = pickUpUtilityItems;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.equipment == null && pawn.apparel == null)
		{
			return null;
		}
		if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return null;
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return null;
		}
		if (pawn.GetRegion() == null)
		{
			return null;
		}
		if (pawn.equipment != null && !AlreadySatisfiedWithCurrentWeapon(pawn))
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn), 8f, (Thing x) => pawn.CanReserve(x) && !x.IsBurning() && ShouldEquipWeapon(x, pawn), null, 0, 15, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: false, lookInHaulSources: true);
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Equip, thing);
				job.expiryInterval = 60;
				return job;
			}
			if (pawn.equipment.Primary != null && !SlaveRebellionUtility.WeaponUsableInRebellion(pawn.equipment.Primary))
			{
				return JobMaker.MakeJob(JobDefOf.DropEquipment, pawn.equipment.Primary);
			}
		}
		Pawn_EquipmentTracker equipment = pawn.equipment;
		if (equipment != null && equipment.Primary?.def?.IsRangedWeapon == true)
		{
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (item.def == ThingDefOf.Apparel_ShieldBelt)
				{
					Job job2 = JobMaker.MakeJob(JobDefOf.RemoveApparel, item);
					job2.expiryInterval = 60;
					return job2;
				}
			}
		}
		if (pickUpUtilityItems && pawn.apparel != null && WouldPickupUtilityItem(pawn))
		{
			Thing thing2 = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Apparel), PathEndMode.OnCell, TraverseParms.For(pawn), 8f, (Thing x) => pawn.CanReserve(x) && !x.IsBurning() && ShouldEquipUtilityItem(x, pawn), null, 0, 15, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: false, lookInHaulSources: true);
			if (thing2 != null)
			{
				Job job3 = JobMaker.MakeJob(JobDefOf.Wear, thing2);
				job3.expiryInterval = 60;
				return job3;
			}
		}
		return null;
	}

	private bool AlreadySatisfiedWithCurrentWeapon(Pawn pawn)
	{
		ThingWithComps primary = pawn.equipment.Primary;
		if (primary == null)
		{
			return false;
		}
		if (preferBuildingDestroyers)
		{
			if (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
			{
				return false;
			}
		}
		else if (!primary.def.IsRangedWeapon || !SlaveRebellionUtility.WeaponUsableInRebellion(primary))
		{
			return false;
		}
		return true;
	}

	private bool ShouldEquipWeapon(Thing newWep, Pawn pawn)
	{
		if (newWep.def.IsRangedWeapon && pawn.WorkTagIsDisabled(WorkTags.Shooting))
		{
			return false;
		}
		if (EquipmentUtility.CanEquip(newWep, pawn) && GetWeaponScore(newWep) > GetWeaponScore(pawn.equipment.Primary))
		{
			return SlaveRebellionUtility.WeaponUsableInRebellion(newWep);
		}
		return false;
	}

	private int GetWeaponScore(Thing wep)
	{
		if (wep == null)
		{
			return 0;
		}
		if (wep.def.IsMeleeWeapon && wep.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS) < MinMeleeWeaponDPSThreshold)
		{
			return 0;
		}
		if (preferBuildingDestroyers && wep.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
		{
			return 3;
		}
		if (wep.def.IsRangedWeapon)
		{
			return 2;
		}
		return 1;
	}

	private bool WouldPickupUtilityItem(Pawn pawn)
	{
		if (pawn.equipment?.Primary != null)
		{
			return false;
		}
		if (pawn.apparel.FirstApparelVerb != null)
		{
			return false;
		}
		return true;
	}

	private bool ShouldEquipUtilityItem(Thing thing, Pawn pawn)
	{
		if (!(thing is Apparel apparel))
		{
			return false;
		}
		if (!apparel.def.apparel.ai_pickUpOpportunistically)
		{
			return false;
		}
		if (EquipmentUtility.CanEquip(apparel, pawn) && ApparelUtility.HasPartsToWear(pawn, apparel.def))
		{
			return !pawn.apparel.WouldReplaceLockedApparel(apparel);
		}
		return false;
	}
}
