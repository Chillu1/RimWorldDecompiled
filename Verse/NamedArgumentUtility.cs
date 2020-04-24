namespace Verse
{
	public static class NamedArgumentUtility
	{
		public static NamedArgument Named(this object arg, string label)
		{
			return new NamedArgument(arg, label);
		}
	}
}
