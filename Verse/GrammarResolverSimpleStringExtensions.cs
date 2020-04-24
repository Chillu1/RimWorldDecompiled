using System.Collections.Generic;

namespace Verse
{
	public static class GrammarResolverSimpleStringExtensions
	{
		private static List<string> argsLabels = new List<string>();

		private static List<object> argsObjects = new List<object>();

		public static TaggedString Formatted(this string str, NamedArgument arg1)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1)
		{
			return str.RawText.Formatted(arg1);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2)
		{
			return str.RawText.Formatted(arg1, arg2);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			return str.RawText.Formatted(arg1, arg2, arg3);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			argsLabels.Add(arg4.label);
			argsObjects.Add(arg4.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			return str.RawText.Formatted(arg1, arg2, arg3, arg4);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			argsLabels.Add(arg4.label);
			argsObjects.Add(arg4.arg);
			argsLabels.Add(arg5.label);
			argsObjects.Add(arg5.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5)
		{
			return str.RawText.Formatted(arg1, arg2, arg3, arg4, arg5);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			argsLabels.Add(arg4.label);
			argsObjects.Add(arg4.arg);
			argsLabels.Add(arg5.label);
			argsObjects.Add(arg5.arg);
			argsLabels.Add(arg6.label);
			argsObjects.Add(arg6.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6)
		{
			return str.RawText.Formatted(arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			argsLabels.Add(arg4.label);
			argsObjects.Add(arg4.arg);
			argsLabels.Add(arg5.label);
			argsObjects.Add(arg5.arg);
			argsLabels.Add(arg6.label);
			argsObjects.Add(arg6.arg);
			argsLabels.Add(arg7.label);
			argsObjects.Add(arg7.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7)
		{
			return str.RawText.Formatted(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public static TaggedString Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7, NamedArgument arg8)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			argsLabels.Add(arg1.label);
			argsObjects.Add(arg1.arg);
			argsLabels.Add(arg2.label);
			argsObjects.Add(arg2.arg);
			argsLabels.Add(arg3.label);
			argsObjects.Add(arg3.arg);
			argsLabels.Add(arg4.label);
			argsObjects.Add(arg4.arg);
			argsLabels.Add(arg5.label);
			argsObjects.Add(arg5.arg);
			argsLabels.Add(arg6.label);
			argsObjects.Add(arg6.arg);
			argsLabels.Add(arg7.label);
			argsObjects.Add(arg7.arg);
			argsLabels.Add(arg8.label);
			argsObjects.Add(arg8.arg);
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7, NamedArgument arg8)
		{
			return str.RawText.Formatted(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		public static TaggedString Formatted(this string str, params NamedArgument[] args)
		{
			argsLabels.Clear();
			argsObjects.Clear();
			for (int i = 0; i < args.Length; i++)
			{
				argsLabels.Add(args[i].label);
				argsObjects.Add(args[i].arg);
			}
			return GrammarResolverSimple.Formatted(str, argsLabels, argsObjects);
		}

		public static TaggedString Formatted(this TaggedString str, params NamedArgument[] args)
		{
			return str.RawText.Formatted(args);
		}
	}
}
