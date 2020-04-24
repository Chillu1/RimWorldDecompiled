using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_EmptySpace : Designator
	{
		public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth)
		{
			return new GizmoResult(GizmoState.Clear);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			throw new NotImplementedException();
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			throw new NotImplementedException();
		}
	}
}
