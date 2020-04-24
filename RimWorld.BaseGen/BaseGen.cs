using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public static class BaseGen
	{
		public static GlobalSettings globalSettings = new GlobalSettings();

		public static SymbolStack symbolStack = new SymbolStack();

		private static Dictionary<string, List<RuleDef>> rulesBySymbol = new Dictionary<string, List<RuleDef>>();

		private static bool working;

		private static string currentSymbolPath;

		private const int MaxResolvedSymbols = 100000;

		private static List<SymbolResolver> tmpResolvers = new List<SymbolResolver>();

		public static string CurrentSymbolPath => currentSymbolPath;

		public static void Reset()
		{
			rulesBySymbol.Clear();
			List<RuleDef> allDefsListForReading = DefDatabase<RuleDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (!rulesBySymbol.TryGetValue(allDefsListForReading[i].symbol, out List<RuleDef> value))
				{
					value = new List<RuleDef>();
					rulesBySymbol.Add(allDefsListForReading[i].symbol, value);
				}
				value.Add(allDefsListForReading[i]);
			}
		}

		public static void Generate()
		{
			if (working)
			{
				Log.Error("Cannot call Generate() while already generating. Nested calls are not allowed.");
				return;
			}
			working = true;
			currentSymbolPath = "";
			try
			{
				if (symbolStack.Empty)
				{
					Log.Warning("Symbol stack is empty.");
				}
				else if (globalSettings.map == null)
				{
					Log.Error("Called BaseGen.Resolve() with null map.");
				}
				else
				{
					int num = symbolStack.Count - 1;
					int num2 = 0;
					while (true)
					{
						if (symbolStack.Empty)
						{
							return;
						}
						num2++;
						if (num2 > 100000)
						{
							break;
						}
						SymbolStack.Element toResolve = symbolStack.Pop();
						currentSymbolPath = toResolve.symbolPath;
						if (symbolStack.Count == num)
						{
							globalSettings.mainRect = toResolve.resolveParams.rect;
							num--;
						}
						try
						{
							Resolve(toResolve);
						}
						catch (Exception ex)
						{
							Log.Error("Error while resolving symbol \"" + toResolve.symbol + "\" with params=" + toResolve.resolveParams + "\n\nException: " + ex);
						}
					}
					Log.Error("Error in BaseGen: Too many iterations. Infinite loop?");
				}
			}
			catch (Exception arg)
			{
				Log.Error("Error in BaseGen: " + arg);
			}
			finally
			{
				working = false;
				symbolStack.Clear();
				globalSettings.Clear();
			}
		}

		private static void Resolve(SymbolStack.Element toResolve)
		{
			string symbol = toResolve.symbol;
			ResolveParams resolveParams = toResolve.resolveParams;
			tmpResolvers.Clear();
			if (rulesBySymbol.TryGetValue(symbol, out List<RuleDef> value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					RuleDef ruleDef = value[i];
					for (int j = 0; j < ruleDef.resolvers.Count; j++)
					{
						SymbolResolver symbolResolver = ruleDef.resolvers[j];
						if (symbolResolver.CanResolve(resolveParams))
						{
							tmpResolvers.Add(symbolResolver);
						}
					}
				}
			}
			if (!tmpResolvers.Any())
			{
				Log.Warning("Could not find any RuleDef for symbol \"" + symbol + "\" with any resolver that could resolve " + resolveParams);
			}
			else
			{
				tmpResolvers.RandomElementByWeight((SymbolResolver x) => x.selectionWeight).Resolve(resolveParams);
			}
		}
	}
}
