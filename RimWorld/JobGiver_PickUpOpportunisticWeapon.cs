using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PickUpOpportunisticWeapon : ThinkNode_JobGiver
	{
		private bool preferBuildingDestroyers;

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
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.equipment == null)
			{
				return null;
			}
			if (AlreadySatisfiedWithCurrentWeapon(pawn))
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
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn), 8f, (Thing x) => pawn.CanReserve(x) && !x.IsBurning() && ShouldEquip(x, pawn), null, 0, 15);
			if (thing != null)
			{
				return JobMaker.MakeJob(JobDefOf.Equip, thing);
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
			else if (!primary.def.IsRangedWeapon)
			{
				return false;
			}
			return true;
		}

		private bool ShouldEquip(Thing newWep, Pawn pawn)
		{
			if (EquipmentUtility.CanEquip(newWep, pawn))
			{
				return GetWeaponScore(newWep) > GetWeaponScore(pawn.equipment.Primary);
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
	}
}
