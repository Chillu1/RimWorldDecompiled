using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_MechCarrier : CompProperties
	{
		public ThingDef fixedIngredient;

		public int costPerPawn;

		public int maxIngredientCount;

		public int startingIngredientCount;

		public PawnKindDef spawnPawnKind;

		public int cooldownTicks = 900;

		public int maxPawnsToSpawn = 3;

		public EffecterDef spawnEffecter;

		public EffecterDef spawnedMechEffecter;

		public bool attachSpawnedEffecter;

		public bool attachSpawnedMechEffecter;

		public CompProperties_MechCarrier()
		{
			compClass = typeof(CompMechCarrier);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
		}
	}
}
