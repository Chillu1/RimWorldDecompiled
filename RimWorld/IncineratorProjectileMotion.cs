using UnityEngine;
using Verse;

namespace RimWorld;

public struct IncineratorProjectileMotion
{
	public MoteDualAttached mote;

	public Vector3 worldSource;

	public Vector3 worldTarget;

	public IntVec3 targetDest;

	public Vector3 moveVector;

	public float startScale;

	public float endScale;

	public int ticks;

	public int lifespanTicks;

	public float Alpha => Mathf.Clamp01((float)ticks / (float)lifespanTicks);

	public Vector3 LerpdPos => Vector3.Lerp(worldSource, worldTarget, Alpha);

	public void Tick(Map map)
	{
		ticks++;
		Vector3 lerpdPos = LerpdPos;
		IntVec3 cell = lerpdPos.ToIntVec3();
		Vector3 offsetA = lerpdPos - cell.ToVector3Shifted();
		Vector3 vector = LerpdPos - moveVector * 2f;
		IntVec3 cell2 = vector.ToIntVec3();
		Vector3 offsetB = vector - cell2.ToVector3Shifted();
		mote.Scale = Mathf.Lerp(startScale, endScale, Alpha);
		mote.UpdateTargets(new TargetInfo(cell, map), new TargetInfo(cell2, map), offsetA, offsetB);
		if (Alpha < 1f)
		{
			mote.Maintain();
		}
	}
}
