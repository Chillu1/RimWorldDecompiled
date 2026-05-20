using System.Collections.Generic;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
	public class StencilDrawerForCells : IExposable
	{
		public Lord sourceLord;

		public List<IntVec3> cells;

		public Vector3 center;

		public IntVec2 dimensionsIfNoCells;

		public int ticksLeftWithoutLord;

		public void Draw()
		{
			if (cells.NullOrEmpty())
			{
				GenDraw.DrawStencilCell(center, GenDraw.RitualStencilMat, dimensionsIfNoCells.x, dimensionsIfNoCells.z);
				return;
			}
			foreach (IntVec3 cell in cells)
			{
				GenDraw.DrawStencilCell(cell.ToVector3Shifted(), GenDraw.RitualStencilMat);
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref sourceLord, "sourceLord");
			Scribe_Collections.Look(ref cells, "cells", LookMode.Value);
			Scribe_Values.Look(ref center, "center");
			Scribe_Values.Look(ref dimensionsIfNoCells, "dimensionsIfNoCells");
			Scribe_Values.Look(ref ticksLeftWithoutLord, "ticksLeftWithoutLord", 0);
		}
	}
}
