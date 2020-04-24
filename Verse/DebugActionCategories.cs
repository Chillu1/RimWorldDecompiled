using System.Collections.Generic;

namespace Verse
{
	public static class DebugActionCategories
	{
		public const string Incidents = "Incidents";

		public const string Quests = "Quests";

		public const string QuestsOld = "Quests (old)";

		public const string Translation = "Translation";

		public const string General = "General";

		public const string Pawns = "Pawns";

		public const string Spawning = "Spawning";

		public const string MapManagement = "Map management";

		public const string Autotests = "Autotests";

		public const string Mods = "Mods";

		public static readonly Dictionary<string, int> categoryOrders;

		static DebugActionCategories()
		{
			categoryOrders = new Dictionary<string, int>();
			categoryOrders.Add("Incidents", 100);
			categoryOrders.Add("Quests", 200);
			categoryOrders.Add("Quests (old)", 250);
			categoryOrders.Add("Translation", 300);
			categoryOrders.Add("General", 400);
			categoryOrders.Add("Pawns", 500);
			categoryOrders.Add("Spawning", 600);
			categoryOrders.Add("Map management", 700);
			categoryOrders.Add("Autotests", 800);
			categoryOrders.Add("Mods", 900);
		}

		public static int GetOrderFor(string category)
		{
			if (categoryOrders.TryGetValue(category, out int value))
			{
				return value;
			}
			return int.MaxValue;
		}
	}
}
