using RimWorld.BaseGen;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterAncientLandingPad : GenStep_Scatterer
{
	private static readonly SimpleCurve SizeChanceCurve = new SimpleCurve
	{
		new CurvePoint(8f, 0f),
		new CurvePoint(12f, 4f),
		new CurvePoint(18f, 0f)
	};

	private const int Gap = 2;

	private int randomSize;

	public override int SeedPart => 1872954345;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter ancient landing"))
		{
			count = 1;
			allowInWaterBiome = false;
			randomSize = Mathf.RoundToInt(Rand.ByCurve(SizeChanceCurve));
			base.Generate(map, parms);
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		CellRect rect = CellRect.CenteredOn(loc, randomSize, randomSize);
		if (!CanPlaceAt(rect, map))
		{
			return false;
		}
		return true;
	}

	private bool CanPlaceAt(CellRect rect, Map map)
	{
		foreach (IntVec3 item in rect.ExpandedBy(2))
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(item);
			if (terrainDef.IsWater || terrainDef.IsRoad)
			{
				return false;
			}
			if (item.GetEdifice(map) != null)
			{
				return false;
			}
			if (!item.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		ResolveParams resolveParams = new ResolveParams
		{
			filthDensity = new FloatRange(0.025f, 0.05f),
			filthDef = ThingDefOf.Filth_Ash,
			rect = CellRect.CenteredOn(loc, randomSize, randomSize)
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("filth", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			landingPadSize = new IntVec2(randomSize, randomSize),
			sketch = new Sketch()
		}, root: SketchResolverDefOf.AncientLandingPad).Spawn(map, loc - new IntVec3(randomSize / 2, 0, randomSize / 2), null);
	}
}
