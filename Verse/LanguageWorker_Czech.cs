using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse;

public class LanguageWorker_Czech : LanguageWorker
{
	private readonly struct LookupCacheKey : IEquatable<LookupCacheKey>
	{
		public string OriginalInput { get; }

		public string Path { get; }

		public int Index { get; }

		public LookupCacheKey(string originalInput, string path, int index)
		{
			OriginalInput = originalInput;
			Path = path;
			Index = index;
		}

		private bool Equals(LookupCacheKey other)
		{
			if (Index == other.Index && string.Equals(Path, other.Path, StringComparison.Ordinal))
			{
				return string.Equals(OriginalInput, other.OriginalInput, StringComparison.Ordinal);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LookupCacheKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Index * 397) ^ (Path?.GetHashCode() ?? 0)) * 397) ^ (OriginalInput?.GetHashCode() ?? 0);
		}

		bool IEquatable<LookupCacheKey>.Equals(LookupCacheKey other)
		{
			return Equals(other);
		}
	}

	private const int ReplaceRegexCacheSize = 107;

	private const int LookupCacheSize = 107;

	private static readonly StringBuilder _log = new StringBuilder();

	private static bool _emitLog;

	private static readonly Dictionary<LookupCacheKey, string> _lookupCache = new Dictionary<LookupCacheKey, string>(107);

	private static readonly LookupCacheKey[] _lookupKeys = new LookupCacheKey[107];

	private static int _lookupKeyDeleteIndex;

	private static readonly char[] _fastLookupChars = new char[2] { '(', '[' };

	private static readonly Dictionary<string, Regex> _replaceRegexCache = new Dictionary<string, Regex>(107);

	private static readonly string[] _replaceRegexKeys = new string[107];

	private static int _replaceRegexKeyDeleteIndex;

	private static readonly char[] _firstPossibleRegexChars = new char[10] { '(', '[', '{', '*', '+', '?', '.', '^', '$', '|' };

	private static readonly Regex _replacePatternArgRegex = new Regex("(?<old>[^\"]*?)\"-\"(?<new>[^\"]*?)\"", RegexOptions.Compiled);

	public override string ResolveFunction(string functionName, List<string> args, string fullStringForReference)
	{
		try
		{
			if (functionName == "lookup" && (args.Count == 3 || args.Count == 2))
			{
				if (DebugSettings.logTranslationLookupErrors)
				{
					_log.Clear();
					_log.AppendLine("ResolveLookup - (" + string.Join(", ", args) + ")");
				}
				return DoLookup(args, fullStringForReference);
			}
		}
		finally
		{
			if (DebugSettings.logTranslationLookupErrors && _emitLog)
			{
				Log.Message(_log.ToString());
				_emitLog = false;
				_log.Clear();
			}
		}
		if (functionName == "replace" && args.Count > 1)
		{
			return DoReplace(args);
		}
		return base.ResolveFunction(functionName, args, fullStringForReference);
	}

	private string DoLookup(List<string> args, string fullStringForReference)
	{
		if (DebugSettings.logTranslationLookupErrors)
		{
			_emitLog = true;
		}
		string text = args[0];
		string text2 = args[1];
		if (Path.DirectorySeparatorChar != '\\')
		{
			text2 = (args[1] = text2.Replace('\\', Path.DirectorySeparatorChar));
		}
		int result;
		int index = ((args.Count != 3) ? 1 : (int.TryParse(args[2], out result) ? result : (-1)));
		LookupCacheKey lookupCacheKey = new LookupCacheKey(text, text2, index);
		if (TryLookup(args, lookupCacheKey, out var lookupResult))
		{
			return lookupResult;
		}
		if (TryFindIndexOfAny(text, _fastLookupChars, 0, out var index2, trim: true))
		{
			if (TryLookup(args, text, 0, index2, out lookupResult))
			{
				SaveToLookupCache(lookupCacheKey, lookupResult);
				return lookupResult;
			}
		}
		else
		{
			index2 = 0;
		}
		if (TryLookupRecursive(args, text, 0, (index2 == 0) ? text.Length : index2, out lookupResult))
		{
			SaveToLookupCache(lookupCacheKey, lookupResult);
			return lookupResult;
		}
		args[0] = text;
		lookupResult = ResolveLookup(args, fullStringForReference);
		SaveToLookupCache(lookupCacheKey, lookupResult);
		return lookupResult;
	}

	private bool TryLookup(List<string> args, LookupCacheKey lookupCacheKey, out string lookupResult)
	{
		if (_lookupCache.TryGetValue(lookupCacheKey, out lookupResult))
		{
			if (DebugSettings.logTranslationLookupErrors)
			{
				_log.AppendLine($" - Cache hit for {lookupCacheKey.Path} at index {lookupCacheKey.Index}: {lookupResult}");
			}
			_emitLog = false;
			return true;
		}
		if (TryLookup(args, lookupCacheKey.OriginalInput, 0, lookupCacheKey.OriginalInput.Length, out lookupResult))
		{
			SaveToLookupCache(lookupCacheKey, lookupResult);
			return true;
		}
		return false;
	}

	private bool TryLookupRecursive(List<string> args, string originalInput, int fromIndex, int count, out string output)
	{
		if (fromIndex + count >= originalInput.Length)
		{
			if (DebugSettings.logTranslationLookupErrors)
			{
				_emitLog = true;
				_log.AppendLine($"TryLookupRecursive - Invalid range {fromIndex + count} >= {originalInput.Length} (length of '{originalInput}'), returning false.");
			}
			output = null;
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num++ > 50)
			{
				if (DebugSettings.logTranslationLookupErrors)
				{
					_emitLog = true;
					_log.AppendLine($"TryLookupRecursive - Too many iterations ({num}), returning false.");
				}
				output = null;
				return false;
			}
			int index = fromIndex + count - 1;
			if (!TryFindWhiteSpaceBackward(originalInput, ref index, fromIndex, lookForWhitespace: true))
			{
				break;
			}
			TryFindWhiteSpaceBackward(originalInput, ref index, fromIndex, lookForWhitespace: false);
			count = index - fromIndex;
			if (TryLookup(args, originalInput, fromIndex, count, out output))
			{
				return true;
			}
		}
		output = null;
		return false;
	}

	private bool TryLookup(List<string> args, string originalInput, int fromIndex, int count, out string output)
	{
		if (DebugSettings.logTranslationLookupErrors)
		{
			_log.Append($"TryLookup: from:{fromIndex}, count:{count}");
		}
		output = "";
		if (count <= 0)
		{
			if (DebugSettings.logTranslationLookupErrors)
			{
				_log.AppendLine(" - Invalid count, returning false.");
			}
			return false;
		}
		if (fromIndex != 0 || count != originalInput.Length)
		{
			args[0] = originalInput.Substring(fromIndex, count);
		}
		string text = ResolveLookup(args, originalInput);
		if (text == "" || text == args[0])
		{
			if (DebugSettings.logTranslationLookupErrors)
			{
				_log.AppendLine(" - FAILED - args[0]: '" + args[0] + "'");
			}
			return false;
		}
		bool flag = fromIndex > 0;
		int num = fromIndex + count;
		bool flag2 = num < originalInput.Length;
		output = ((!flag) ? (flag2 ? (text + originalInput.Substring(num)) : text) : (flag2 ? (originalInput.Substring(0, fromIndex) + text + originalInput.Substring(num)) : (originalInput.Substring(0, fromIndex) + text)));
		if (DebugSettings.logTranslationLookupErrors)
		{
			_log.AppendLine($" - args[0]: '{args[0]}' - return: {output} (lookupResult: {text}, hasPrefix: {flag}, hasSuffix: {flag2}, endLookupIndex: {num})");
		}
		return output != "";
	}

	private static void SaveToLookupCache(LookupCacheKey lookupCacheKey, string lookupResult)
	{
		SaveToCache(lookupCacheKey, lookupResult, _lookupCache, _lookupKeys, ref _lookupKeyDeleteIndex, 107);
	}

	private string DoReplace(List<string> args)
	{
		if (args.Count == 0)
		{
			return null;
		}
		string text = args[0];
		if (args.Count == 1)
		{
			return text;
		}
		for (int i = 1; i < args.Count; i++)
		{
			string input = args[i];
			Match match = _replacePatternArgRegex.Match(input);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups["old"].Value;
			string value2 = match.Groups["new"].Value;
			if (text.Contains(value))
			{
				return text.Replace(value, value2);
			}
			Regex replaceRegex = GetReplaceRegex(value);
			if (replaceRegex != null)
			{
				string text2 = replaceRegex.Replace(text, value2);
				if (!(text2 == text))
				{
					return text2;
				}
			}
		}
		return text;
	}

	private Regex GetReplaceRegex(string regexPattern)
	{
		if (_replaceRegexCache.TryGetValue(regexPattern, out var value))
		{
			return value;
		}
		if (!TryFindIndexOfAny(regexPattern, _firstPossibleRegexChars, 0, out var _))
		{
			SaveToReplaceCache(regexPattern, null);
			return null;
		}
		try
		{
			value = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			SaveToReplaceCache(regexPattern, value);
			return value;
		}
		catch
		{
			SaveToReplaceCache(regexPattern, null);
			return null;
		}
	}

	private static void SaveToReplaceCache(string replacePattern, Regex regex)
	{
		SaveToCache(replacePattern, regex, _replaceRegexCache, _replaceRegexKeys, ref _replaceRegexKeyDeleteIndex, 107);
	}

	private static bool TryFindIndexOfAny(string input, char[] chars, int fromIndex, out int index, bool trim = false)
	{
		index = input.IndexOfAny(chars, fromIndex);
		if (index == -1)
		{
			return false;
		}
		if (trim)
		{
			TryFindWhiteSpaceBackward(input, ref index, fromIndex, lookForWhitespace: false);
		}
		return true;
	}

	private static bool TryFindWhiteSpaceBackward(string input, ref int index, int stopIndex, bool lookForWhitespace)
	{
		while (true)
		{
			if (index <= 0)
			{
				return false;
			}
			if (index <= stopIndex)
			{
				return false;
			}
			if (char.IsWhiteSpace(input[index - 1]) == lookForWhitespace)
			{
				break;
			}
			index--;
		}
		return true;
	}

	private static void SaveToCache<TKey, TValue>(TKey key, TValue value, Dictionary<TKey, TValue> cache, TKey[] keys, ref int deleteIndex, int cacheSize)
	{
		if (cache.Count == cacheSize)
		{
			TKey key2 = keys[deleteIndex];
			cache.Remove(key2);
			keys[deleteIndex] = key;
			deleteIndex = (deleteIndex + 1) % cacheSize;
		}
		else
		{
			keys[cache.Count] = key;
		}
		cache[key] = value;
	}
}
