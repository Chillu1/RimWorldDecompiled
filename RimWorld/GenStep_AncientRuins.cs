using Verse;

namespace RimWorld;

public class GenStep_AncientRuins : GenStep_BaseRuins
{
	private LayoutDef layoutDef;

	private static readonly FloatRange BlastMarksPer10K = new FloatRange(2f, 6f);

	private static readonly FloatRange RubblePilesPer10K = new FloatRange(4f, 10f);

	private static readonly IntRange RubblePileCountRange = new IntRange(3, 8);

	private static readonly IntRange RubblePileDistanceRange = new IntRange(3, 8);

	public override int SeedPart => 964521;

	protected override LayoutDef LayoutDef => layoutDef;

	protected override int RegionSize => 45;

	protected override FloatRange DefaultMapFillPercentRange => new FloatRange(0.15f, 0.3f);

	protected override FloatRange MergeRange => new FloatRange(0.1f, 0.35f);

	protected override int MoveRangeLimit => 3;

	protected override int ContractLimit => 3;

	protected override int MinRegionSize => 14;

	protected override Faction Faction => Faction.OfAncientsHostile;

	public override void GenerateRuins(Map map, GenStepParams parms, FloatRange mapFillPercentRange)
	{
		if (ModLister.CheckOdyssey("Ancient Ruins"))
		{
			base.GenerateRuins(map, parms, mapFillPercentRange);
			MapGenUtility.SpawnExteriorLumps(map, ThingDefOf.RubblePile, RubblePilesPer10K, RubblePileCountRange, RubblePileDistanceRange);
			MapGenUtility.SpawnScatter(map, ThingDefOf.Filth_BlastMark, BlastMarksPer10K);
		}
	}
}
