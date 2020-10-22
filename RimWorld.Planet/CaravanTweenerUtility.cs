using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanTweenerUtility
	{
		private const float BaseRadius = 0.15f;

		private const float BaseDistToCollide = 0.2f;

		public static Vector3 PatherTweenedPosRoot(Caravan caravan)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			if (!caravan.Spawned)
			{
				return worldGrid.GetTileCenter(caravan.Tile);
			}
			if (caravan.pather.Moving)
			{
				float num = (caravan.pather.IsNextTilePassable() ? (1f - caravan.pather.nextTileCostLeft / caravan.pather.nextTileCostTotal) : 0f);
				int tileID = ((caravan.pather.nextTile != caravan.Tile || caravan.pather.previousTileForDrawingIfInDoubt == -1) ? caravan.Tile : caravan.pather.previousTileForDrawingIfInDoubt);
				return worldGrid.GetTileCenter(caravan.pather.nextTile) * num + worldGrid.GetTileCenter(tileID) * (1f - num);
			}
			return worldGrid.GetTileCenter(caravan.Tile);
		}

		public static Vector3 CaravanCollisionPosOffsetFor(Caravan caravan)
		{
			if (!caravan.Spawned)
			{
				return Vector3.zero;
			}
			bool flag = caravan.Spawned && caravan.pather.Moving;
			float d = 0.15f * Find.WorldGrid.averageTileSize;
			if (!flag || caravan.pather.nextTile == caravan.pather.Destination)
			{
				int num = ((!flag) ? caravan.Tile : caravan.pather.nextTile);
				int caravansCount = 0;
				int caravansWithLowerIdCount = 0;
				GetCaravansStandingAtOrAboutToStandAt(num, out caravansCount, out caravansWithLowerIdCount, caravan);
				if (caravansCount == 0)
				{
					return Vector3.zero;
				}
				return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Find.WorldGrid.GetTileCenter(num), GenGeo.RegularPolygonVertexPosition(caravansCount, caravansWithLowerIdCount) * d);
			}
			if (DrawPosCollides(caravan))
			{
				Rand.PushState();
				Rand.Seed = caravan.ID;
				float f = Rand.Range(0f, 360f);
				Rand.PopState();
				Vector2 point = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * d;
				return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(PatherTweenedPosRoot(caravan), point);
			}
			return Vector3.zero;
		}

		private static void GetCaravansStandingAtOrAboutToStandAt(int tile, out int caravansCount, out int caravansWithLowerIdCount, Caravan forCaravan)
		{
			caravansCount = 0;
			caravansWithLowerIdCount = 0;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (caravan.Tile != tile)
				{
					if (!caravan.pather.Moving || caravan.pather.nextTile != caravan.pather.Destination || caravan.pather.Destination != tile)
					{
						continue;
					}
				}
				else if (caravan.pather.Moving)
				{
					continue;
				}
				caravansCount++;
				if (caravan.ID < forCaravan.ID)
				{
					caravansWithLowerIdCount++;
				}
			}
		}

		private static bool DrawPosCollides(Caravan caravan)
		{
			Vector3 a = PatherTweenedPosRoot(caravan);
			float num = Find.WorldGrid.averageTileSize * 0.2f;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan2 = caravans[i];
				if (caravan2 != caravan && Vector3.Distance(a, PatherTweenedPosRoot(caravan2)) < num)
				{
					return true;
				}
			}
			return false;
		}
	}
}
