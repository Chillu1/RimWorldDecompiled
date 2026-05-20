using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_GravshipThruster : CompProperties_GravshipFacility
{
	public class ExhaustSettings
	{
		public bool enabled = true;

		private FleckDef exhaustFleckDef;

		public float emissionsPerSecond = 10f;

		public Vector3 spawnOffset = new Vector3(0f, 0f, 0f);

		public FloatRange spawnRadiusRange = new FloatRange(0f);

		public bool inheritGravshipRotation = true;

		public bool inheritThrusterRotation = true;

		public Vector3 velocity = new Vector3(0f, 0f, -1f);

		public FloatRange velocityRotationRange = new FloatRange(0f);

		public FloatRange velocityMultiplierRange = new FloatRange(1f);

		public FloatRange rotationOverTimeRange = new FloatRange(0f);

		public FloatRange scaleRange = new FloatRange(1f);

		public FleckDef ExhaustFleckDef => exhaustFleckDef ?? FleckDefOf.GravshipThrusterExhaust;
	}

	public IntVec3 exclusionAreaSize;

	public IntVec3 exclusionAreaOffset;

	public int directionInfluence;

	public List<Vector3> flameOffsetsPerDirection = new List<Vector3>
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	public float flameSize;

	private ShaderTypeDef flameShaderType;

	public List<ShaderParameter> flameShaderParameters = new List<ShaderParameter>();

	public ExhaustSettings exhaustSettings = new ExhaustSettings();

	public ShaderTypeDef FlameShaderType => flameShaderType ?? ShaderTypeDefOf.MoteGlow;

	public CompProperties_GravshipThruster()
	{
		compClass = typeof(CompGravshipThruster);
	}

	public void GetExclusionZone(IntVec3 pos, Rot4 rot, ref List<IntVec3> exclusionCells)
	{
		if (exclusionCells == null)
		{
			exclusionCells = new List<IntVec3>();
		}
		else
		{
			exclusionCells.Clear();
		}
		for (int i = 0; i < exclusionAreaSize.x; i++)
		{
			for (int j = 0; j < exclusionAreaSize.z; j++)
			{
				exclusionCells.Add(pos + new IntVec3(i, 0, j).RotatedBy(rot) + exclusionAreaOffset.RotatedBy(rot));
			}
		}
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (exhaustSettings != null && exhaustSettings.ExhaustFleckDef != null && exhaustSettings.ExhaustFleckDef.fleckSystemClass != null && exhaustSettings.ExhaustFleckDef.fleckSystemClass != typeof(FleckSystemThrown))
		{
			yield return "exhaust fleck '" + exhaustSettings.ExhaustFleckDef.defName + "' uses a fleckSystemClass other than FleckSystemThrown";
		}
	}
}
