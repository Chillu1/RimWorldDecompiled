using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EmptyRoomRounded : SymbolResolver
	{
		private List<IntVec3> wallCells = new List<IntVec3>();

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (rp.cornerRadius.HasValue)
			{
				int num = Mathf.Min(rp.rect.Width, rp.rect.Height);
				if (rp.cornerRadius * 2 > num)
				{
					return false;
				}
			}
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			ThingDef thingDef = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, notVeryFlammable: true);
			TerrainDef floorDef = rp.floorDef ?? BaseGenUtility.CorrespondingTerrainDef(thingDef, beautiful: true, rp.faction);
			int num = Mathf.Min(rp.rect.Width, rp.rect.Height);
			int num2 = rp.cornerRadius ?? (num / 4);
			if (num2 > num / 2)
			{
				num2 = Mathf.FloorToInt((float)num / 2f);
			}
			CellRect cellRect = new CellRect(rp.rect.minX + num2, rp.rect.minZ, rp.rect.Width - 2 * num2, rp.rect.Height);
			CellRect cellRect2 = new CellRect(rp.rect.minX, rp.rect.minZ + num2, num2, rp.rect.Height - 2 * num2);
			CellRect cellRect3 = new CellRect(rp.rect.maxX - num2 + 1, rp.rect.minZ + num2, num2, rp.rect.Height - 2 * num2);
			foreach (IntVec3 corner in rp.rect.Corners)
			{
				int newX = ((corner.x < rp.rect.CenterCell.x) ? 1 : (-1));
				int newZ = ((corner.z < rp.rect.CenterCell.z) ? 1 : (-1));
				IntVec3 intVec = new IntVec3(newX, 0, newZ);
				IntVec3 center = corner + intVec * num2;
				CellRect cellRect4 = new CellRect(Mathf.Min(center.x, corner.x), Mathf.Min(center.z, corner.z), num2, num2);
				foreach (IntVec3 item in GenRadial.RadialCellsAround(center, num2 - 1, num2))
				{
					if (cellRect4.Contains(item))
					{
						ResolveParams resolveParams = rp;
						resolveParams.wallStuff = thingDef;
						resolveParams.rect = CellRect.CenteredOn(item, 1, 1);
						BaseGen.symbolStack.Push("edgeWalls", resolveParams);
					}
				}
				foreach (IntVec3 item2 in GenRadial.RadialCellsAround(center, num2, useCenter: true))
				{
					if (cellRect4.Contains(item2))
					{
						ResolveParams resolveParams2 = rp;
						resolveParams2.rect = CellRect.CenteredOn(item2, 1, 1);
						resolveParams2.floorDef = floorDef;
						if (!rp.noRoof.HasValue || !rp.noRoof.Value)
						{
							BaseGen.symbolStack.Push("roof", resolveParams2);
						}
						BaseGen.symbolStack.Push("floor", resolveParams2);
						BaseGen.symbolStack.Push("clear", resolveParams2);
					}
				}
			}
			wallCells.Clear();
			if ((float)cellRect.Area > 0f)
			{
				wallCells.AddRange(cellRect.GetEdgeCells(Rot4.North));
				wallCells.AddRange(cellRect.GetEdgeCells(Rot4.South));
			}
			if ((float)cellRect2.Area > 0f)
			{
				wallCells.AddRange(cellRect2.GetEdgeCells(Rot4.West));
			}
			if ((float)cellRect3.Area > 0f)
			{
				wallCells.AddRange(cellRect3.GetEdgeCells(Rot4.East));
			}
			ResolveParams resolveParams3 = rp;
			foreach (IntVec3 wallCell in wallCells)
			{
				resolveParams3.wallStuff = thingDef;
				resolveParams3.rect = CellRect.CenteredOn(wallCell, 1, 1);
				BaseGen.symbolStack.Push("edgeWalls", resolveParams3);
			}
			ResolveParams resolveParams4 = rp;
			resolveParams4.floorDef = floorDef;
			CellRect[] array = new CellRect[3] { cellRect, cellRect2, cellRect3 };
			for (int i = 0; i < array.Length; i++)
			{
				CellRect rect = array[i];
				if (!((float)rect.Area <= 0f))
				{
					resolveParams4.rect = rect;
					if (!rp.noRoof.HasValue || !rp.noRoof.Value)
					{
						BaseGen.symbolStack.Push("roof", resolveParams4);
					}
					BaseGen.symbolStack.Push("floor", resolveParams4);
					BaseGen.symbolStack.Push("clear", resolveParams4);
				}
			}
			wallCells.Clear();
		}
	}
}
