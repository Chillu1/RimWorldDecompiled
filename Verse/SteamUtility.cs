using Steamworks;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public static class SteamUtility
{
	private static string cachedPersonaName;

	public static string SteamPersonaName
	{
		get
		{
			if (SteamManager.Initialized && cachedPersonaName == null)
			{
				cachedPersonaName = SteamFriends.GetPersonaName();
			}
			if (cachedPersonaName == null)
			{
				return "???";
			}
			return cachedPersonaName;
		}
	}

	public static void OpenUrl(string url)
	{
		if (SteamManager.Initialized && SteamUtils.IsOverlayEnabled())
		{
			SteamFriends.ActivateGameOverlayToWebPage(url);
		}
		else
		{
			Application.OpenURL(url);
		}
	}

	public static void OpenWorkshopPage(PublishedFileId_t pfid)
	{
		OpenUrl(SteamWorkshopPageUrl(pfid));
	}

	public static void OpenSteamWorkshopPage()
	{
		OpenUrl("http://steamcommunity.com/workshop/browse/?appid=" + SteamUtils.GetAppID().ToString());
	}

	public static string SteamWorkshopPageUrl(PublishedFileId_t pfid)
	{
		PublishedFileId_t publishedFileId_t = pfid;
		return "steam://url/CommunityFilePage/" + publishedFileId_t.ToString();
	}
}
