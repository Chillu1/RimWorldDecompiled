using System.IO;
using System.Linq;
using RimWorld;
using Steamworks;

namespace Verse.Steam;

public class WorkshopItem_Scenario : WorkshopItem
{
	private Scenario cachedScenario;

	public override PublishedFileId_t PublishedFileId
	{
		get
		{
			return base.PublishedFileId;
		}
		set
		{
			base.PublishedFileId = value;
			if (cachedScenario != null)
			{
				cachedScenario.SetPublishedFileId(value);
			}
		}
	}

	public Scenario GetScenario()
	{
		if (cachedScenario == null)
		{
			LoadScenario();
		}
		return cachedScenario;
	}

	private void LoadScenario()
	{
		if (GameDataSaveLoader.TryLoadScenario(base.Directory.GetFiles("*.rsc").First((FileInfo fi) => fi.Extension == ".rsc").FullName, ScenarioCategory.SteamWorkshop, out cachedScenario))
		{
			cachedScenario.SetPublishedFileId(PublishedFileId);
		}
	}
}
