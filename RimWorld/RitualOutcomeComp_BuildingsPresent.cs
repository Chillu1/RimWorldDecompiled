using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class RitualOutcomeComp_BuildingsPresent : RitualOutcomeComp_QualitySingleOffset
	{
		private static HashSet<Thing> tmpThingsAlreadyCounted = new HashSet<Thing>();

		protected virtual int RequiredAmount(RitualRoleAssignments assignments)
		{
			return 1;
		}

		protected virtual string LabelForPredictedOutcomeDesc(Precept_Ritual ritual)
		{
			return LabelForDesc;
		}

		protected abstract Thing LookForBuilding(IntVec3 cell, Map map, Precept_Ritual ritual);

		protected virtual int CountAvailable(Precept_Ritual ritual, TargetInfo ritualTarget)
		{
			if (ritualTarget == null || !ritualTarget.IsValid)
			{
				return 0;
			}
			try
			{
				bool usesLectern = ritual.behavior.def.UsesLectern;
				RitualPosition_Lectern lecternPos = ritual.behavior.def.FirstLecternPosition;
				List<Thing> allLecterns = (usesLectern ? ritualTarget.Map.listerThings.ThingsOfDef(ThingDefOf.Lectern) : null);
				int num = 0;
				if (GatheringsUtility.UseWholeRoomAsGatheringArea(ritualTarget.Cell, ritualTarget.Map))
				{
					foreach (IntVec3 cell in ritualTarget.Cell.GetRoom(ritualTarget.Map).Cells)
					{
						if (Check(cell))
						{
							num++;
						}
					}
				}
				else
				{
					foreach (IntVec3 item in CellRect.CenteredOn(ritualTarget.Cell, 18))
					{
						if (Check(item))
						{
							num++;
						}
					}
				}
				return num;
				bool Check(IntVec3 cell)
				{
					Thing thing = LookForBuilding(cell, ritualTarget.Map, ritual);
					if (thing != null && GatheringsUtility.InGatheringArea(cell, ritualTarget.Cell, ritualTarget.Map))
					{
						IntVec3? intVec = null;
						if (allLecterns != null)
						{
							foreach (Thing item2 in allLecterns)
							{
								if (GatheringsUtility.InGatheringArea(item2.Position, ritualTarget.Cell, ritualTarget.Map) && lecternPos.IsUsableThing(item2, ritualTarget.Cell, ritualTarget))
								{
									intVec = item2.Position;
									break;
								}
							}
						}
						if (!intVec.HasValue)
						{
							intVec = ritualTarget.Thing.Position;
						}
						if (thing.def.building == null || !thing.def.building.isSittable || SpectatorCellFinder.CorrectlyRotatedChairAt(thing.Position, thing.Map, CellRect.CenteredOn(intVec.Value, 1)))
						{
							return tmpThingsAlreadyCounted.Add(thing);
						}
					}
					return false;
				}
			}
			finally
			{
				tmpThingsAlreadyCounted.Clear();
			}
		}

		private float CalcQualityOffset(int available, int required)
		{
			return qualityOffset * Mathf.Clamp01((float)available / (float)required);
		}

		public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
		{
			return CalcQualityOffset(CountAvailable(ritual.Ritual, ritual.selectedTarget), RequiredAmount(ritual.assignments));
		}

		public override bool Applies(LordJob_Ritual ritual)
		{
			return CountAvailable(ritual.Ritual, ritual.selectedTarget) > 0;
		}

		public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
		{
			int num = RequiredAmount(assignments);
			if (num <= 0)
			{
				return null;
			}
			int num2 = CountAvailable(ritual, ritualTarget);
			string count = null;
			if (num != 1)
			{
				count = num2 + " / " + num;
			}
			float quality = CalcQualityOffset(num2, num);
			return new QualityFactor
			{
				label = LabelForPredictedOutcomeDesc(ritual),
				qualityChange = ExpectedOffsetDesc(num2 > 0, quality),
				present = (num2 > 0),
				quality = quality,
				positive = (num2 > 0),
				priority = 1f,
				count = count
			};
		}
	}
}
