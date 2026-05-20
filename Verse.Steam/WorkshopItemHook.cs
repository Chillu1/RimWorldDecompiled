using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace Verse.Steam;

public class WorkshopItemHook
{
	private WorkshopUploadable owner;

	private CSteamID steamAuthor = CSteamID.Nil;

	private CallResult<SteamUGCRequestUGCDetailsResult_t> queryResult;

	public PublishedFileId_t PublishedFileId
	{
		get
		{
			return owner.GetPublishedFileId();
		}
		set
		{
			owner.SetPublishedFileId(value);
		}
	}

	public string Name => owner.GetWorkshopName();

	public string Description => owner.GetWorkshopDescription();

	public string PreviewImagePath => owner.GetWorkshopPreviewImagePath();

	public IList<string> Tags => owner.GetWorkshopTags();

	public DirectoryInfo Directory => owner.GetWorkshopUploadDirectory();

	public IEnumerable<System.Version> SupportedVersions => owner.SupportedVersions;

	public bool MayHaveAuthorNotCurrentUser
	{
		get
		{
			if (PublishedFileId == PublishedFileId_t.Invalid)
			{
				return false;
			}
			if (steamAuthor == CSteamID.Nil)
			{
				return true;
			}
			return steamAuthor != SteamUser.GetSteamID();
		}
	}

	public WorkshopItemHook(WorkshopUploadable owner)
	{
		this.owner = owner;
		if (owner.GetPublishedFileId() != PublishedFileId_t.Invalid)
		{
			SendSteamDetailsQuery();
		}
	}

	public void PrepareForWorkshopUpload()
	{
		owner.PrepareForWorkshopUpload();
	}

	private void SendSteamDetailsQuery()
	{
		if (SteamManager.Initialized)
		{
			SteamAPICall_t hAPICall = SteamUGC.RequestUGCDetails(PublishedFileId, 999999u);
			queryResult = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(OnDetailsQueryReturned);
			queryResult.Set(hAPICall);
		}
	}

	private void OnDetailsQueryReturned(SteamUGCRequestUGCDetailsResult_t result, bool IOFailure)
	{
		steamAuthor = (CSteamID)result.m_details.m_ulSteamIDOwner;
	}
}
