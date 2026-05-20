using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThingSetMakerDef : Def
{
	public ThingSetMaker root;

	public ThingSetMakerParams debugParams;

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		root.ResolveReferences();
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (root == null)
		{
			yield return "root is null.";
			yield break;
		}
		foreach (string item2 in root.ConfigErrors())
		{
			yield return item2;
		}
	}
}
