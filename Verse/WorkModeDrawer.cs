using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class WorkModeDrawer
	{
		private const float MouseoverLineWidth = 0.1f;

		private const float CircleOutlineRadius = 0.5f;

		private static readonly Vector3 IconScale = Vector3.one * 0.5f;

		public MechWorkModeDef def;

		private Material iconMat;

		protected virtual bool DrawIconAtTarget => true;

		public virtual void DrawControlGroupMouseOverExtra(MechanitorControlGroup group)
		{
			GlobalTargetInfo targetForLine = GetTargetForLine(group);
			List<Pawn> mechsForReading = group.MechsForReading;
			Map currentMap = Find.CurrentMap;
			if (!targetForLine.IsValid || targetForLine.Map != currentMap)
			{
				return;
			}
			Vector3 vector = targetForLine.Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead);
			for (int i = 0; i < mechsForReading.Count; i++)
			{
				if (mechsForReading[i].Map == currentMap)
				{
					GenDraw.DrawLineBetween(vector, mechsForReading[i].DrawPos, SimpleColor.White, 0.1f);
					GenDraw.DrawCircleOutline(mechsForReading[i].DrawPos, 0.5f);
				}
			}
			if (DrawIconAtTarget)
			{
				if (iconMat == null)
				{
					iconMat = MaterialPool.MatFrom(def.uiIcon);
				}
				Matrix4x4 matrix = Matrix4x4.TRS(vector, Quaternion.identity, IconScale);
				Graphics.DrawMesh(MeshPool.plane14, matrix, iconMat, 0);
			}
		}

		public virtual GlobalTargetInfo GetTargetForLine(MechanitorControlGroup group)
		{
			return group.Target;
		}
	}
}
