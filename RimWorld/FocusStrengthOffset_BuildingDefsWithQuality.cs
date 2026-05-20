using Verse;

namespace RimWorld;

public class FocusStrengthOffset_BuildingDefsWithQuality : FocusStrengthOffset_BuildingDefs
{
	public SimpleCurve focusPerQuality;

	protected override float OffsetForBuilding(Thing b)
	{
		if (b.TryGetQuality(out var qc))
		{
			return focusPerQuality.Evaluate((int)qc);
		}
		return 0f;
	}

	public override string GetExplanation(Thing parent)
	{
		if (!parent.Spawned)
		{
			return GetExplanationAbstract(parent.def);
		}
		int num = BuildingCount(parent);
		string text = focusPerQuality.Points[focusPerQuality.Points.Count - 1].y.ToString("0%");
		return explanationKey.Translate(num, maxBuildings, text) + ": " + GetOffset(parent).ToStringWithSign("0%");
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		string text = focusPerQuality.Points[focusPerQuality.Points.Count - 1].y.ToString("0%");
		return explanationKeyAbstract.Translate(maxBuildings, text) + ": +0-" + MaxOffset().ToString("0%");
	}

	public override float MaxOffset(Thing parent = null)
	{
		return (float)maxBuildings * focusPerQuality.Points[focusPerQuality.Points.Count - 1].y;
	}
}
