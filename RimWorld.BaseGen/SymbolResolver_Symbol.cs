using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Symbol : SymbolResolver
	{
		public string symbol;

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
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
					if (ruleDef.resolvers[j].CanResolve(rp))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push(symbol, rp);
		}
	}
}
