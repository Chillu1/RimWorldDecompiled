using Verse;

namespace RimWorld
{
	public static class StoragePriorityHelper
	{
		public static string Label(this StoragePriority p)
		{
			switch (p)
			{
			case StoragePriority.Unstored:
				return "StoragePriorityUnstored".Translate();
			case StoragePriority.Low:
				return "StoragePriorityLow".Translate();
			case StoragePriority.Normal:
				return "StoragePriorityNormal".Translate();
			case StoragePriority.Preferred:
				return "StoragePriorityPreferred".Translate();
			case StoragePriority.Important:
				return "StoragePriorityImportant".Translate();
			case StoragePriority.Critical:
				return "StoragePriorityCritical".Translate();
			default:
				return "Unknown";
			}
		}
	}
}
