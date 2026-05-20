using Verse;

namespace RimWorld;

public static class StoragePriorityHelper
{
	public static string Label(this StoragePriority p)
	{
		return p switch
		{
			StoragePriority.Unstored => "StoragePriorityUnstored".Translate(), 
			StoragePriority.Low => "StoragePriorityLow".Translate(), 
			StoragePriority.Normal => "StoragePriorityNormal".Translate(), 
			StoragePriority.Preferred => "StoragePriorityPreferred".Translate(), 
			StoragePriority.Important => "StoragePriorityImportant".Translate(), 
			StoragePriority.Critical => "StoragePriorityCritical".Translate(), 
			_ => "Unknown", 
		};
	}
}
