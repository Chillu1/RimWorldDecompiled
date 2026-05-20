using Verse;

namespace RimWorld;

public class FocusStrengthOffset_NearbyGraves : FocusStrengthOffset_BuildingDefs
{
	public float focusPerFullGrave;

	protected override float OffsetForBuilding(Thing b)
	{
		float num = OffsetFor(b.def);
		if (b is Building_Grave { HasCorpse: not false } building_Grave && building_Grave.Corpse.InnerPawn.RaceProps.Humanlike)
		{
			num += focusPerFullGrave;
		}
		return num;
	}

	public override float MaxOffset(Thing parent = null)
	{
		return (float)maxBuildings * (focusPerFullGrave + base.MaxOffsetPerBuilding);
	}

	public override string GetExplanation(Thing parent)
	{
		if (!parent.Spawned)
		{
			return GetExplanationAbstract(parent.def);
		}
		int num = BuildingCount(parent);
		return explanationKey.Translate(num, maxBuildings, base.MaxOffsetPerBuilding.ToString("0%"), (base.MaxOffsetPerBuilding + focusPerFullGrave).ToString("0%")) + ": " + GetOffset(parent).ToStringWithSign("0%");
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return explanationKeyAbstract.Translate(maxBuildings, base.MaxOffsetPerBuilding.ToString("0%"), (base.MaxOffsetPerBuilding + focusPerFullGrave).ToString("0%")) + ": +0-" + MaxOffset().ToString("0%");
	}
}
