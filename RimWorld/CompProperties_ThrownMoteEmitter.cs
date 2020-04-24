using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_ThrownMoteEmitter : CompProperties
	{
		public ThingDef mote;

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

		public CompProperties_ThrownMoteEmitter()
		{
			compClass = typeof(CompThrownMoteEmitter);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (mote == null)
			{
				yield return "CompThrownMoteEmitter must have a mote assigned.";
			}
		}
	}
}
