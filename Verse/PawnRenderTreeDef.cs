using System.Collections.Generic;

namespace Verse;

public class PawnRenderTreeDef : Def
{
	public PawnRenderNodeProperties root;

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

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		root?.ResolveReferencesRecursive();
	}
}
