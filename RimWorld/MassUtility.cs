using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MassUtility
	{
		public const float MassCapacityPerBodySize = 35f;

		public static float EncumbrancePercent(Pawn pawn)
		{
			return Mathf.Clamp01(UnboundedEncumbrancePercent(pawn));
		}

		public static float UnboundedEncumbrancePercent(Pawn pawn)
		{
			return GearAndInventoryMass(pawn) / Capacity(pawn);
		}

		public static bool IsOverEncumbered(Pawn pawn)
		{
			return UnboundedEncumbrancePercent(pawn) > 1f;
		}

		public static bool WillBeOverEncumberedAfterPickingUp(Pawn pawn, Thing thing, int count)
		{
			return FreeSpace(pawn) < (float)count * thing.GetStatValue(StatDefOf.Mass);
		}

		public static int CountToPickUpUntilOverEncumbered(Pawn pawn, Thing thing)
		{
			return Mathf.FloorToInt(FreeSpace(pawn) / thing.GetStatValue(StatDefOf.Mass));
		}

		public static float FreeSpace(Pawn pawn)
		{
			return Mathf.Max(Capacity(pawn) - GearAndInventoryMass(pawn), 0f);
		}

		public static float GearAndInventoryMass(Pawn pawn)
		{
			return GearMass(pawn) + InventoryMass(pawn);
		}

		public static float GearMass(Pawn p)
		{
			float num = 0f;
			if (p.apparel != null)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					num += wornApparel[i].GetStatValue(StatDefOf.Mass);
				}
			}
			if (p.equipment != null)
			{
				foreach (ThingWithComps item in p.equipment.AllEquipmentListForReading)
				{
					num += item.GetStatValue(StatDefOf.Mass);
				}
				return num;
			}
			return num;
		}

		public static float InventoryMass(Pawn p)
		{
			float num = 0f;
			for (int i = 0; i < p.inventory.innerContainer.Count; i++)
			{
				Thing thing = p.inventory.innerContainer[i];
				num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Mass);
			}
			return num;
		}

		public static float Capacity(Pawn p, StringBuilder explanation = null)
		{
			if (!CanEverCarryAnything(p))
			{
				return 0f;
			}
			float num = p.BodySize * 35f;
			if (explanation != null)
			{
				if (explanation.Length > 0)
				{
					explanation.AppendLine();
				}
				explanation.Append("  - " + p.LabelShortCap + ": " + num.ToStringMassOffset());
			}
			return num;
		}

		public static bool CanEverCarryAnything(Pawn p)
		{
			if (!p.RaceProps.ToolUser)
			{
				return p.RaceProps.packAnimal;
			}
			return true;
		}
	}
}
