namespace Verse
{
	public static class TranslatorFormattedStringExtensions
	{
		public static TaggedString Translate(this string key, NamedArgument arg1)
		{
			return key.Translate().Formatted(arg1);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2)
		{
			return key.Translate().Formatted(arg1, arg2);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			return key.Translate().Formatted(arg1, arg2, arg3);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			return key.Translate().Formatted(arg1, arg2, arg3, arg4);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5)
		{
			return key.Translate().Formatted(arg1, arg2, arg3, arg4, arg5);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6)
		{
			return key.Translate().Formatted(arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7)
		{
			return key.Translate().Formatted(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public static TaggedString Translate(this string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7, NamedArgument arg8)
		{
			return key.Translate().Formatted(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		public static TaggedString Translate(this string key, params NamedArgument[] args)
		{
			return key.Translate().Formatted(args);
		}
	}
}
