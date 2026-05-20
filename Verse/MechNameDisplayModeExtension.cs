using System;

namespace Verse
{
	public static class MechNameDisplayModeExtension
	{
		public static string ToStringHuman(this MechNameDisplayMode mode)
		{
			return mode switch
			{
				MechNameDisplayMode.None => "Never".Translate().CapitalizeFirst(), 
				MechNameDisplayMode.WhileDrafted => "MechNameDisplayMode_WhileDrafted".Translate().CapitalizeFirst(), 
				MechNameDisplayMode.Always => "MechNameDisplayMode_Always".Translate().CapitalizeFirst(), 
				_ => throw new NotImplementedException(), 
			};
		}

		public static bool ShouldDisplayMechName(this MechNameDisplayMode mode, Pawn mech)
		{
			switch (mode)
			{
			case MechNameDisplayMode.None:
				return false;
			case MechNameDisplayMode.WhileDrafted:
				if (mech.Name != null)
				{
					return mech.Drafted;
				}
				return false;
			case MechNameDisplayMode.Always:
				return mech.Name != null;
			default:
				throw new NotImplementedException(Prefs.MechNameMode.ToStringSafe());
			}
		}
	}
}
