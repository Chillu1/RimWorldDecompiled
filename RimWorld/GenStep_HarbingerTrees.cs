using Verse;

namespace RimWorld;

public class GenStep_HarbingerTrees : GenStep_SpecialTrees
{
	private const float Growth = 1f;

	public override int SeedPart => 786491238;

	protected override float GetGrowth()
	{
		return 1f;
	}

	public override int DesiredTreeCountForMap(Map map)
	{
		return Find.Anomaly.LevelDef.desiredHarbingerTreeCount;
	}
}
