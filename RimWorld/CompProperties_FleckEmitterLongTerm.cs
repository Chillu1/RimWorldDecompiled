using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_FleckEmitterLongTerm : CompProperties
{
	public FleckDef fleckDef;

	public float spawnRadius;

	public float spawnChance;

	public Vector3 spawnOffsetFromCenter = Vector3.zero;

	public float prewarmCycles;

	public float fleckScale = 1f;

	public float minRotationRate;

	public float maxRotationRate;

	public float minVelocityAngle;

	public float maxVelocityAngle;

	public float minVelocitySpeed = 1f;

	public float maxVelocitySpeed = 1f;

	public bool forceEnabled;

	public CompProperties_FleckEmitterLongTerm()
	{
		compClass = typeof(CompFleckEmitterLongTerm);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (fleckDef == null)
		{
			yield return "CompLongTermFleckEmitter must have a fleck assigned.";
		}
	}
}
