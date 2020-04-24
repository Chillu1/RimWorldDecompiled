using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class DebugCellDrawer
	{
		private List<DebugCell> debugCells = new List<DebugCell>();

		private List<DebugLine> debugLines = new List<DebugLine>();

		private const int DefaultLifespanTicks = 50;

		public void FlashCell(IntVec3 c, float colorPct = 0f, string text = null, int duration = 50)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.colorPct = colorPct;
			debugCell.ticksLeft = duration;
			debugCells.Add(debugCell);
		}

		public void FlashCell(IntVec3 c, Material mat, string text = null, int duration = 50)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.customMat = mat;
			debugCell.ticksLeft = duration;
			debugCells.Add(debugCell);
		}

		public void FlashLine(IntVec3 a, IntVec3 b, int duration = 50, SimpleColor color = SimpleColor.White)
		{
			debugLines.Add(new DebugLine(a.ToVector3Shifted(), b.ToVector3Shifted(), duration, color));
		}

		public void DebugDrawerUpdate()
		{
			for (int i = 0; i < debugCells.Count; i++)
			{
				debugCells[i].Draw();
			}
			for (int j = 0; j < debugLines.Count; j++)
			{
				debugLines[j].Draw();
			}
		}

		public void DebugDrawerTick()
		{
			for (int num = debugCells.Count - 1; num >= 0; num--)
			{
				DebugCell debugCell = debugCells[num];
				debugCell.ticksLeft--;
				if (debugCell.ticksLeft <= 0)
				{
					debugCells.RemoveAt(num);
				}
			}
			debugLines.RemoveAll((DebugLine dl) => dl.Done);
		}

		public void DebugDrawerOnGUI()
		{
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleCenter;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				for (int i = 0; i < debugCells.Count; i++)
				{
					debugCells[i].OnGUI();
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}
}
