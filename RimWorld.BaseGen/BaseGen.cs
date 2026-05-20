using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public static class BaseGen
{
	public static GlobalSettings globalSettings = new GlobalSettings();

	public static SymbolStack symbolStack = new SymbolStack();

	private static Dictionary<string, List<RuleDef>> rulesBySymbol = new Dictionary<string, List<RuleDef>>();

	private static bool working;

	private static string currentSymbolPath;

	private const int MaxResolvedSymbols = 100000;

	private static readonly List<SymbolResolver> tmpResolvers = new List<SymbolResolver>();

	public static string CurrentSymbolPath => currentSymbolPath;

	public static void Reset()
	{
		rulesBySymbol.Clear();
		List<RuleDef> allDefsListForReading = DefDatabase<RuleDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (!rulesBySymbol.TryGetValue(allDefsListForReading[i].symbol, out var value))
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
		globalSettings.ClearResult();
		try
		{
			if (symbolStack.Empty)
			{
				Log.Warning("Symbol stack is empty.");
				return;
			}
			if (globalSettings.map == null)
			{
				Log.Error("Called BaseGen.Resolve() with null map.");
				return;
			}
			int num = symbolStack.Count - 1;
			int num2 = 0;
			while (!symbolStack.Empty)
			{
				num2++;
				if (num2 > 100000)
				{
					Log.Error("Error in BaseGen: Too many iterations. Infinite loop?");
					break;
				}
				SymbolStack.Element toResolve = symbolStack.Pop();
				currentSymbolPath = toResolve.symbolPath;
				if (symbolStack.Count == num)
				{
					globalSettings.mainRect = toResolve.resolveParams.rect;
					num--;
				}
				using (Rand.Block(globalSettings.map.NextGenSeed))
				{
					try
					{
						Resolve(toResolve);
					}
					catch (Exception ex)
					{
						string[] obj = new string[6] { "Error while resolving symbol \"", toResolve.symbol, "\" with params=", null, null, null };
						ResolveParams resolveParams = toResolve.resolveParams;
						obj[3] = resolveParams.ToString();
						obj[4] = "\n\nException: ";
						obj[5] = ex?.ToString();
						Log.Error(string.Concat(obj));
					}
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Error("Error in BaseGen: " + ex2);
		}
		finally
		{
			globalSettings.landingPadsGenerated = globalSettings.basePart_landingPadsResolved;
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
		if (rulesBySymbol.TryGetValue(symbol, out var value))
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
			ResolveParams resolveParams2 = resolveParams;
			Log.Warning("Could not find any RuleDef for symbol \"" + symbol + "\" with any resolver that could resolve " + resolveParams2.ToString());
			return;
		}
		SymbolResolver symbolResolver2 = tmpResolvers.RandomElementByWeight((SymbolResolver x) => x.selectionWeight);
		resolveParams.rect = resolveParams.rect.ClipInsideMap(globalSettings.map);
		symbolResolver2.Resolve(resolveParams);
	}
}
