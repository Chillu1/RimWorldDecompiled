using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class VerbDefsHardcodedNative
	{
		public static IEnumerable<VerbProperties> AllVerbDefs()
		{
			VerbProperties verbProperties = new VerbProperties();
			verbProperties.verbClass = typeof(Verb_BeatFire);
			verbProperties.category = VerbCategory.BeatFire;
			verbProperties.range = 1.42f;
			verbProperties.noiseRadius = 3f;
			verbProperties.targetParams.canTargetFires = true;
			verbProperties.targetParams.canTargetPawns = false;
			verbProperties.targetParams.canTargetBuildings = false;
			verbProperties.targetParams.mapObjectTargetsMustBeAutoAttackable = false;
			verbProperties.warmupTime = 0f;
			verbProperties.defaultCooldownTime = 1.1f;
			verbProperties.soundCast = SoundDefOf.Interact_BeatFire;
			yield return verbProperties;
			verbProperties = new VerbProperties();
			verbProperties.verbClass = typeof(Verb_Ignite);
			verbProperties.category = VerbCategory.Ignite;
			verbProperties.range = 1.42f;
			verbProperties.noiseRadius = 3f;
			verbProperties.targetParams.onlyTargetFlammables = true;
			verbProperties.targetParams.canTargetBuildings = true;
			verbProperties.targetParams.canTargetPawns = false;
			verbProperties.targetParams.mapObjectTargetsMustBeAutoAttackable = false;
			verbProperties.warmupTime = 3f;
			verbProperties.defaultCooldownTime = 1.3f;
			verbProperties.soundCast = SoundDefOf.Interact_Ignite;
			yield return verbProperties;
		}
	}
}
