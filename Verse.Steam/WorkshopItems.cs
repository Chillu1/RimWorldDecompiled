using System.Collections.Generic;
using System.Text;
using RimWorld;
using Steamworks;

namespace Verse.Steam
{
	public static class WorkshopItems
	{
		private static List<WorkshopItem> subbedItems;

		public static IEnumerable<WorkshopItem> AllSubscribedItems => subbedItems;

		public static int DownloadingItemsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < subbedItems.Count; i++)
				{
					if (subbedItems[i] is WorkshopItem_NotInstalled)
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
			foreach (PublishedFileId_t item in Workshop.AllSubscribedItems())
			{
				subbedItems.Add(WorkshopItem.MakeFrom(item));
			}
			ModLister.RebuildModList();
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
}
