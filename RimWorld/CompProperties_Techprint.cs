using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompProperties_Techprint : CompProperties
{
	public ResearchProjectDef project;

	public CompProperties_Techprint()
	{
		compClass = typeof(CompTechprint);
	}

	public override void ResolveReferences(ThingDef parentDef)
	{
		if (parentDef.descriptionHyperlinks == null)
		{
			parentDef.descriptionHyperlinks = new List<DefHyperlink>();
		}
		List<Def> unlockedDefs = project.UnlockedDefs;
		for (int i = 0; i < unlockedDefs.Count; i++)
		{
			if (unlockedDefs[i] is ThingDef thingDef)
			{
				parentDef.descriptionHyperlinks.Add(thingDef);
			}
			else if (unlockedDefs[i] is RecipeDef recipeDef && !recipeDef.products.NullOrEmpty())
			{
				for (int j = 0; j < recipeDef.products.Count; j++)
				{
					parentDef.descriptionHyperlinks.Add(recipeDef.products[j].thingDef);
				}
			}
		}
		parentDef.description += "\n\n" + "Unlocks".Translate() + ": " + project.UnlockedDefs.Select((Def x) => x.label).ToCommaList().CapitalizeFirst();
	}
}
