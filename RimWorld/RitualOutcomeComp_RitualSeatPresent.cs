using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_RitualSeatPresent : RitualOutcomeComp_BuildingsPresent
{
	protected override string LabelForDesc => "RitualSeat".Translate();

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		if (assignments.SpectatorsForReading.Count > 0)
		{
			return base.GetQualityFactor(ritual, ritualTarget, obligation, assignments, data);
		}
		return null;
	}

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (Find.IdeoManager.classicMode)
		{
			return false;
		}
		if (base.Applies(ritual) && ritual.assignments != null && ritual.assignments.SpectatorsForReading.Count > 0)
		{
			return ritual.Ritual.ideo.RitualSeatDef != null;
		}
		return false;
	}

	protected override int RequiredAmount(RitualRoleAssignments assignments)
	{
		if (assignments.Ritual.ideo.RitualSeatDef == null)
		{
			return 0;
		}
		int num = assignments.Ritual.ideo.RitualSeatDef.Size.x * assignments.Ritual.ideo.RitualSeatDef.Size.z;
		return Mathf.Max(1, Mathf.CeilToInt((float)assignments.SpectatorsForReading.Count / (float)num));
	}

	protected override string LabelForPredictedOutcomeDesc(Precept_Ritual ritual)
	{
		ThingDef ritualSeatDef = ritual.ideo.RitualSeatDef;
		if (ritualSeatDef == null)
		{
			return LabelForDesc;
		}
		return ritualSeatDef.LabelCap;
	}

	protected override Thing LookForBuilding(IntVec3 cell, Map map, Precept_Ritual ritual)
	{
		ThingDef ritualSeatDef = ritual.ideo.RitualSeatDef;
		if (ritualSeatDef == null)
		{
			return null;
		}
		Thing firstThing = cell.GetFirstThing(map, ritualSeatDef);
		if (firstThing != null)
		{
			return firstThing;
		}
		return null;
	}
}
