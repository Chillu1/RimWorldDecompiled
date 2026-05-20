using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class DrawStyle_FilledOval : DrawStyle
{
	private const float RadiusOffset = 0.4f;

	public override bool CanHaveDuplicates => false;

	public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
	{
		CellRect cellRect = CellRect.FromLimits(origin, target);
		float radius = (float)cellRect.Width / 2f;
		float ratio = (float)cellRect.Width / (float)cellRect.Height;
		Vector3 vector = cellRect.CenterCell.ToVector3();
		if (cellRect.Width % 2 == 0)
		{
			vector.x -= 0.5f;
		}
		if (cellRect.Height % 2 == 0)
		{
			vector.z -= 0.5f;
		}
		foreach (IntVec3 cell in cellRect.Cells)
		{
			Vector3 vector2 = cell.ToVector3() - vector;
			if (Filled(vector2.x, vector2.z, radius, ratio))
			{
				buffer.Add(cell);
			}
		}
	}

	protected float DistanceSqr(float x, float z, float ratio)
	{
		return Mathf.Pow(z * ratio, 2f) + Mathf.Pow(x, 2f);
	}

	protected bool Inside(float x, float z, float radius, float ratio)
	{
		return DistanceSqr(x, z, ratio) <= (radius + 0.4f) * (radius + 0.4f);
	}

	protected virtual bool Filled(float x, float z, float radius, float ratio)
	{
		return Inside(x, z, radius, ratio);
	}
}
