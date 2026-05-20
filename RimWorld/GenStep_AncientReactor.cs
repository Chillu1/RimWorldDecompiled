using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class GenStep_AncientReactor : GenStep_LargeRuins
{
	private bool placedReactorLayout;

	private static readonly SimpleCurve SentryCountFromPointsCurve = new SimpleCurve(new CurvePoint[4]
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(100f, 2f),
		new CurvePoint(1000f, 10f),
		new CurvePoint(5000f, 20f)
	});

	public override int SeedPart => 79884699;

	protected override int RegionSize => 45;

	protected override FloatRange DefaultMapFillPercentRange => new FloatRange(0.6f, 0.75f);

	protected override FloatRange MergeRange => new FloatRange(1f, 1f);

	protected override int MoveRangeLimit => 6;

	protected override int ContractLimit => 6;

	protected override int MinRegionSize => 15;

	protected override IntRange RuinsMinMaxRange => new IntRange(2, 6);

	protected override LayoutDef LayoutDef => LayoutDefOf.AncientRuinsReactor_Standard;

	protected override Faction Faction => Faction.OfAncientsHostile;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckOdyssey("Ancient Reactor"))
		{
			placedReactorLayout = false;
			base.Generate(map, parms);
		}
	}

	protected override LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
	{
		if (!placedReactorLayout)
		{
			placedReactorLayout = true;
			layoutDef = LayoutDefOf.AncientRuinsReactor_Reactor;
			MapGenerator.SetVar("SpawnRect", rect.ExpandedBy(1));
		}
		return base.GenerateAndSpawn(rect, map, parms, layoutDef);
	}

	public override void PostMapInitialized(Map map, GenStepParams parms)
	{
		BaseGenUtility.ScatterSentryDronesInMap(SentryCountFromPointsCurve, map, Faction.OfAncientsHostile, parms);
	}
}
