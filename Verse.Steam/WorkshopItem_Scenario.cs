using RimWorld;
using Steamworks;
using System.IO;
using System.Linq;

namespace Verse.Steam
{
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
			if (GameDataSaveLoader.TryLoadScenario((from fi in base.Directory.GetFiles("*.rsc")
				where fi.Extension == ".rsc"
				select fi).First().FullName, ScenarioCategory.SteamWorkshop, out cachedScenario))
			{
				cachedScenario.SetPublishedFileId(PublishedFileId);
			}
		}
	}
}
