using System;

namespace Verse
{
	public static class AnimalNameDisplayModeExtension
	{
		public static string ToStringHuman(this AnimalNameDisplayMode mode)
		{
			return mode switch
			{
				AnimalNameDisplayMode.None => "None".Translate(), 
				AnimalNameDisplayMode.TameNamed => "AnimalNameDisplayMode_TameNamed".Translate(), 
				AnimalNameDisplayMode.TameAll => "AnimalNameDisplayMode_TameAll".Translate(), 
				_ => throw new NotImplementedException(), 
			};
		}
	}
}
