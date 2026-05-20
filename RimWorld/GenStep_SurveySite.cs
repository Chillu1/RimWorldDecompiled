using Verse;

namespace RimWorld;

public class GenStep_SurveySite : GenStep
{
	public override int SeedPart => 3615199;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		CellRect rect;
		IntVec3 intVec = (MapGenUtility.TryGetRandomClearRect(20, 20, out rect, -1, -1, RectValidator, -1f, 0.7f, MapGenUtility.SearchWeightMode.Center) ? (rect.CenterCell + IntVec3.SouthEast) : map.Center);
		PrefabDef surveyScanner = PrefabDefOf.SurveyScanner;
		CellRect rect2 = intVec.RectAbout(surveyScanner.size);
		MapGenerator.UsedRects.Add(rect2.ExpandedBy(8));
		foreach (IntVec3 cell in rect2.Cells)
		{
			cell.GetEdifice(map)?.Destroy();
		}
		MapGenUtility.DoRectEdgeLumps(map, null, ref rect2, IntRange.Between(2, 4), IntRange.Between(2, 1));
		PrefabUtility.SpawnPrefab(PrefabDefOf.SurveyScanner, map, intVec, Rot4.North, null, null, null, OnSpawned);
		static void OnSpawned(Thing thing)
		{
			if (thing.def == ThingDefOf.SurveyScanner)
			{
				thing.SetFaction(Find.FactionManager.OfPlayer);
			}
		}
		bool RectValidator(CellRect r)
		{
			foreach (IntVec3 cell2 in r.Cells)
			{
				if (cell2.GetEdifice(map) != null || !cell2.GetAffordances(map).Contains(TerrainAffordanceDefOf.Medium) || MapGenerator.UsedRects.Any((CellRect a) => a.Overlaps(r)))
				{
					return false;
				}
			}
			return true;
		}
	}
}
