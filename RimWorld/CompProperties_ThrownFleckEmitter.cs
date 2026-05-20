using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_ThrownFleckEmitter : CompProperties
	{
		public FleckDef fleck;

		public Vector3 offsetMin;

		public Vector3 offsetMax;

		public int emissionInterval = -1;

		public int burstCount = 1;

		public Color colorA = Color.white;

		public Color colorB = Color.white;

		public FloatRange scale;

		public FloatRange rotationRate;

		public FloatRange velocityX;

		public FloatRange velocityY;

		public CompProperties_ThrownFleckEmitter()
		{
			compClass = typeof(CompThrownFleckEmitter);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (fleck == null)
			{
				yield return "CompThrownFleckEmitter must have a fleck assigned.";
			}
		}
	}
}
