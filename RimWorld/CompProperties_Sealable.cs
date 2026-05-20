using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Sealable : CompProperties
{
	[NoTranslate]
	public string sealTexPath;

	[MustTranslate]
	public string sealCommandLabel;

	[MustTranslate]
	public string sealCommandDesc;

	[MustTranslate]
	public string cannotSealLabel;

	[MustTranslate]
	public string confirmSealText;

	[MustTranslate]
	public string sealedInspectString;

	[MustTranslate]
	public string cannotEnterLabel;

	public bool destroyPortal;

	public CompProperties_Sealable()
	{
		compClass = typeof(CompSealable);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(MapPortal).IsAssignableFrom(parentDef.thingClass))
		{
			yield return "has CompSealable, but its ThingClass is not MapPortal";
		}
	}
}
