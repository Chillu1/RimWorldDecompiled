using System.Collections.Generic;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetMonumentSketch : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<Map> useOnlyResourcesAvailableOnMap;

	public SlateRef<int?> maxSize;

	public SlateRef<float> pointsPerArea;

	public SlateRef<bool?> clearStuff;

	private static readonly FloatRange RandomAspectRatioRange = new FloatRange(1f, 3f);

	private const int MinEdgeLength = 3;

	private const int MaxArea = 2500;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		DoWork(QuestGen.slate);
	}

	private bool DoWork(Slate slate)
	{
		float num = slate.Get("points", 0f);
		float value = pointsPerArea.GetValue(slate);
		float num2 = Mathf.Min(num / value, 2500f);
		float randomInRange = RandomAspectRatioRange.RandomInRange;
		float f = Mathf.Sqrt(randomInRange * num2);
		float f2 = Mathf.Sqrt(num2 / randomInRange);
		int num3 = GenMath.RoundRandom(f);
		int num4 = GenMath.RoundRandom(f2);
		if (Rand.Bool)
		{
			int num5 = num3;
			num3 = num4;
			num4 = num5;
		}
		int? value2 = maxSize.GetValue(slate);
		if (value2.HasValue)
		{
			num3 = Mathf.Min(num3, value2.Value);
			num4 = Mathf.Min(num4, value2.Value);
		}
		num3 = Mathf.Max(num3, 3);
		num4 = Mathf.Max(num4, 3);
		IntVec2 value3 = new IntVec2(num3, num4);
		SketchResolveParams parms = new SketchResolveParams
		{
			sketch = new Sketch(),
			monumentSize = value3,
			useOnlyStonesAvailableOnMap = useOnlyResourcesAvailableOnMap.GetValue(slate),
			onlyBuildableByPlayer = true
		};
		if (useOnlyResourcesAvailableOnMap.GetValue(slate) != null)
		{
			parms.allowWood = useOnlyResourcesAvailableOnMap.GetValue(slate).Biome.TreeDensity >= BiomeDefOf.BorealForest.TreeDensity;
		}
		parms.allowedMonumentThings = new ThingFilter();
		parms.allowedMonumentThings.SetAllowAll(null, includeNonStorable: true);
		parms.allowedMonumentThings.SetAllow(ThingDefOf.Urn, allow: false);
		Sketch sketch = RimWorld.SketchGen.SketchGen.Generate(SketchResolverDefOf.Monument, parms);
		if (clearStuff.GetValue(slate) ?? true)
		{
			List<SketchThing> things = sketch.Things;
			for (int i = 0; i < things.Count; i++)
			{
				things[i].stuff = null;
			}
			List<SketchTerrain> terrain = sketch.Terrain;
			for (int j = 0; j < terrain.Count; j++)
			{
				terrain[j].treatSimilarAsSame = true;
			}
		}
		slate.Set(storeAs.GetValue(slate), sketch);
		return true;
	}
}
