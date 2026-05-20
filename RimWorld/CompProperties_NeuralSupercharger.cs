using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_NeuralSupercharger : CompProperties_Rechargeable
	{
		[MustTranslate]
		public string jobString;

		public EffecterDef effectCharged;

		public CompProperties_NeuralSupercharger()
		{
			compClass = typeof(CompNeuralSupercharger);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (effectCharged != null && parentDef.tickerType != TickerType.Normal)
			{
				yield return $"CompProperties_NeuralSupercharger has effectCharged but parent {parentDef} has tickerType!=Normal";
			}
		}
	}
}
