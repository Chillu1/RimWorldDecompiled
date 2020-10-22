using Verse;

namespace RimWorld
{
	public static class ZonePresetNames
	{
		public static string PresetName(this StorageSettingsPreset preset)
		{
			return preset switch
			{
				StorageSettingsPreset.DumpingStockpile => "DumpingStockpile".Translate(), 
				StorageSettingsPreset.DefaultStockpile => "Stockpile".Translate(), 
				_ => "Zone".Translate(), 
			};
		}
	}
}
