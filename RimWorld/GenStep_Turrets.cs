using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_Turrets : GenStep
{
	public IntRange defaultTurretsCountRange = new IntRange(4, 5);

	public IntRange defaultMortarsCountRange = new IntRange(0, 1);

	public IntRange widthRange = new IntRange(3, 4);

	public IntRange guardsCountRange = new IntRange(1, 1);

	private const int Padding = 7;

	public const int DefaultGuardsCount = 1;

	public override int SeedPart => 895502705;

	public override void Generate(Map map, GenStepParams parms)
	{
		int num = 0;
		if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var var))
		{
			var = FindRandomRectToDefend(map);
		}
		else
		{
			if (!MapGenerator.TryGetVar<int>("RectOfInterestTurretsGenStepsCount", out var var2))
			{
				var2 = 0;
			}
			num += var2 * 4;
			var2++;
			MapGenerator.SetVar("RectOfInterestTurretsGenStepsCount", var2);
		}
		Faction faction = ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.AllFactions.Where((Faction x) => !x.defeated && x.HostileTo(Faction.OfPlayer) && !x.Hidden && (int)x.def.techLevel >= 4 && !x.temporary).RandomElementWithFallback(Find.FactionManager.RandomEnemyFaction()));
		int randomInRange = widthRange.RandomInRange;
		CellRect rect = var.ExpandedBy(7 + randomInRange + num).ClipInsideMap(map);
		int value;
		int value2;
		if (parms.sitePart != null)
		{
			value = parms.sitePart.parms.turretsCount;
			value2 = parms.sitePart.parms.mortarsCount;
		}
		else
		{
			value = defaultTurretsCountRange.RandomInRange;
			value2 = defaultMortarsCountRange.RandomInRange;
		}
		ResolveParams resolveParams = new ResolveParams
		{
			rect = rect,
			faction = faction,
			edgeDefenseWidth = randomInRange,
			edgeDefenseTurretsCount = value,
			edgeDefenseMortarsCount = value2,
			edgeDefenseGuardsCount = guardsCountRange.RandomInRange,
			edgeThingMustReachMapEdge = true
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("edgeDefense", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		ResolveParams resolveParams2 = new ResolveParams
		{
			rect = rect,
			faction = faction
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("outdoorLighting", resolveParams2);
		RimWorld.BaseGen.BaseGen.Generate();
	}

	private CellRect FindRandomRectToDefend(Map map)
	{
		if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var usedRects))
		{
			usedRects = null;
		}
		int rectRadius = Mathf.Max(Mathf.RoundToInt((float)Mathf.Min(map.Size.x, map.Size.z) * 0.07f), 1);
		TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors);
		if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate(IntVec3 x)
		{
			if (!map.reachability.CanReachMapEdge(x, traverseParams))
			{
				return false;
			}
			CellRect cellRect = CellRect.CenteredOn(x, rectRadius);
			int num = 0;
			foreach (IntVec3 c in cellRect)
			{
				if (!c.InBounds(map))
				{
					return false;
				}
				if (usedRects != null && cellRect.IsOnEdge(c) && usedRects.Any((CellRect y) => y.Contains(c)))
				{
					return false;
				}
				if (c.Standable(map) || c.GetPlant(map) != null)
				{
					num++;
				}
			}
			return (float)num / (float)cellRect.Area >= 0.6f;
		}, map, out var result))
		{
			return CellRect.CenteredOn(result, rectRadius);
		}
		if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map), map, out result))
		{
			return CellRect.CenteredOn(result, rectRadius);
		}
		return CellRect.CenteredOn(CellFinder.RandomCell(map), rectRadius).ClipInsideMap(map);
	}
}
