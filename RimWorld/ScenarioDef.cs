using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenarioDef : Def
	{
		public Scenario scenario;

		public override void PostLoad()
		{
			base.PostLoad();
			if (scenario.name.NullOrEmpty())
			{
				scenario.name = label;
			}
			if (scenario.description.NullOrEmpty())
			{
				scenario.description = description;
			}
			scenario.Category = ScenarioCategory.FromDef;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (scenario == null)
			{
				yield return "null scenario";
			}
			foreach (string item in scenario.ConfigErrors())
			{
				yield return item;
			}
		}
	}
}
