using System;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld.QuestGen;

public struct SlateRef<T> : ISlateRef, IEquatable<SlateRef<T>>
{
	public const string SlateRefFieldName = "slateRef";

	[MustTranslate_SlateRef]
	private string slateRef;

	private static Slate tmpCurSlate;

	private static readonly Regex VarsRegex = new Regex("\\$([a-zA-Z0-1_/]*)");

	private static readonly Regex HighPriorityVarsRegex = new Regex("\\(\\(\\$([a-zA-Z0-1_/]*)\\)\\)");

	private static readonly Regex MathExprRegex = new Regex("\\$\\((.*)\\)");

	private static MatchEvaluator RegexMatchEvaluatorConcatenateCached = RegexMatchEvaluatorConcatenate;

	private static MatchEvaluator RegexMatchEvaluatorConcatenateZeroIfEmptyCached = RegexMatchEvaluatorConcatenateZeroIfEmpty;

	private static MatchEvaluator RegexMatchEvaluatorEvaluateMathExpressionCached = RegexMatchEvaluatorResolveMathExpression;

	string ISlateRef.SlateRef
	{
		get
		{
			return slateRef;
		}
		set
		{
			slateRef = value;
		}
	}

	public SlateRef(string slateRef)
	{
		this.slateRef = slateRef;
	}

	public T GetValue(Slate slate)
	{
		TryGetValue(slate, out var value);
		return value;
	}

	public bool TryGetValue(Slate slate, out T value)
	{
		return TryGetConvertedValue<T>(slate, out value);
	}

	public bool TryGetConvertedValue<TAnything>(Slate slate, out TAnything value)
	{
		if (slateRef == null)
		{
			value = default(TAnything);
			return true;
		}
		tmpCurSlate = slate;
		string text = HighPriorityVarsRegex.Replace(slateRef, RegexMatchEvaluatorConcatenate);
		if (!SlateRefUtility.CheckSingleVariableSyntax(text, slate, out var obj, out var exists))
		{
			obj = MathExprRegex.Replace(text, RegexMatchEvaluatorEvaluateMathExpressionCached);
			obj = VarsRegex.Replace((string)obj, RegexMatchEvaluatorConcatenateCached);
			exists = true;
		}
		tmpCurSlate = null;
		if (!exists)
		{
			value = default(TAnything);
			return false;
		}
		if (obj == null)
		{
			value = default(TAnything);
			return true;
		}
		if (obj is TAnything)
		{
			value = (TAnything)obj;
			return true;
		}
		if (ConvertHelper.CanConvert<TAnything>(obj))
		{
			value = ConvertHelper.Convert<TAnything>(obj);
			return true;
		}
		Log.Error("Could not convert SlateRef \"" + slateRef + "\" (" + obj.GetType().Name + ") to " + typeof(TAnything).Name);
		value = default(TAnything);
		return false;
	}

	private static string RegexMatchEvaluatorConcatenate(Match match)
	{
		string value = match.Groups[1].Value;
		if (!tmpCurSlate.TryGet<object>(value, out var var))
		{
			return "";
		}
		if (var == null)
		{
			return "";
		}
		return var.ToString();
	}

	private static string RegexMatchEvaluatorConcatenateZeroIfEmpty(Match match)
	{
		string value = match.Groups[1].Value;
		if (!tmpCurSlate.TryGet<object>(value, out var var))
		{
			Log.ErrorOnce("Tried to use variable \"" + value + "\" in a math expression but it doesn't exist.", value.GetHashCode() ^ 0xB9D489F);
			return "0";
		}
		if (var == null)
		{
			return "0";
		}
		string text = var.ToString();
		if (text == "")
		{
			return "0";
		}
		return text;
	}

	private static string RegexMatchEvaluatorResolveMathExpression(Match match)
	{
		string value = match.Groups[1].Value;
		value = VarsRegex.Replace(value, RegexMatchEvaluatorConcatenateZeroIfEmptyCached);
		return MathEvaluator.Evaluate(value).ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SlateRef<T>))
		{
			return false;
		}
		return Equals((SlateRef<T>)obj);
	}

	public bool Equals(SlateRef<T> other)
	{
		return this == other;
	}

	public static bool operator ==(SlateRef<T> a, SlateRef<T> b)
	{
		return a.slateRef == b.slateRef;
	}

	public static bool operator !=(SlateRef<T> a, SlateRef<T> b)
	{
		return !(a == b);
	}

	public static implicit operator SlateRef<T>(T t)
	{
		return new SlateRef<T>(t?.ToString());
	}

	public override int GetHashCode()
	{
		if (slateRef == null)
		{
			return 0;
		}
		return slateRef.GetHashCode();
	}

	public string ToString(Slate slate)
	{
		TryGetConvertedValue<string>(slate, out var value);
		return value;
	}

	public override string ToString()
	{
		if (!QuestGen.Working)
		{
			return slateRef;
		}
		return ToString(QuestGen.slate);
	}
}
