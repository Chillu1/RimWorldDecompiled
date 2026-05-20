namespace Verse
{
	public static class PawnRenderFlagsExtension
	{
		public static bool FlagSet(this PawnRenderFlags flags, PawnRenderFlags flag)
		{
			return (flags & flag) != 0;
		}
	}
}
