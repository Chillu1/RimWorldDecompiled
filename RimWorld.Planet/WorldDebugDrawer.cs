using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDebugDrawer
	{
		private List<DebugTile> debugTiles = new List<DebugTile>();

		private List<DebugWorldLine> debugLines = new List<DebugWorldLine>();

		private const int DefaultLifespanTicks = 50;

		private const float MaxDistToCameraToDisplayLabel = 39f;

		public void FlashTile(int tile, float colorPct = 0f, string text = null, int duration = 50)
		{
			DebugTile debugTile = new DebugTile();
			debugTile.tile = tile;
			debugTile.displayString = text;
			debugTile.colorPct = colorPct;
			debugTile.ticksLeft = duration;
			debugTiles.Add(debugTile);
		}

		public void FlashTile(int tile, Material mat, string text = null, int duration = 50)
		{
			DebugTile debugTile = new DebugTile();
			debugTile.tile = tile;
			debugTile.displayString = text;
			debugTile.customMat = mat;
			debugTile.ticksLeft = duration;
			debugTiles.Add(debugTile);
		}

		public void FlashLine(Vector3 a, Vector3 b, bool onPlanetSurface = false, int duration = 50)
		{
			DebugWorldLine debugWorldLine = new DebugWorldLine(a, b, onPlanetSurface);
			debugWorldLine.TicksLeft = duration;
			debugLines.Add(debugWorldLine);
		}

		public void FlashLine(int tileA, int tileB, int duration = 50)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			Vector3 tileCenter = worldGrid.GetTileCenter(tileA);
			Vector3 tileCenter2 = worldGrid.GetTileCenter(tileB);
			DebugWorldLine debugWorldLine = new DebugWorldLine(tileCenter, tileCenter2, onPlanetSurface: true);
			debugWorldLine.TicksLeft = duration;
			debugLines.Add(debugWorldLine);
		}

		public void WorldDebugDrawerUpdate()
		{
			for (int i = 0; i < debugTiles.Count; i++)
			{
				debugTiles[i].Draw();
			}
			for (int j = 0; j < debugLines.Count; j++)
			{
				debugLines[j].Draw();
			}
		}

		public void WorldDebugDrawerTick()
		{
			for (int num = debugTiles.Count - 1; num >= 0; num--)
			{
				DebugTile debugTile = debugTiles[num];
				debugTile.ticksLeft--;
				if (debugTile.ticksLeft <= 0)
				{
					debugTiles.RemoveAt(num);
				}
			}
			for (int num2 = debugLines.Count - 1; num2 >= 0; num2--)
			{
				DebugWorldLine debugWorldLine = debugLines[num2];
				debugWorldLine.ticksLeft--;
				if (debugWorldLine.ticksLeft <= 0)
				{
					debugLines.RemoveAt(num2);
				}
			}
		}

		public void WorldDebugDrawerOnGUI()
		{
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			for (int i = 0; i < debugTiles.Count; i++)
			{
				if (!(debugTiles[i].DistanceToCamera > 39f))
				{
					debugTiles[i].OnGUI();
				}
			}
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
