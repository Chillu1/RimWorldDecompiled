using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Steamworks;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public class Scenario : IExposable, WorkshopUploadable
	{
		[MustTranslate]
		public string name;

		[MustTranslate]
		public string summary;

		[MustTranslate]
		public string description;

		internal ScenPart_PlayerFaction playerFaction;

		internal List<ScenPart> parts = new List<ScenPart>();

		private PublishedFileId_t publishedFileIdInt = PublishedFileId_t.Invalid;

		private ScenarioCategory categoryInt;

		[NoTranslate]
		public string fileName;

		private WorkshopItemHook workshopHookInt;

		[NoTranslate]
		private string tempUploadDir;

		public bool enabled = true;

		public bool showInUI = true;

		public const int NameMaxLength = 55;

		public const int SummaryMaxLength = 300;

		public const int DescriptionMaxLength = 1000;

		public IEnumerable<System.Version> SupportedVersions
		{
			get
			{
				yield return new System.Version(VersionControl.CurrentMajor, VersionControl.CurrentMinor);
			}
		}

		public FileInfo File => new FileInfo(GenFilePaths.AbsPathForScenario(fileName));

		public IEnumerable<ScenPart> AllParts
		{
			get
			{
				yield return playerFaction;
				for (int i = 0; i < parts.Count; i++)
				{
					yield return parts[i];
				}
			}
		}

		public ScenarioCategory Category
		{
			get
			{
				if (categoryInt == ScenarioCategory.Undefined)
				{
					Log.Error("Category is Undefined on Scenario " + this);
				}
				return categoryInt;
			}
			set
			{
				categoryInt = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref summary, "summary");
			Scribe_Values.Look(ref description, "description");
			Scribe_Values.Look(ref publishedFileIdInt, "publishedFileId", PublishedFileId_t.Invalid);
			Scribe_Deep.Look(ref playerFaction, "playerFaction");
			Scribe_Collections.Look(ref parts, "parts", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (parts.RemoveAll((ScenPart x) => x == null) != 0)
				{
					Log.Warning("Some ScenParts were null after loading.");
				}
				if (parts.RemoveAll((ScenPart x) => x.HasNullDefs()) != 0)
				{
					Log.Warning("Some ScenParts had null defs.");
				}
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (name.NullOrEmpty())
			{
				yield return "no title";
			}
			if (parts.NullOrEmpty())
			{
				yield return "no parts";
			}
			if (playerFaction == null)
			{
				yield return "no playerFaction";
			}
			foreach (ScenPart allPart in AllParts)
			{
				foreach (string item in allPart.ConfigErrors())
				{
					yield return item;
				}
			}
		}

		public string GetFullInformationText()
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(description);
				stringBuilder.AppendLine();
				foreach (ScenPart allPart in AllParts)
				{
					allPart.summarized = false;
				}
				foreach (ScenPart item in from p in AllParts
					orderby p.def.summaryPriority descending, p.def.defName
					where p.visible
					select p)
				{
					string text = item.Summary(this);
					if (!text.NullOrEmpty())
					{
						stringBuilder.AppendLine(text);
					}
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
			catch (Exception ex)
			{
				Log.ErrorOnce("Exception in Scenario.GetFullInformationText():\n" + ex.ToString(), 10395878);
				return "Cannot read data.";
			}
		}

		public string GetSummary()
		{
			return summary;
		}

		public Scenario CopyForEditing()
		{
			Scenario scenario = new Scenario();
			scenario.name = name;
			scenario.summary = summary;
			scenario.description = description;
			scenario.playerFaction = (ScenPart_PlayerFaction)playerFaction.CopyForEditing();
			scenario.parts.AddRange(parts.Select((ScenPart p) => p.CopyForEditing()));
			scenario.categoryInt = ScenarioCategory.CustomLocal;
			return scenario;
		}

		public void PreConfigure()
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.PreConfigure();
			}
		}

		public Page GetFirstConfigPage()
		{
			List<Page> list = new List<Page>();
			list.Add(new Page_SelectStoryteller());
			list.Add(new Page_CreateWorldParams());
			list.Add(new Page_SelectStartingSite());
			foreach (Page item in parts.SelectMany((ScenPart p) => p.GetConfigPages()))
			{
				list.Add(item);
			}
			Page page = PageUtility.StitchedPages(list);
			if (page != null)
			{
				Page page2 = page;
				while (page2.next != null)
				{
					page2 = page2.next;
				}
				page2.nextAct = delegate
				{
					PageUtility.InitGameStart();
				};
			}
			return page;
		}

		public bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			foreach (ScenPart allPart in AllParts)
			{
				if (!allPart.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
				{
					return false;
				}
			}
			return true;
		}

		public void Notify_NewPawnGenerating(Pawn pawn, PawnGenerationContext context)
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.Notify_NewPawnGenerating(pawn, context);
			}
		}

		public void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed)
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.Notify_PawnGenerated(pawn, context, redressed);
			}
		}

		public void Notify_PawnDied(Corpse corpse)
		{
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].Notify_PawnDied(corpse);
			}
		}

		public void PostWorldGenerate()
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.PostWorldGenerate();
			}
		}

		public void PreMapGenerate()
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.PreMapGenerate();
			}
		}

		public void GenerateIntoMap(Map map)
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.GenerateIntoMap(map);
			}
		}

		public void PostMapGenerate(Map map)
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.PostMapGenerate(map);
			}
		}

		public void PostGameStart()
		{
			foreach (ScenPart allPart in AllParts)
			{
				allPart.PostGameStart();
			}
		}

		public float GetStatFactor(StatDef stat)
		{
			float num = 1f;
			for (int i = 0; i < parts.Count; i++)
			{
				ScenPart_StatFactor scenPart_StatFactor = parts[i] as ScenPart_StatFactor;
				if (scenPart_StatFactor != null)
				{
					num *= scenPart_StatFactor.GetStatFactor(stat);
				}
			}
			return num;
		}

		public void TickScenario()
		{
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].Tick();
			}
		}

		public void RemovePart(ScenPart part)
		{
			if (!parts.Contains(part))
			{
				Log.Error("Cannot remove: " + part);
			}
			parts.Remove(part);
		}

		public bool CanReorder(ScenPart part, ReorderDirection dir)
		{
			if (!part.def.PlayerAddRemovable)
			{
				return false;
			}
			int num = parts.IndexOf(part);
			switch (dir)
			{
			case ReorderDirection.Up:
				if (num == 0)
				{
					return false;
				}
				if (num > 0 && !parts[num - 1].def.PlayerAddRemovable)
				{
					return false;
				}
				return true;
			case ReorderDirection.Down:
				return num != parts.Count - 1;
			default:
				throw new NotImplementedException();
			}
		}

		public void Reorder(ScenPart part, ReorderDirection dir)
		{
			int num = parts.IndexOf(part);
			parts.RemoveAt(num);
			if (dir == ReorderDirection.Up)
			{
				parts.Insert(num - 1, part);
			}
			if (dir == ReorderDirection.Down)
			{
				parts.Insert(num + 1, part);
			}
		}

		public bool CanToUploadToWorkshop()
		{
			if (Category == ScenarioCategory.FromDef)
			{
				return false;
			}
			if (!TryUploadReport().Accepted)
			{
				return false;
			}
			if (GetWorkshopItemHook().MayHaveAuthorNotCurrentUser)
			{
				return false;
			}
			return true;
		}

		public void PrepareForWorkshopUpload()
		{
			string path = name + Rand.RangeInclusive(100, 999);
			tempUploadDir = Path.Combine(GenFilePaths.TempFolderPath, path);
			DirectoryInfo directoryInfo = new DirectoryInfo(tempUploadDir);
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete();
			}
			directoryInfo.Create();
			string str = Path.Combine(tempUploadDir, name);
			str += ".rsc";
			GameDataSaveLoader.SaveScenario(this, str);
		}

		public AcceptanceReport TryUploadReport()
		{
			if (name == null || name.Length < 3 || summary == null || summary.Length < 3 || description == null || description.Length < 3)
			{
				return "TextFieldsMustBeFilled".TranslateSimple();
			}
			return AcceptanceReport.WasAccepted;
		}

		public PublishedFileId_t GetPublishedFileId()
		{
			return publishedFileIdInt;
		}

		public void SetPublishedFileId(PublishedFileId_t newPfid)
		{
			publishedFileIdInt = newPfid;
			if (Category == ScenarioCategory.CustomLocal && !fileName.NullOrEmpty())
			{
				GameDataSaveLoader.SaveScenario(this, GenFilePaths.AbsPathForScenario(fileName));
			}
		}

		public string GetWorkshopName()
		{
			return name;
		}

		public string GetWorkshopDescription()
		{
			return GetFullInformationText();
		}

		public string GetWorkshopPreviewImagePath()
		{
			return GenFilePaths.ScenarioPreviewImagePath;
		}

		public IList<string> GetWorkshopTags()
		{
			return new List<string>
			{
				"Scenario"
			};
		}

		public DirectoryInfo GetWorkshopUploadDirectory()
		{
			return new DirectoryInfo(tempUploadDir);
		}

		public WorkshopItemHook GetWorkshopItemHook()
		{
			if (workshopHookInt == null)
			{
				workshopHookInt = new WorkshopItemHook(this);
			}
			return workshopHookInt;
		}

		public override string ToString()
		{
			if (name.NullOrEmpty())
			{
				return "LabellessScenario";
			}
			return name;
		}

		public override int GetHashCode()
		{
			int num = 6126121;
			if (name != null)
			{
				num ^= name.GetHashCode();
			}
			if (summary != null)
			{
				num ^= summary.GetHashCode();
			}
			if (description != null)
			{
				num ^= description.GetHashCode();
			}
			return num ^ publishedFileIdInt.GetHashCode();
		}
	}
}
