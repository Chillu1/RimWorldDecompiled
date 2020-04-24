using System;

namespace Verse
{
	public static class AnimalNameDisplayModeExtension
	{
		public static string ToStringHuman(this AnimalNameDisplayMode mode)
		{
			switch (mode)
			{
			case AnimalNameDisplayMode.None:
				return "None".Translate();
			case AnimalNameDisplayMode.TameNamed:
				return "AnimalNameDisplayMode_TameNamed".Translate();
			case AnimalNameDisplayMode.TameAll:
				return "AnimalNameDisplayMode_TameAll".Translate();
			default:
				throw new NotImplementedException();
			}
		}
	}
}
