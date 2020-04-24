using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage : ScenPart
	{
		public override IEnumerable<Page> GetConfigPages()
		{
			yield return (Page)Activator.CreateInstance(def.pageClass);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
		}
	}
}
