using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ShipJobDef : Def
	{
		public Type jobClass;

		public bool blocksDisposalIfQueuedUnspawned = true;

		public override IEnumerable<string> ConfigErrors()
		{
			IEnumerable<string> enumerable = base.ConfigErrors();
			foreach (string item in enumerable)
			{
				yield return item;
			}
			if (!typeof(ShipJob).IsAssignableFrom(jobClass))
			{
				yield return jobClass.Name + " does not inherit from ShipJob";
			}
		}
	}
}
