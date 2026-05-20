using System;

namespace Verse
{
	public static class ShowWeaponsUnderPortraitModeExtension
	{
		public static string ToStringHuman(this ShowWeaponsUnderPortraitMode mode)
		{
			return mode switch
			{
				ShowWeaponsUnderPortraitMode.Never => "Never".Translate().CapitalizeFirst(), 
				ShowWeaponsUnderPortraitMode.WhileDrafted => "ShowWeapons_WhileDrafted".Translate().CapitalizeFirst(), 
				ShowWeaponsUnderPortraitMode.Always => "ShowWeapons_Always".Translate().CapitalizeFirst(), 
				_ => throw new NotImplementedException(), 
			};
		}
	}
}
