using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_RitualVisualEffect
	{
		public Type compClass = typeof(RitualVisualEffectComp);

		public ThingDef moteDef;

		public FleckDef fleckDef;

		public EffecterDef effecterDef;

		public FloatRange velocity = FloatRange.Zero;

		public Vector3 velocityDir = Vector3.zero;

		public FloatRange rotation = FloatRange.Zero;

		public FloatRange rotationRate = FloatRange.Zero;

		public FloatRange scale = FloatRange.One;

		public Vector3 offset = Vector3.zero;

		public IntVec3 roomCheckOffset = IntVec3.Zero;

		public bool scaleWithRoom;

		public bool scalePositionWithRoom;

		public bool onlySpawnInSameRoom;

		public Color? colorOverride;

		public List<Color> overrideColors = new List<Color>();

		public CompProperties_RitualVisualEffect()
		{
		}

		public CompProperties_RitualVisualEffect(Type compClass)
		{
			this.compClass = compClass;
		}

		public RitualVisualEffectComp GetInstance()
		{
			return (RitualVisualEffectComp)Activator.CreateInstance(compClass);
		}
	}
}
