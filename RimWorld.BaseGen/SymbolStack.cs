using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolStack
{
	public struct Element
	{
		public string symbol;

		public ResolveParams resolveParams;

		public string symbolPath;
	}

	private Stack<Element> stack = new Stack<Element>();

	public bool Empty => stack.Count == 0;

	public int Count => stack.Count;

	public void Push(string symbol, ResolveParams resolveParams, string customNameForPath = null)
	{
		string text = BaseGen.CurrentSymbolPath;
		if (!text.NullOrEmpty())
		{
			text += "_";
		}
		text += customNameForPath ?? symbol;
		Element item = new Element
		{
			symbol = symbol,
			resolveParams = resolveParams,
			symbolPath = text
		};
		stack.Push(item);
	}

	public void Push(string symbol, CellRect rect, string customNameForPath = null)
	{
		Push(symbol, new ResolveParams
		{
			rect = rect
		}, customNameForPath);
	}

	public void PushMany(ResolveParams resolveParams, params string[] symbols)
	{
		for (int i = 0; i < symbols.Length; i++)
		{
			Push(symbols[i], resolveParams);
		}
	}

	public void PushMany(CellRect rect, params string[] symbols)
	{
		for (int i = 0; i < symbols.Length; i++)
		{
			Push(symbols[i], rect);
		}
	}

	public Element Pop()
	{
		return stack.Pop();
	}

	public void Clear()
	{
		stack.Clear();
	}
}
