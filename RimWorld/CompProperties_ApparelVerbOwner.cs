using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_ApparelVerbOwner : CompProperties
{
	public KeyBindingDef hotKey;

	public bool displayGizmoWhileUndrafted = true;

	public bool displayGizmoWhileDrafted = true;

	public CompProperties_ApparelVerbOwner()
	{
		compClass = typeof(CompApparelVerbOwner);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(Apparel).IsAssignableFrom(parentDef.thingClass))
		{
			yield return $"Comp {compClass} can only be added to Apparel";
		}
	}
}
