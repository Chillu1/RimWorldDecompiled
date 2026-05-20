using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_MoteEmitterRandomColor : CompProperties_MoteEmitter
	{
		public List<Color> colors;

		public CompProperties_MoteEmitterRandomColor()
		{
			compClass = typeof(CompMoteEmitterRandomColor);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (colors.NullOrEmpty())
			{
				yield return "colors list is empty or null";
			}
		}
	}
}
