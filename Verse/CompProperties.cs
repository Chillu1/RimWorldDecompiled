using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CompProperties
	{
		[TranslationHandle]
		public Type compClass = typeof(ThingComp);

		public CompProperties()
		{
		}

		public CompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		public virtual void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
		{
		}

		public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (compClass == null)
			{
				yield return parentDef.defName + " has CompProperties with null compClass.";
			}
		}

		public virtual void ResolveReferences(ThingDef parentDef)
		{
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			yield break;
		}

		public virtual void PostLoadSpecial(ThingDef parent)
		{
		}
	}
}
