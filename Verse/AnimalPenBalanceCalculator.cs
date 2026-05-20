using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	public class AnimalPenBalanceCalculator
	{
		private struct AnimalMembershipInfo
		{
			public Pawn animal;

			public District pen;
		}

		private const float DensityTolerance = 0.2f;

		private readonly Map map;

		private bool considerInProgressMovement;

		private readonly List<AnimalMembershipInfo> membership = new List<AnimalMembershipInfo>();

		private bool dirty = true;

		public AnimalPenBalanceCalculator(Map map, bool considerInProgressMovement)
		{
			this.map = map;
			this.considerInProgressMovement = considerInProgressMovement;
		}

		public void MarkDirty()
		{
			dirty = true;
		}

		public bool IsBetterPen(CompAnimalPenMarker markerA, CompAnimalPenMarker markerB, bool leavingMarkerB, Pawn animal)
		{
			RecalculateIfDirty();
			District district = markerA.parent.GetDistrict();
			District district2 = markerB.parent.GetDistrict();
			if (district == district2)
			{
				return false;
			}
			float bodySize = animal.BodySize;
			float num = TotalBodySizeIn(district) + bodySize;
			float num2 = TotalBodySizeIn(district2) + (leavingMarkerB ? (0f - bodySize) : bodySize);
			float num3 = num / (float)district.CellCount;
			float num4 = num2 / (float)district2.CellCount;
			return num3 * 1.2f < num4;
		}

		public float TotalBodySizeIn(District district)
		{
			RecalculateIfDirty();
			float num = 0f;
			foreach (AnimalMembershipInfo item in membership)
			{
				if (item.pen == district)
				{
					num += item.animal.BodySize;
				}
			}
			return num;
		}

		private void RecalculateIfDirty()
		{
			if (!dirty)
			{
				return;
			}
			dirty = false;
			membership.Clear();
			foreach (Pawn item in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (!AnimalPenUtility.NeedsToBeManagedByRope(item))
				{
					continue;
				}
				District district = null;
				if (considerInProgressMovement && item.roping.IsRopedByPawn && item.roping.RopedByPawn.jobs.curDriver is JobDriver_RopeToPen)
				{
					District district2 = item.roping.RopedByPawn.CurJob.GetTarget(TargetIndex.C).Thing?.GetDistrict();
					if (district2 != null && !district2.TouchesMapEdge)
					{
						district = district2;
					}
				}
				if (district == null)
				{
					district = AnimalPenUtility.GetCurrentPenOf(item, allowUnenclosedPens: false)?.parent.GetDistrict();
				}
				membership.Add(new AnimalMembershipInfo
				{
					animal = item,
					pen = district
				});
			}
		}
	}
}
