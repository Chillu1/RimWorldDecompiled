using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_MoteEmitter : CompProperties
	{
		public ThingDef mote;

		public Vector3 offset;

		public int emissionInterval = -1;

		public bool maintain;

		public string saveKeysPrefix;

		public CompProperties_MoteEmitter()
		{
			compClass = typeof(CompMoteEmitter);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (mote == null)
			{
				yield return "CompMoteEmitter must have a mote assigned.";
			}
		}
	}
}
