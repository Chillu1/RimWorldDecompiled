using Verse;

namespace RimWorld
{
	public static class ZonePresetNames
	{
		public static string PresetName(this StorageSettingsPreset preset)
		{
			switch (preset)
			{
			case StorageSettingsPreset.DumpingStockpile:
				return "DumpingStockpile".Translate();
			case StorageSettingsPreset.DefaultStockpile:
				return "Stockpile".Translate();
			default:
				return "Zone".Translate();
			}
		}
	}
}
