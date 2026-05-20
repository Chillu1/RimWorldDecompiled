using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_WorkSite_Farming : SymbolResolver_WorkSite
{
	private static readonly SimpleCurve FieldRadiusThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(200f, 3f),
		new CurvePoint(500f, 5f),
		new CurvePoint(1000f, 7f)
	};

	private static readonly SimpleCurve FieldCountThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(1000f, 4f)
	};

	private const float MinDistField = 3f;

	private const float MaxDistField = 15f;

	public override void Resolve(ResolveParams rp)
	{
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		CellRect rect = rp.rect;
		float workSitePoints = rp.workSitePoints;
		int num = Mathf.FloorToInt(FieldRadiusThreatPointsCurve.Evaluate(workSitePoints));
		int num2 = Mathf.FloorToInt(FieldCountThreatPointsCurve.Evaluate(workSitePoints));
		Map map = BaseGen.globalSettings.map;
		float num3 = 3f + (float)num / 2f;
		float num4 = 15f + (float)num / 2f;
		ThingDef thingDef = (rp.stockpileConcreteContents.NullOrEmpty() ? DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.plant != null && def.plant.Sowable).RandomElement() : DefDatabase<ThingDef>.AllDefs.First((ThingDef def) => def.plant != null && def.plant.harvestedThingDef == rp.stockpileConcreteContents.First().def));
		List<CellRect> list = new List<CellRect>();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(rect.CenterCell, num4 + (float)Mathf.Max(rect.Width, rect.Height), useCenter: false))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			float num5 = Mathf.Sqrt(rect.ClosestDistSquaredTo(item));
			if (num5 < num3 || num5 > num4)
			{
				continue;
			}
			CellRect _r = CellRect.CenteredOn(item, num);
			if (list.Any((CellRect fieldRect) => fieldRect.Overlaps(_r)))
			{
				continue;
			}
			bool flag = false;
			foreach (IntVec3 item2 in _r)
			{
				if (!item2.InBounds(map) || item2.GetEdifice(map) != null || !thingDef.CanEverPlantAt(item2, map, canWipePlantsExceptTree: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BaseGen.symbolStack.Push("cultivatedPlants", new ResolveParams
				{
					rect = _r,
					cultivatedPlantDef = thingDef,
					fixedCulativedPlantGrowth = 0.25f
				});
				list.Add(_r);
				orGenerateVar.Add(_r);
				if (list.Count >= num2)
				{
					break;
				}
			}
		}
		base.Resolve(rp);
	}
}
