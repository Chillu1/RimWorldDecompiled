using System.Collections.Generic;
using System.Text;
using RimWorld;
using Steamworks;

namespace Verse.Steam;

public static class WorkshopItems
{
	private static List<WorkshopItem> subbedItems;

	private static List<WorkshopItem_Downloading> downloadingItems;

	public static IEnumerable<WorkshopItem> AllSubscribedItems => subbedItems;

	public static IEnumerable<WorkshopItem_Downloading> AllDownloadingItems => downloadingItems;

	public static int DownloadingItemsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < subbedItems.Count; i++)
			{
				if (subbedItems[i] is WorkshopItem_Downloading)
				{
					num++;
				}
			}
			return num;
		}
	}

	static WorkshopItems()
	{
		subbedItems = new List<WorkshopItem>();
		downloadingItems = new List<WorkshopItem_Downloading>();
		RebuildItemsList();
	}

	public static void EnsureInit()
	{
	}

	public static WorkshopItem GetItem(PublishedFileId_t pfid)
	{
		for (int i = 0; i < subbedItems.Count; i++)
		{
			if (subbedItems[i].PublishedFileId == pfid)
			{
				return subbedItems[i];
			}
		}
		return null;
	}

	public static bool HasItem(PublishedFileId_t pfid)
	{
		return GetItem(pfid) != null;
	}

	private static void RebuildItemsList()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		subbedItems.Clear();
		downloadingItems.Clear();
		foreach (PublishedFileId_t item in Workshop.AllSubscribedItems())
		{
			WorkshopItem workshopItem = WorkshopItem.MakeFrom(item);
			if (workshopItem != null)
			{
				subbedItems.Add(workshopItem);
				if (workshopItem is WorkshopItem_Downloading)
				{
					downloadingItems.Add(workshopItem as WorkshopItem_Downloading);
				}
			}
		}
		ScenarioLister.MarkDirty();
	}

	internal static void Notify_Subscribed(PublishedFileId_t pfid)
	{
		RebuildItemsList();
	}

	internal static void Notify_Installed(PublishedFileId_t pfid)
	{
		RebuildItemsList();
	}

	internal static void Notify_Unsubscribed(PublishedFileId_t pfid)
	{
		RebuildItemsList();
	}

	public static string DebugOutput()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Subscribed items:");
		foreach (WorkshopItem subbedItem in subbedItems)
		{
			stringBuilder.AppendLine("  " + subbedItem.ToString());
		}
		return stringBuilder.ToString();
	}
}
