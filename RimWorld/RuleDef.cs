using System.Collections.Generic;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class RuleDef : Def
{
	[NoTranslate]
	public string symbol;

	public List<SymbolResolver> resolvers;
}
