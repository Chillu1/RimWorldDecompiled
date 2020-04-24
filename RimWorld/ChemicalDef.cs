using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChemicalDef : Def
	{
		public HediffDef addictionHediff;

		public HediffDef toleranceHediff;

		public bool canBinge = true;

		public float onGeneratedAddictedToleranceChance;

		public List<HediffGiver_Event> onGeneratedAddictedEvents;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (addictionHediff == null)
			{
				yield return "addictionHediff is null";
			}
		}
	}
}
