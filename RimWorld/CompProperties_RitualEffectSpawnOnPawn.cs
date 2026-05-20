using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_RitualEffectSpawnOnPawn : CompProperties_RitualEffectIntervalSpawn
	{
		public Vector3 westRotationOffset;

		public Vector3 eastRotationOffset;

		public Vector3 northRotationOffset;

		public Vector3 southRotationOffset;

		[NoTranslate]
		public string requiredTag;
	}
}
