using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_AnimaTrees : GenStep_SpecialTrees
{
	public static readonly float Density = 1.25E-05f;

	private static readonly FloatRange GrowthRange = new FloatRange(0.5f, 0.75f);

	public override int SeedPart => 647816171;

	public override int DesiredTreeCountForMap(Map map)
	{
		return Mathf.Max(Mathf.RoundToInt(Density * (float)map.Area), 1);
	}

	protected override float GetGrowth()
	{
		return GrowthRange.RandomInRange;
	}
}
