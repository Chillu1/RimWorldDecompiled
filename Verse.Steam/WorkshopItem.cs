using Steamworks;
using System.IO;

namespace Verse.Steam
{
	public class WorkshopItem
	{
		protected DirectoryInfo directoryInt;

		private PublishedFileId_t pfidInt;

		public DirectoryInfo Directory => directoryInt;

		public virtual PublishedFileId_t PublishedFileId
		{
			get
			{
				return pfidInt;
			}
			set
			{
				pfidInt = value;
			}
		}

		public static WorkshopItem MakeFrom(PublishedFileId_t pfid)
		{
			ulong punSizeOnDisk;
			string pchFolder;
			uint punTimeStamp;
			bool itemInstallInfo = SteamUGC.GetItemInstallInfo(pfid, out punSizeOnDisk, out pchFolder, 257u, out punTimeStamp);
			WorkshopItem workshopItem = null;
			if (!itemInstallInfo)
			{
				workshopItem = new WorkshopItem_NotInstalled();
			}
			else
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(pchFolder);
				if (!directoryInfo.Exists)
				{
					Log.Error("Created WorkshopItem for " + pfid + " but there is no folder for it.");
				}
				FileInfo[] files = directoryInfo.GetFiles();
				for (int i = 0; i < files.Length; i++)
				{
					if (files[i].Extension == ".rsc")
					{
						workshopItem = new WorkshopItem_Scenario();
						break;
					}
				}
				if (workshopItem == null)
				{
					workshopItem = new WorkshopItem_Mod();
				}
				workshopItem.directoryInt = directoryInfo;
			}
			workshopItem.PublishedFileId = pfid;
			return workshopItem;
		}

		public override string ToString()
		{
			return GetType().ToString() + "-" + PublishedFileId;
		}
	}
}
