using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RitualVisualEffectDef : Def
	{
		public Type workerClass = typeof(RitualVisualEffect);

		public List<CompProperties_RitualVisualEffect> comps;

		public Color tintColor = Color.white;

		public RitualVisualEffect GetInstance()
		{
			RitualVisualEffect obj = (RitualVisualEffect)Activator.CreateInstance(workerClass);
			obj.def = this;
			return obj;
		}
	}
}
