using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Indoors_Leaf_Gravcore : SymbolResolver
{
	public static bool CanResolve(string symbol, ResolveParams rp)
	{
		List<RuleDef> allDefsListForReading = DefDatabase<RuleDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			RuleDef ruleDef = allDefsListForReading[i];
			if (ruleDef.symbol != symbol)
			{
				continue;
			}
			for (int j = 0; j < ruleDef.resolvers.Count; j++)
			{
				if (ruleDef.resolvers[j] is SymbolResolver_BasePart_Indoors_Leaf_Gravcore && ruleDef.resolvers[j].CanResolve(rp))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_gravcoresResolved >= BaseGen.globalSettings.requiredGravcoreRooms)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		BaseGen.symbolStack.Push("gravcore", rp);
		BaseGen.globalSettings.basePart_gravcoresResolved++;
	}
}
