using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class GenStep_BaseRuins : GenStep
	{
		public readonly List<LayoutStructureSketch> structureSketches = new List<LayoutStructureSketch>();

		protected virtual int RegionSize => 45;

		protected virtual FloatRange DefaultMapFillPercentRange => new FloatRange(0.15f, 0.3f);

		protected virtual FloatRange MergeRange => new FloatRange(0.1f, 0.35f);

		protected virtual int MoveRangeLimit => 3;

		protected virtual int ContractLimit => 3;

		protected virtual int MinRegionSize => 14;

		protected virtual bool UseUsedRects => true;

		protected virtual bool AvoidWaterRoads => true;

		protected virtual TerrainAffordanceDef MinAffordance => TerrainAffordanceDefOf.Heavy;

		protected abstract LayoutDef LayoutDef { get; }

		protected abstract Faction Faction { get; }

		protected virtual CellRect GetBounds(Map map)
		{
			return map.BoundsRect(16);
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			GenerateRuins(map, parms, DefaultMapFillPercentRange);
		}

		public virtual void GenerateRuins(Map map, GenStepParams parms, FloatRange mapFillPercentRange)
		{
			structureSketches.Clear();
			CellRect bounds = GetBounds(map);
			List<CellRect> list = GetRects(bounds, map).ToList();
			if (!list.Any())
			{
				Log.Warning($"{GetType().Name} did not find any valid rects to generate ruins in for bounds {bounds}.");
				return;
			}
			float num = (float)bounds.Area * mapFillPercentRange.RandomInRange;
			foreach (CellRect item in GetRectOrder(list, map))
			{
				if (num <= 0f)
				{
					break;
				}
				GenerateAndSpawn(item, map, parms, LayoutDef);
				num -= (float)item.Area;
			}
		}

		protected virtual IEnumerable<CellRect> GetRects(CellRect area, Map map)
		{
			List<CellRect> list = MapGenUtility.SubdivideRectIntoChunks(area, RegionSize, 1);
			MapGenUtility.TryFixInvalidRects(list, map, MinRegionSize, UseUsedRects, MinAffordance, AvoidWaterRoads);
			MergeRandomRects(list, map);
			OffsetContractRandomRects(list);
			MapGenUtility.RemoveInvalidRects(list, map, UseUsedRects, MinAffordance, AvoidWaterRoads);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!IsValidRect(list[num], map))
				{
					list.RemoveAt(num);
				}
			}
			return list;
		}

		protected virtual IEnumerable<CellRect> GetRectOrder(IEnumerable<CellRect> rects, Map map)
		{
			return rects.InRandomOrder();
		}

		protected virtual bool IsValidRect(CellRect rect, Map map)
		{
			if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var) && var.Any(((CellRect)rect).Overlaps))
			{
				return false;
			}
			foreach (IntVec3 cell in rect.Cells)
			{
				if (!MapGenUtility.IsCellValid(cell, map, UseUsedRects, MinAffordance, AvoidWaterRoads))
				{
					return false;
				}
			}
			return true;
		}

		private void OffsetContractRandomRects(List<CellRect> rects)
		{
			for (int i = 0; i < rects.Count; i++)
			{
				CellRect cellRect = rects[i];
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = cellRect.Width - MinRegionSize;
				int num6 = cellRect.Height - MinRegionSize;
				if (num5 > MinRegionSize)
				{
					num5 -= (num = Rand.Range(0, Mathf.Min(num5, MoveRangeLimit)));
					num3 = Rand.Range(0, Mathf.Min(num5, ContractLimit));
				}
				if (num6 > MinRegionSize)
				{
					num6 -= (num2 = Rand.Range(0, Mathf.Min(num6, MoveRangeLimit)));
					num4 = Rand.Range(0, Mathf.Min(num6, ContractLimit));
				}
				cellRect = cellRect.MovedBy(num, num2).ContractedBy(num + num3, num2 + num4);
				rects[i] = cellRect;
			}
		}

		private void MergeRandomRects(List<CellRect> rects, Map map)
		{
			int num = Mathf.RoundToInt(MergeRange.RandomInRange * (float)rects.Count);
			List<CellRect> merged = new List<CellRect>();
			for (int i = 0; i < num; i++)
			{
				if (rects.Count <= 2)
				{
					break;
				}
				List<CellRect> list = rects.Where((CellRect x) => !merged.Contains(x)).ToList();
				if (!list.Any())
				{
					break;
				}
				foreach (CellRect item in list.InRandomOrder())
				{
					bool flag = false;
					for (int num2 = 0; num2 < rects.Count; num2++)
					{
						CellRect cellRect = rects[num2];
						if (cellRect == item || merged.Contains(cellRect) || !cellRect.InlineWith(item))
						{
							continue;
						}
						CellRect cellRect2 = item.Encapsulate(cellRect);
						bool flag2 = true;
						if (!IsValidRect(cellRect2, map))
						{
							continue;
						}
						foreach (CellRect rect in rects)
						{
							if (!(rect == item) && !(rect == cellRect) && rect.Overlaps(cellRect2))
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							rects.Remove(cellRect);
							merged.Add(cellRect2);
							rects[rects.IndexOf(item)] = cellRect2;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}

		protected virtual StructureGenParams GetStructureGenParams(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
		{
			return new StructureGenParams
			{
				size = rect.Size
			};
		}

		protected virtual LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
		{
			StructureGenParams structureGenParams = GetStructureGenParams(rect, map, parms, layoutDef);
			LayoutWorker worker = layoutDef.Worker;
			LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(structureGenParams);
			using (new RandBlock(layoutStructureSketch.id))
			{
				map.layoutStructureSketches.Add(layoutStructureSketch);
				structureSketches.Add(layoutStructureSketch);
				float? threatPoints = null;
				if (parms.sitePart != null)
				{
					threatPoints = parms.sitePart.parms.points;
				}
				if (!threatPoints.HasValue && map.Parent is Site site)
				{
					threatPoints = site.ActualThreatPoints;
				}
				worker.Spawn(layoutStructureSketch, map, rect.Min, threatPoints, null, roofs: true, canReuseSketch: false, Faction);
				if (UseUsedRects)
				{
					MapGenerator.UsedRects.Add(rect.ExpandedBy(1));
				}
				return layoutStructureSketch;
			}
		}
	}
}
