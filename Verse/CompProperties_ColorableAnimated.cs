using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CompProperties_ColorableAnimated : CompProperties
	{
		public int changeInterval = 1;

		public bool startWithRandom;

		public List<Color> colors = new List<Color>();

		public CompProperties_ColorableAnimated()
		{
			compClass = typeof(CompColorable_Animated);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (colors.Count == 0)
			{
				yield return "there should be at least one color specified in colors list";
			}
		}
	}
}
