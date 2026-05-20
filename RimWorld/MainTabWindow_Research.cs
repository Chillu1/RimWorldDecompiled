using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Research : MainTabWindow
	{
		private class ResearchTabRecord : TabRecord
		{
			public readonly ResearchTabDef def;

			public ResearchProjectDef firstMatch;

			private string cachedTip;

			public bool AnyMatches => firstMatch != null;

			public override string TutorTag => def.tutorTag;

			public ResearchTabRecord(ResearchTabDef def, string label, Action clickedAction, Func<bool> selected)
				: base(label, clickedAction, selected)
			{
				this.def = def;
			}

			public void Reset()
			{
				firstMatch = null;
				labelColor = null;
			}

			public override string GetTip()
			{
				if (cachedTip == null)
				{
					cachedTip = def.generalTitle.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) ?? "";
					if (!def.generalDescription.NullOrEmpty())
					{
						cachedTip = cachedTip + "\n" + def.generalDescription;
					}
				}
				return cachedTip;
			}
		}

		protected ResearchProjectDef selectedProject;

		private ScrollPositioner scrollPositioner = new ScrollPositioner();

		private Vector2 leftScrollPosition = Vector2.zero;

		private float leftScrollViewHeight;

		private Vector2 rightScrollPosition;

		private float rightViewWidth;

		private ResearchTabDef curTabInt;

		private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

		private bool editMode;

		private List<ResearchProjectDef> draggingTabs = new List<ResearchProjectDef>();

		private List<ResearchTabRecord> tabs = new List<ResearchTabRecord>();

		private List<ResearchProjectDef> cachedVisibleResearchProjects;

		private Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>> cachedUnlockedDefsGroupedByPrerequisites;

		private readonly HashSet<ResearchProjectDef> matchingProjects = new HashSet<ResearchProjectDef>();

		private const float leftAreaWidthPercent = 0.22f;

		private const float LeftAreaWidthMin = 270f;

		private const float ProjectTitleHeight = 50f;

		private const float ProjectTitleLeftMargin = 0f;

		private const int ResearchItemW = 140;

		private const int ResearchItemH = 50;

		private const int ResearchItemPaddingW = 50;

		private const int ResearchItemPaddingH = 50;

		private const int CategoryRectW = 14;

		private const float IndentSpacing = 6f;

		private const float RowHeight = 24f;

		private const float LeftStartButHeight = 55f;

		private const float SearchBoxHeight = 24f;

		private const float SearchBoxWidth = 200f;

		private const float MinTabWidth = 100f;

		private const float MaxTabWidth = 200f;

		private const int SearchHighlightMargin = 4;

		private const KeyCode SelectMultipleKey = KeyCode.LeftShift;

		private const KeyCode DeselectKey = KeyCode.LeftControl;

		private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(TexUI.ResearchMainTabColor);

		private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		private static readonly CachedTexture BasicBackgroundTex = new CachedTexture("UI/AnomalyResearchCategoryMarkers/AnomalyResearchBackground_Basic");

		private static readonly CachedTexture AdvancedBackgroundTex = new CachedTexture("UI/AnomalyResearchCategoryMarkers/AnomalyResearchBackground_Advanced");

		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = ColorLibrary.RedReadable;

		private static readonly Color ProjectWithMissingPrerequisiteLabelColor = Color.gray;

		private static readonly Color HiddenProjectLabelColor = Color.gray;

		private static readonly Color ActiveProjectLabelColor = new ColorInt(219, 201, 126, 255).ToColor;

		private static readonly Color NoMatchTintColor = Widgets.MenuSectionBGFillColor;

		private const float NoMatchTintFactor = 0.4f;

		private static readonly CachedTexture TechprintRequirementTex = new CachedTexture("UI/Icons/Research/Techprint");

		private static readonly CachedTexture StudyRequirementTex = new CachedTexture("UI/Icons/Study");

		private static List<string> lockedReasons = new List<string>();

		private List<(BuildableDef, List<string>)> cachedDefsWithMissingMemes = new List<(BuildableDef, List<string>)>();

		private static Dictionary<string, string> labelsWithNewlineCached = new Dictionary<string, string>();

		private static Dictionary<Pair<int, int>, string> techprintsInfoCached = new Dictionary<Pair<int, int>, string>();

		private List<string> tmpSuffixesForUnlocked = new List<string>();

		private static List<Building> tmpAllBuildings = new List<Building>();

		public ResearchTabDef CurTab
		{
			get
			{
				return curTabInt;
			}
			set
			{
				if (value != curTabInt)
				{
					curTabInt = value;
					rightViewWidth = ViewSize(CurTab).x;
					rightScrollPosition = Vector2.zero;
				}
			}
		}

		private ResearchTabRecord CurTabRecord
		{
			get
			{
				foreach (ResearchTabRecord tab in tabs)
				{
					if (tab.def == CurTab)
					{
						return tab;
					}
				}
				return null;
			}
		}

		private bool ColonistsHaveResearchBench
		{
			get
			{
				bool result = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.ColonistsHaveResearchBench())
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public List<ResearchProjectDef> VisibleResearchProjects
		{
			get
			{
				if (cachedVisibleResearchProjects == null)
				{
					cachedVisibleResearchProjects = new List<ResearchProjectDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef d) => Find.Storyteller.difficulty.AllowedBy(d.hideWhen) || Find.ResearchManager.IsCurrentProject(d)));
				}
				return cachedVisibleResearchProjects;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 initialSize = base.InitialSize;
				float b = UI.screenHeight - 35;
				float num = 0f;
				foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
				{
					num = Mathf.Max(num, ViewSize(allDef).y);
				}
				float num2 = Mathf.Max(270f, (float)UI.screenWidth * 0.22f);
				float overflowTabHeight = TabDrawer.GetOverflowTabHeight(new Rect(num2, 0f, (float)UI.screenWidth - num2 - 200f - 4f, 0f), tabs, 100f, 200f);
				float b2 = Margin + 10f + overflowTabHeight + 10f + num + 10f + 10f + Margin;
				float a = Mathf.Max(initialSize.y, b2);
				initialSize.y = Mathf.Min(a, b);
				return initialSize;
			}
		}

		private Vector2 ViewSize(ResearchTabDef tab)
		{
			List<ResearchProjectDef> visibleResearchProjects = VisibleResearchProjects;
			float num = 0f;
			float num2 = 0f;
			Text.Font = GameFont.Small;
			float num3 = 0f;
			if (ModsConfig.AnomalyActive && tab == ResearchTabDefOf.Anomaly)
			{
				num3 = 14f;
			}
			for (int i = 0; i < visibleResearchProjects.Count; i++)
			{
				ResearchProjectDef researchProjectDef = visibleResearchProjects[i];
				if (researchProjectDef.tab == tab)
				{
					Rect rect = new Rect(0f, 0f, 140f, 0f);
					Widgets.LabelCacheHeight(ref rect, GetLabelWithNewlineCached(GetLabel(researchProjectDef)), renderLabel: false);
					num = Mathf.Max(num, PosX(researchProjectDef) + 140f + num3);
					num2 = Mathf.Max(num2, PosY(researchProjectDef) + rect.height);
				}
			}
			return new Vector2(num + 20f + 4f, num2 + 20f + 4f);
		}

		public override void PreOpen()
		{
			base.PreOpen();
			UpdateSelectedProject(Find.ResearchManager);
			scrollPositioner.Arm();
			cachedVisibleResearchProjects = null;
			cachedUnlockedDefsGroupedByPrerequisites = null;
			quickSearchWidget.Reset();
			if (CurTab == null)
			{
				CurTab = ((selectedProject != null) ? selectedProject.tab : ResearchTabDefOf.Main);
			}
			UpdateSearchResults();
		}

		public void Select(ResearchProjectDef project)
		{
			CurTab = project.tab;
			selectedProject = project;
		}

		private void UpdateSelectedProject(ResearchManager researchManager)
		{
			if (ModsConfig.AnomalyActive && curTabInt == ResearchTabDefOf.Anomaly)
			{
				selectedProject = null;
				{
					foreach (ResearchManager.KnowledgeCategoryProject currentAnomalyKnowledgeProject in researchManager.CurrentAnomalyKnowledgeProjects)
					{
						if (currentAnomalyKnowledgeProject.project != null)
						{
							selectedProject = currentAnomalyKnowledgeProject.project;
							break;
						}
					}
					return;
				}
			}
			selectedProject = researchManager.GetProject();
		}

		public override void DoWindowContents(Rect inRect)
		{
			windowRect.width = UI.screenWidth;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			float width = Mathf.Max(270f, inRect.width * 0.22f);
			Rect leftOutRect = new Rect(0f, 0f, width, inRect.height);
			Rect searchRect = new Rect(inRect.xMax - 200f, 0f, 200f, 24f);
			Rect rightOutRect = new Rect(leftOutRect.xMax + 10f, 0f, inRect.width - leftOutRect.width - 10f, inRect.height);
			DrawSearchRect(searchRect);
			DrawLeftRect(leftOutRect);
			DrawRightRect(rightOutRect, searchRect.x - 4f);
		}

		private void DrawSearchRect(Rect searchRect)
		{
			quickSearchWidget.OnGUI(searchRect, UpdateSearchResults);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			Widgets.BeginGroup(leftOutRect);
			if (selectedProject != null)
			{
				DrawProjectInfo(leftOutRect);
			}
			else
			{
				DrawCurrentTabInfo(leftOutRect);
			}
			Widgets.EndGroup();
		}

		private void DrawCurrentTabInfo(Rect rect)
		{
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, leftScrollViewHeight);
			Widgets.BeginScrollView(outRect, ref leftScrollPosition, viewRect);
			float num = 0f;
			Text.Font = GameFont.Medium;
			GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
			Rect rect2 = new Rect(0f, num, viewRect.width - 0f, 50f);
			Widgets.LabelCacheHeight(ref rect2, curTabInt.generalTitle.CapitalizeFirst());
			GenUI.ResetLabelAlign();
			Text.Font = GameFont.Small;
			num += rect2.height;
			Rect rect3 = new Rect(0f, num, viewRect.width, 0f);
			Widgets.LabelCacheHeight(ref rect3, curTabInt.generalDescription);
			num += rect3.height;
			leftScrollViewHeight = num;
			Widgets.EndScrollView();
		}

		private void DrawProjectInfo(Rect rect)
		{
			int num = ((!ModsConfig.AnomalyActive || curTabInt != ResearchTabDefOf.Anomaly) ? 1 : 2);
			float num2 = ((num > 1) ? (75f * (float)num) : 100f);
			Rect rect2 = rect;
			rect2.yMin = rect.yMax - num2;
			rect2.yMax = rect.yMax;
			Rect rect3 = rect2;
			Rect rect4 = rect2;
			rect4.y = rect2.y - 30f;
			rect4.height = 28f;
			rect2 = rect2.ContractedBy(10f);
			rect2.y += 5f;
			Text.Font = GameFont.Medium;
			string key = ((num > 1) ? "ActiveProjectPlural" : "ActiveProject");
			Widgets.Label(rect4, key.Translate());
			Text.Font = GameFont.Small;
			Rect startButRect = new Rect
			{
				y = rect4.y - 55f - 10f,
				height = 55f,
				x = rect.center.x - rect.width / 4f,
				width = rect.width / 2f + 20f
			};
			Widgets.DrawMenuSection(rect3);
			if (ModsConfig.AnomalyActive && curTabInt == ResearchTabDefOf.Anomaly)
			{
				Rect rect5 = rect2;
				rect5.height = rect2.height / 2f;
				Rect rect6 = rect5;
				rect6.yMin = rect2.yMax - rect5.height;
				rect6.yMax = rect2.yMax;
				ResearchProjectDef project = Find.ResearchManager.GetProject(KnowledgeCategoryDefOf.Basic);
				ResearchProjectDef project2 = Find.ResearchManager.GetProject(KnowledgeCategoryDefOf.Advanced);
				if (project == null && project2 == null)
				{
					using (new TextBlock(TextAnchor.MiddleCenter))
					{
						Widgets.Label(rect2, "NoProjectSelected".Translate());
					}
				}
				else
				{
					float prefixWidth = DefDatabase<KnowledgeCategoryDef>.AllDefs.Max((KnowledgeCategoryDef x) => Text.CalcSize(x.LabelCap + ":").x);
					DrawProjectProgress(rect5, project, KnowledgeCategoryDefOf.Basic.LabelCap, prefixWidth);
					DrawProjectProgress(rect6, project2, KnowledgeCategoryDefOf.Advanced.LabelCap, prefixWidth);
				}
			}
			else
			{
				ResearchProjectDef project3 = Find.ResearchManager.GetProject();
				if (project3 == null)
				{
					using (new TextBlock(TextAnchor.MiddleCenter))
					{
						Widgets.Label(rect2, "NoProjectSelected".Translate());
					}
				}
				else
				{
					DrawProjectProgress(rect2, project3);
				}
			}
			DrawStartButton(startButRect);
			if (Prefs.DevMode && !Find.ResearchManager.IsCurrentProject(selectedProject) && !selectedProject.IsFinished)
			{
				Text.Font = GameFont.Tiny;
				if (Widgets.ButtonText(new Rect(rect.xMax - 120f, rect4.y, 120f, 25f), "Debug: Finish now"))
				{
					Find.ResearchManager.SetCurrentProject(selectedProject);
					Find.ResearchManager.FinishProject(selectedProject);
				}
				Text.Font = GameFont.Small;
			}
			if (Prefs.DevMode && !selectedProject.TechprintRequirementMet)
			{
				Text.Font = GameFont.Tiny;
				if (Widgets.ButtonText(new Rect(rect.xMax - 300f, rect4.y, 170f, 25f), "Debug: Apply techprint"))
				{
					Find.ResearchManager.ApplyTechprint(selectedProject, null);
					SoundDefOf.TechprintApplied.PlayOneShotOnCamera();
				}
				Text.Font = GameFont.Small;
			}
			float y = 0f;
			DrawProjectPrimaryInfo(rect, ref y);
			Rect rect7 = new Rect(0f, y, rect.width, 0f);
			rect7.yMax = startButRect.yMin - 10f;
			DrawProjectScrollView(rect7);
		}

		private void DrawProjectPrimaryInfo(Rect rect, ref float y)
		{
			using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft))
			{
				Widgets.Label(new Rect(0f, y, rect.width - 0f, 50f), ref y, selectedProject.LabelCap);
			}
			y += 10f;
			string text = selectedProject.Description;
			if (ModsConfig.AnomalyActive && selectedProject.knowledgeCategory != null)
			{
				text = text + "\n\n" + "AnomalyResearchDescriptionHelpText".Translate().Colorize(ColoredText.SubtleGrayColor);
			}
			Widgets.Label(0f, ref y, rect.width, text);
			y += 10f;
			Widgets.DrawLineHorizontal(rect.x - 8f, y, rect.width, Color.gray);
			y += 10f;
			if (ModsConfig.AnomalyActive && selectedProject.knowledgeCategory != null)
			{
				Widgets.Label(0f, ref y, rect.width, "KnowledgeCategory".Translate() + ": " + selectedProject.knowledgeCategory.LabelCap);
			}
			Rect rect2 = new Rect(0f, y, rect.width, 500f);
			DrawTechprintInfo(rect2, ref y);
		}

		private void DrawProjectScrollView(Rect rect)
		{
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, leftScrollViewHeight);
			float y = 3f;
			Widgets.BeginScrollView(rect, ref leftScrollPosition, viewRect);
			if ((int)selectedProject.techLevel > (int)Faction.OfPlayer.def.techLevel)
			{
				float num = selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
				Rect rect2 = new Rect(0f, y, viewRect.width, 0f);
				string text = "TechLevelTooLow".Translate(Faction.OfPlayer.def.techLevel.ToStringHuman(), selectedProject.techLevel.ToStringHuman(), (1f / num).ToStringPercent());
				if (num != 1f)
				{
					text += " " + "ResearchCostComparison".Translate(selectedProject.Cost.ToString("F0"), selectedProject.CostApparent.ToString("F0"));
				}
				Widgets.LabelCacheHeight(ref rect2, text);
				y += rect2.height;
			}
			Rect rect3 = new Rect(0f, y, viewRect.width, 0f);
			DrawResearchPrerequisites(rect3, ref y, selectedProject);
			Rect rect4 = new Rect(0f, y, viewRect.width, 500f);
			y += DrawResearchBenchRequirements(selectedProject, rect4);
			y += DrawStudyRequirements(rect: new Rect(0f, y, viewRect.width, 500f), project: selectedProject);
			y += DrawInspectionRequirements(rect: new Rect(0f, y, viewRect.width, 500f), project: selectedProject);
			Rect rect5 = new Rect(0f, y, viewRect.width, 500f);
			Rect visibleRect = new Rect(0f, leftScrollPosition.y, viewRect.width, rect.height);
			y += DrawUnlockableHyperlinks(rect5, visibleRect, selectedProject);
			y += DrawCustomUnlockables(new Rect(0f, y, viewRect.width, 500f), selectedProject);
			Rect rect6 = new Rect(0f, y, viewRect.width, 500f);
			y += DrawContentSource(rect6, selectedProject);
			y += 3f;
			leftScrollViewHeight = y;
			Widgets.EndScrollView();
		}

		private void DrawStartButton(Rect startButRect)
		{
			if (selectedProject.CanStartNow && !Find.ResearchManager.IsCurrentProject(selectedProject))
			{
				if (Widgets.ButtonText(startButRect, "Research".Translate()))
				{
					AttemptBeginResearch(selectedProject);
				}
				return;
			}
			if (Find.ResearchManager.IsCurrentProject(selectedProject))
			{
				if (Widgets.ButtonText(startButRect, "StopResearch".Translate()))
				{
					Find.ResearchManager.StopProject(selectedProject);
				}
				return;
			}
			lockedReasons.Clear();
			string text;
			if (selectedProject.IsFinished)
			{
				text = "Finished".Translate();
				Text.Anchor = TextAnchor.MiddleCenter;
			}
			else if (Find.ResearchManager.IsCurrentProject(selectedProject))
			{
				text = "InProgress".Translate();
				Text.Anchor = TextAnchor.MiddleCenter;
			}
			else
			{
				if (!selectedProject.PrerequisitesCompleted)
				{
					lockedReasons.Add("PrerequisitesNotCompleted".Translate());
				}
				if (!selectedProject.TechprintRequirementMet)
				{
					lockedReasons.Add("InsufficientTechprintsApplied".Translate(selectedProject.TechprintsApplied, selectedProject.TechprintCount));
				}
				if ((!ModsConfig.AnomalyActive || curTabInt != ResearchTabDefOf.Anomaly) && !selectedProject.PlayerHasAnyAppropriateResearchBench)
				{
					lockedReasons.Add("MissingRequiredResearchFacilities".Translate());
				}
				if (!selectedProject.PlayerMechanitorRequirementMet)
				{
					lockedReasons.Add("MissingRequiredMechanitor".Translate());
				}
				if (!selectedProject.InspectionRequirementsMet)
				{
					lockedReasons.Add("MissingGravEngineInspection".Translate());
				}
				if (!selectedProject.AnalyzedThingsRequirementsMet)
				{
					for (int i = 0; i < selectedProject.requiredAnalyzed.Count; i++)
					{
						lockedReasons.Add("NotStudied".Translate(selectedProject.requiredAnalyzed[i].LabelCap));
					}
				}
				if (lockedReasons.NullOrEmpty())
				{
					Log.ErrorOnce("Research " + selectedProject.defName + " locked but no reasons given", selectedProject.GetHashCode() ^ 0x5FE2BD1);
				}
				text = "Locked".Translate();
			}
			Widgets.DrawHighlight(startButRect);
			startButRect = startButRect.ContractedBy(4f);
			string text2 = text;
			if (!lockedReasons.NullOrEmpty())
			{
				text2 = text2 + ":\n" + lockedReasons.ToLineList("  ");
			}
			Vector2 vector = Text.CalcSize(text2);
			if (vector.x > startButRect.width || vector.y > startButRect.height)
			{
				TooltipHandler.TipRegion(startButRect.ExpandedBy(4f), text2);
				Text.Anchor = TextAnchor.MiddleCenter;
			}
			else
			{
				text = text2;
			}
			Widgets.Label(startButRect, text);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawProjectProgress(Rect rect, ResearchProjectDef project, string prefixTitle = null, float prefixWidth = 75f)
		{
			Rect rect2 = rect;
			if (!string.IsNullOrEmpty(prefixTitle))
			{
				Rect rect3 = rect2;
				rect3.width = prefixWidth;
				rect2.xMin = rect3.xMax + 10f;
				using (new TextBlock(TextAnchor.MiddleLeft))
				{
					Widgets.Label(rect3, prefixTitle + ":");
				}
			}
			if (project == null)
			{
				using (new TextBlock(TextAnchor.MiddleCenter))
				{
					Widgets.Label(rect2, "NoProjectSelected".Translate());
					return;
				}
			}
			rect2 = rect2.ContractedBy(15f);
			Widgets.FillableBar(rect2, project.ProgressPercent, ResearchBarFillTex, ResearchBarBGTex, doBorder: true);
			Text.Anchor = TextAnchor.MiddleCenter;
			string label = project.ProgressApparentString + " / " + project.CostApparent.ToString("F0");
			Widgets.Label(rect2, label);
			Rect rect4 = rect2;
			rect4.y = rect2.y - 22f;
			rect4.height = 22f;
			float x = Text.CalcSize(project.LabelCap).x;
			Widgets.Label(rect4, project.LabelCap.Truncate(rect4.width));
			if (x > rect4.width)
			{
				TooltipHandler.TipRegion(rect4, project.LabelCap);
				Widgets.DrawHighlightIfMouseover(rect4);
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void AttemptBeginResearch(ResearchProjectDef projectToStart)
		{
			List<(BuildableDef, List<string>)> list = ComputeUnlockedDefsThatHaveMissingMemes(projectToStart);
			if (!list.Any())
			{
				DoBeginResearch(projectToStart);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ResearchProjectHasDefsWithMissingMemes".Translate(projectToStart.LabelCap)).Append(":");
			stringBuilder.AppendLine();
			foreach (var (buildableDef, items) in list)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("  - ").Append(buildableDef.LabelCap.Colorize(ColoredText.NameColor)).Append(" (")
					.Append(items.ToCommaList())
					.Append(")");
			}
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), delegate
			{
				DoBeginResearch(projectToStart);
			}));
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		private List<(BuildableDef, List<string>)> ComputeUnlockedDefsThatHaveMissingMemes(ResearchProjectDef project)
		{
			cachedDefsWithMissingMemes.Clear();
			if (!ModsConfig.IdeologyActive)
			{
				return cachedDefsWithMissingMemes;
			}
			if (Faction.OfPlayer.ideos?.PrimaryIdeo == null)
			{
				return cachedDefsWithMissingMemes;
			}
			foreach (Def unlockedDef in project.UnlockedDefs)
			{
				if (!(unlockedDef is BuildableDef { canGenerateDefaultDesignator: false } buildableDef))
				{
					continue;
				}
				List<string> list = null;
				foreach (MemeDef item in DefDatabase<MemeDef>.AllDefsListForReading)
				{
					if (!Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(item) && item.AllDesignatorBuildables.Contains(buildableDef))
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(item.LabelCap);
					}
				}
				if (list != null)
				{
					cachedDefsWithMissingMemes.Add((buildableDef, list));
				}
			}
			return cachedDefsWithMissingMemes;
		}

		private void DoBeginResearch(ResearchProjectDef projectToStart)
		{
			SoundDefOf.ResearchStart.PlayOneShotOnCamera();
			Find.ResearchManager.SetCurrentProject(projectToStart);
			TutorSystem.Notify_Event("StartResearchProject");
			if ((!ModsConfig.AnomalyActive || projectToStart.knowledgeCategory == null) && !ColonistsHaveResearchBench)
			{
				Messages.Message("MessageResearchMenuWithoutBench".Translate(), MessageTypeDefOf.CautionInput);
			}
		}

		private float CoordToPixelsX(float x)
		{
			return x * 190f;
		}

		private float CoordToPixelsY(float y)
		{
			return y * 100f;
		}

		private float PixelsToCoordX(float x)
		{
			return x / 190f;
		}

		private float PixelsToCoordY(float y)
		{
			return y / 100f;
		}

		private float PosX(ResearchProjectDef d)
		{
			return CoordToPixelsX(d.ResearchViewX);
		}

		private float PosY(ResearchProjectDef d)
		{
			return CoordToPixelsY(d.ResearchViewY);
		}

		public override void PostOpen()
		{
			base.PostOpen();
			tabs.Clear();
			foreach (ResearchTabDef tabDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				tabs.Add(new ResearchTabRecord(tabDef, tabDef.LabelCap, delegate
				{
					CurTab = tabDef;
					UpdateSelectedProject(Find.ResearchManager);
				}, () => CurTab == tabDef));
			}
		}

		private void DrawRightRect(Rect rightOutRect, float maxTabX)
		{
			Rect baseRect = rightOutRect;
			baseRect.xMax = maxTabX;
			rightOutRect.yMin += TabDrawer.GetOverflowTabHeight(baseRect, tabs, 100f, 200f);
			Widgets.DrawMenuSection(rightOutRect);
			TabDrawer.DrawTabsOverflow(baseRect, tabs, 100f, 200f);
			if (Prefs.DevMode)
			{
				Rect rect = rightOutRect;
				rect.yMax = rect.yMin + 20f;
				rect.xMin = rect.xMax - 80f;
				Rect butRect = rect.RightPartPixels(30f);
				rect = rect.LeftPartPixels(rect.width - 30f);
				Widgets.CheckboxLabeled(rect, "Edit", ref editMode);
				if (Widgets.ButtonImageFitted(butRect, TexButton.Copy))
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (ResearchProjectDef item in VisibleResearchProjects.Where((ResearchProjectDef def) => def.Debug_IsPositionModified()))
					{
						stringBuilder.AppendLine(item.defName);
						stringBuilder.AppendLine($"  <researchViewX>{item.ResearchViewX:F2}</researchViewX>");
						stringBuilder.AppendLine($"  <researchViewY>{item.ResearchViewY:F2}</researchViewY>");
						stringBuilder.AppendLine();
					}
					GUIUtility.systemCopyBuffer = stringBuilder.ToString();
					Messages.Message("Modified data copied to clipboard.", MessageTypeDefOf.SituationResolved, historical: false);
				}
			}
			else
			{
				editMode = false;
			}
			bool elementClicked = false;
			Rect rect2 = rightOutRect.ContractedBy(1f);
			Rect rect3 = new Rect(0f, 0f, Mathf.Max(rightViewWidth + 20f, rect2.width), rightOutRect.height - 16f);
			Rect rect4 = rect3.ContractedBy(20f);
			Rect rect5 = rect3;
			rect5.height = rightOutRect.height;
			scrollPositioner.ClearInterestRects();
			if (Find.ResearchManager.TabInfoVisible(CurTab))
			{
				Widgets.ScrollHorizontal(rect2, ref rightScrollPosition, rect3);
				Widgets.BeginScrollView(rect2, ref rightScrollPosition, rect3);
				if (CurTab == ResearchTabDefOf.Anomaly)
				{
					Color color = GUI.color;
					GUI.color = new Color(1f, 1f, 1f, 0.15f);
					GUI.DrawTexture(rect5.LeftPartPixels(830f), BasicBackgroundTex.Texture, ScaleMode.StretchToFill);
					GUI.DrawTexture(rect5.RightPartPixels(rect5.width - 830f + 1f), AdvancedBackgroundTex.Texture, ScaleMode.StretchToFill);
					GUI.color = color;
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.AnomalyResearch, OpportunityType.GoodToKnow);
				}
				Widgets.BeginGroup(rect4);
				ListProjects(rect4, ref elementClicked);
				Widgets.EndGroup();
				Widgets.EndScrollView();
				scrollPositioner.ScrollHorizontally(ref rightScrollPosition, rect2.size);
				if (!editMode)
				{
					return;
				}
				if (!elementClicked && Input.GetMouseButtonDown(0))
				{
					draggingTabs.Clear();
				}
				if (draggingTabs.NullOrEmpty())
				{
					return;
				}
				if (Input.GetMouseButtonUp(0))
				{
					for (int num = 0; num < draggingTabs.Count; num++)
					{
						draggingTabs[num].Debug_SnapPositionData();
					}
				}
				else if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && Event.current.type == EventType.Layout)
				{
					for (int num2 = 0; num2 < draggingTabs.Count; num2++)
					{
						draggingTabs[num2].Debug_ApplyPositionDelta(new Vector2(PixelsToCoordX(Event.current.delta.x), PixelsToCoordY(Event.current.delta.y)));
					}
				}
			}
			else
			{
				Widgets.BeginGroup(rect2);
				using (new TextBlock(TextAnchor.MiddleCenter, ColoredText.SubtleGrayColor))
				{
					Widgets.Label(rect2.AtZero(), "ResearchNotDiscovered".Translate());
				}
				Widgets.EndGroup();
			}
		}

		private void ListProjects(Rect rightInRect, ref bool elementClicked)
		{
			List<ResearchProjectDef> visibleResearchProjects = VisibleResearchProjects;
			Vector2 start = default(Vector2);
			Vector2 end = default(Vector2);
			for (int i = 0; i < 2; i++)
			{
				foreach (ResearchProjectDef item in visibleResearchProjects)
				{
					if (item.tab != CurTab)
					{
						continue;
					}
					float num = 0f;
					if (ModsConfig.AnomalyActive && item.knowledgeCategory != null)
					{
						num = 14f;
					}
					start.x = PosX(item);
					start.y = PosY(item) + 25f;
					for (int j = 0; j < item.prerequisites.CountAllowNull(); j++)
					{
						ResearchProjectDef researchProjectDef = item.prerequisites[j];
						if (researchProjectDef == null || researchProjectDef.tab != CurTab)
						{
							continue;
						}
						end.x = PosX(researchProjectDef) + 140f + num;
						end.y = PosY(researchProjectDef) + 25f;
						if (selectedProject == item || selectedProject == researchProjectDef)
						{
							if (i == 1)
							{
								Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
							}
						}
						else if (i == 0)
						{
							Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
						}
					}
				}
			}
			Rect other = new Rect(rightScrollPosition.x, rightScrollPosition.y, rightInRect.width, rightInRect.height).ExpandedBy(10f);
			foreach (ResearchProjectDef project in visibleResearchProjects)
			{
				if (project.tab != CurTab)
				{
					continue;
				}
				float num2 = 0f;
				if (ModsConfig.AnomalyActive && project.knowledgeCategory != null)
				{
					num2 = 14f;
				}
				Rect rect = new Rect(PosX(project), PosY(project), 140f, 50f);
				rect.xMax += num2;
				string label = GetLabel(project);
				Rect rect2 = rect;
				rect2.xMin += num2;
				Widgets.LabelCacheHeight(ref rect2, label);
				Rect rect3 = rect2;
				rect3.y = rect2.yMax - 4f;
				Widgets.LabelCacheHeight(ref rect3, " ");
				rect.yMax = rect3.yMax;
				bool flag = quickSearchWidget.filter.Active && !matchingProjects.Contains(project);
				bool flag2 = quickSearchWidget.filter.Active && matchingProjects.Contains(project);
				if (flag2 || selectedProject == project)
				{
					scrollPositioner.RegisterInterestRect(rect);
				}
				if (project.IsHidden)
				{
					label = string.Format("({0})", "UnknownResearch".Translate());
				}
				if (!rect.Overlaps(other))
				{
					continue;
				}
				Color color = Widgets.NormalOptionColor;
				Color color2 = TexUI.OtherActiveResearchColor;
				Color windowBGFillColor = Widgets.WindowBGFillColor;
				Color color3 = default(Color);
				bool flag3 = !project.IsFinished && !project.CanStartNow;
				bool flag4 = false;
				if (project.IsHidden)
				{
					color2 = TexUI.HiddenResearchColor;
					color = HiddenProjectLabelColor;
				}
				else if (Find.ResearchManager.IsCurrentProject(project))
				{
					color2 = TexUI.ActiveResearchColor;
					color = ActiveProjectLabelColor;
				}
				else if (project.IsFinished)
				{
					color2 = TexUI.FinishedResearchColor;
				}
				else if (flag3)
				{
					color2 = TexUI.LockedResearchColor;
				}
				if (flag3)
				{
					color = ProjectWithMissingPrerequisiteLabelColor;
				}
				if (editMode && draggingTabs.Contains(project))
				{
					color3 = Color.yellow;
				}
				else if (selectedProject == project)
				{
					color2 += TexUI.HighlightBgResearchColor;
					color3 = TexUI.BorderResearchSelectedColor;
					color = TexUI.BorderResearchSelectedColor;
					flag4 = true;
				}
				else if (Find.ResearchManager.IsCurrentProject(project))
				{
					color3 = TexUI.BorderResearchingColor;
					flag4 = true;
				}
				else
				{
					color3 = TexUI.DefaultBorderResearchColor;
				}
				if (selectedProject != null)
				{
					if (project.prerequisites.NotNullAndContains(selectedProject) || project.hiddenPrerequisites.NotNullAndContains(selectedProject))
					{
						color3 = TexUI.HighlightLineResearchColor;
					}
					if (selectedProject.prerequisites.NotNullAndContains(project) || selectedProject.hiddenPrerequisites.NotNullAndContains(project))
					{
						color3 = (project.IsFinished ? TexUI.HighlightLineResearchColor : TexUI.DependencyOutlineResearchColor);
					}
				}
				Color color4 = (project.TechprintRequirementMet ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
				Color color5 = (project.AnalyzedThingsRequirementsMet ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
				if (flag)
				{
					color = NoMatchTint(color);
					color2 = NoMatchTint(color2);
					color3 = NoMatchTint(color3);
					color4 = NoMatchTint(color4);
					color5 = NoMatchTint(color5);
				}
				if (flag2)
				{
					Widgets.DrawStrongHighlight(rect.ExpandedBy(4f));
				}
				int num3 = ((!flag4) ? 1 : 2);
				if (Widgets.CustomButtonText(ref rect, "", color2, color, color3, windowBGFillColor, cacheHeight: false, num3, doMouseoverSound: true, active: true, project.ProgressPercent) && !project.IsHidden)
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					selectedProject = project;
				}
				Color color6 = GUI.color;
				TextAnchor anchor = Text.Anchor;
				if (ModsConfig.AnomalyActive && project.knowledgeCategory != null)
				{
					Rect position = rect;
					position.x += 4f;
					position.width = 14f;
					GUI.color = project.knowledgeCategory.color;
					GUI.DrawTexture(position, project.knowledgeCategory.Tex, ScaleMode.ScaleToFit);
				}
				GUI.color = color;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect2, label);
				if (!project.IsHidden)
				{
					DrawBottomRow(rect3, project, color4, color5);
					if (Mouse.IsOver(rect) && !editMode)
					{
						Widgets.DrawLightHighlight(rect);
						TooltipHandler.TipRegion(rect, () => project.GetTip(), project.GetHashCode() ^ 0x1664F);
					}
				}
				GUI.color = color6;
				Text.Anchor = anchor;
				if (!editMode || !Mouse.IsOver(rect2) || !Input.GetMouseButtonDown(0))
				{
					continue;
				}
				elementClicked = true;
				if (Input.GetKey(KeyCode.LeftShift))
				{
					if (!draggingTabs.Contains(project))
					{
						draggingTabs.Add(project);
					}
				}
				else if (!Input.GetKey(KeyCode.LeftControl) && !draggingTabs.Contains(project))
				{
					draggingTabs.Clear();
					draggingTabs.Add(project);
				}
				if (Input.GetKey(KeyCode.LeftControl) && draggingTabs.Contains(project))
				{
					draggingTabs.Remove(project);
				}
			}
		}

		private void DrawBottomRow(Rect rect, ResearchProjectDef project, Color techprintColor, Color studiedColor)
		{
			Color color = GUI.color;
			TextAnchor anchor = Text.Anchor;
			int num = 1;
			if (project.TechprintCount > 0)
			{
				num++;
			}
			if (project.RequiredAnalyzedThingCount > 0)
			{
				num++;
			}
			float num2 = rect.width / (float)num;
			Rect rect2 = rect;
			rect2.x = rect.x;
			rect2.width = num2;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect2, project.CostApparent.ToString());
			rect2.x += num2;
			if (project.TechprintCount > 0)
			{
				string text = GetTechprintsInfoCached(project.TechprintsApplied, project.TechprintCount);
				Vector2 vector = Text.CalcSize(text);
				Rect rect3 = rect2;
				rect3.xMin = rect2.xMax - vector.x - 10f;
				Rect rect4 = rect2;
				rect4.width = rect4.height;
				rect4.x = rect3.x - rect4.width;
				GUI.color = techprintColor;
				Widgets.Label(rect3, text);
				GUI.color = Color.white;
				GUI.DrawTexture(rect4.ContractedBy(4f), TechprintRequirementTex.Texture);
				rect2.x += num2;
			}
			if (project.RequiredAnalyzedThingCount > 0)
			{
				string text2 = GetTechprintsInfoCached(project.AnalyzedThingsCompleted, project.RequiredAnalyzedThingCount);
				Vector2 vector2 = Text.CalcSize(text2);
				Rect rect5 = rect2;
				rect5.xMin = rect2.xMax - vector2.x - 10f;
				Rect rect6 = rect2;
				rect6.width = rect6.height;
				rect6.x = rect5.x - rect6.width;
				GUI.color = studiedColor;
				Widgets.Label(rect5, text2);
				GUI.color = Color.white;
				GUI.DrawTexture(rect6.ContractedBy(4f), StudyRequirementTex.Texture);
			}
			GUI.color = color;
			Text.Anchor = anchor;
		}

		private Color NoMatchTint(Color color)
		{
			return Color.Lerp(color, NoMatchTintColor, 0.4f);
		}

		private void DrawResearchPrerequisites(Rect rect, ref float y, ResearchProjectDef project)
		{
			if (project.prerequisites.NullOrEmpty() && project.hiddenPrerequisites.NullOrEmpty())
			{
				return;
			}
			Widgets.Label(rect, ref y, string.Format("{0}:", "Prerequisites".Translate()));
			rect.xMin += 6f;
			foreach (ResearchProjectDef item in project.prerequisites.OrElseEmptyEnumerable())
			{
				SetPrerequisiteStatusColor(item.IsFinished, project);
				Widgets.Label(rect, ref y, $"- {item.LabelCap}");
			}
			if (project.hiddenPrerequisites != null)
			{
				foreach (ResearchProjectDef hiddenPrerequisite in project.hiddenPrerequisites)
				{
					SetPrerequisiteStatusColor(hiddenPrerequisite.IsFinished, project);
					Widgets.Label(rect, ref y, $"- {hiddenPrerequisite.LabelCap}");
				}
			}
			GUI.color = Color.white;
		}

		private string GetLabelWithNewlineCached(string label)
		{
			if (!labelsWithNewlineCached.ContainsKey(label))
			{
				labelsWithNewlineCached.Add(label, label + "\n");
			}
			return labelsWithNewlineCached[label];
		}

		private string GetTechprintsInfoCached(int applied, int total)
		{
			Pair<int, int> key = new Pair<int, int>(applied, total);
			if (!techprintsInfoCached.ContainsKey(key))
			{
				techprintsInfoCached.Add(key, applied + " / " + total);
			}
			return techprintsInfoCached[key];
		}

		private float DrawResearchBenchRequirements(ResearchProjectDef project, Rect rect)
		{
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			if (project.requiredResearchBuilding != null)
			{
				bool present = false;
				foreach (Map map in Find.Maps)
				{
					if (map.listerBuildings.allBuildingsColonist.Find((Building x) => x.def == project.requiredResearchBuilding) != null)
					{
						present = true;
						break;
					}
				}
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":");
				rect.xMin += 6f;
				rect.yMin += rect.height;
				SetPrerequisiteStatusColor(present, project);
				rect.height = Text.CalcHeight(project.requiredResearchBuilding.LabelCap, rect.width - 24f - 6f);
				Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(project.requiredResearchBuilding));
				rect.yMin += rect.height + 4f;
				GUI.color = Color.white;
				rect.xMin = xMin;
			}
			if (!project.requiredResearchFacilities.NullOrEmpty())
			{
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBenchFacilities".Translate() + ":");
				rect.yMin += rect.height;
				Building_ResearchBench building_ResearchBench = FindBenchFulfillingMostRequirements(project.requiredResearchBuilding, project.requiredResearchFacilities);
				CompAffectedByFacilities bestMatchingBench = null;
				if (building_ResearchBench != null)
				{
					bestMatchingBench = building_ResearchBench.TryGetComp<CompAffectedByFacilities>();
				}
				rect.xMin += 6f;
				foreach (ThingDef requiredResearchFacility in project.requiredResearchFacilities)
				{
					DrawResearchBenchFacilityRequirement(requiredResearchFacility, bestMatchingBench, project, ref rect);
					rect.yMin += rect.height;
				}
				rect.yMin += 4f;
			}
			GUI.color = Color.white;
			rect.xMin = xMin;
			return rect.yMin - yMin;
		}

		private float DrawStudyRequirements(ResearchProjectDef project, Rect rect)
		{
			float yMin = rect.yMin;
			if (project.RequiredAnalyzedThingCount > 0)
			{
				Widgets.LabelCacheHeight(ref rect, "StudyRequirements".Translate() + ":");
				rect.xMin += 6f;
				rect.yMin += rect.height;
				foreach (ThingDef item in project.requiredAnalyzed)
				{
					Rect rect2 = new Rect(rect.x, rect.yMin, rect.width, 24f);
					Color? color = null;
					if (quickSearchWidget.filter.Active)
					{
						if (MatchesUnlockedDef(item))
						{
							Widgets.DrawTextHighlight(rect2);
						}
						else
						{
							color = NoMatchTint(Widgets.NormalOptionColor);
						}
					}
					Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(item);
					Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, LabelSuffixForUnlocked(item));
					rect.yMin += 24f;
				}
			}
			return rect.yMin - yMin;
		}

		private float DrawInspectionRequirements(ResearchProjectDef project, Rect rect)
		{
			float yMin = rect.yMin;
			if (ModsConfig.OdysseyActive && project.requireGravEngineInspected)
			{
				Widgets.LabelCacheHeight(ref rect, "RequiresInspectionof".Translate() + ":");
				rect.yMin += 24f;
				rect.xMin += 6f;
				Rect rect2 = new Rect(rect.x, rect.yMin, rect.width, 24f);
				Widgets.HyperlinkWithIcon(rect2, new Dialog_InfoCard.Hyperlink(ThingDefOf.GravEngine));
				rect.yMin += rect2.height;
			}
			return rect.yMin - yMin;
		}

		private float DrawUnlockableHyperlinks(Rect rect, Rect visibleRect, ResearchProjectDef project)
		{
			List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> list = UnlockedDefsGroupedByPrerequisites(project);
			if (list.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			float x = rect.x;
			foreach (Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>> item in list)
			{
				ResearchPrerequisitesUtility.UnlockedHeader first = item.First;
				rect.x = x;
				if (!first.unlockedBy.Any())
				{
					Widgets.LabelCacheHeight(ref rect, "Unlocks".Translate() + ":");
				}
				else
				{
					Widgets.LabelCacheHeight(ref rect, string.Concat("UnlockedWith".Translate(), " ", HeaderLabel(first), ":"));
				}
				rect.x += 6f;
				rect.yMin += rect.height;
				foreach (Def item2 in item.Second)
				{
					Rect rect2 = new Rect(rect.x, rect.yMin, rect.width, 24f);
					if (visibleRect.Overlaps(rect2))
					{
						Color? color = null;
						if (quickSearchWidget.filter.Active)
						{
							if (MatchesUnlockedDef(item2))
							{
								Widgets.DrawTextHighlight(rect2);
							}
							else
							{
								color = NoMatchTint(Widgets.NormalOptionColor);
							}
						}
						Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(item2);
						Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, LabelSuffixForUnlocked(item2));
					}
					rect.yMin += 24f;
				}
			}
			return rect.yMin - yMin;
		}

		private float DrawCustomUnlockables(Rect rect, ResearchProjectDef project)
		{
			if (project.customUnlockTexts.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			for (int i = 0; i < project.customUnlockTexts.Count; i++)
			{
				if (i == 0)
				{
					Widgets.LabelCacheHeight(ref rect, "Unlocks".Translate() + ":");
					rect.x += 6f;
					rect.yMin += rect.height;
				}
				Widgets.LabelCacheHeight(ref rect, project.customUnlockTexts[i]);
				rect.yMin += rect.height;
			}
			return rect.yMin - yMin;
		}

		private float DrawContentSource(Rect rect, ResearchProjectDef project)
		{
			if (project.modContentPack == null || project.modContentPack.IsCoreMod)
			{
				return 0f;
			}
			float yMin = rect.yMin;
			TaggedString taggedString = "Stat_Source_Label".Translate() + ":  " + project.modContentPack.Name;
			Widgets.LabelCacheHeight(ref rect, taggedString.Colorize(Color.grey));
			ExpansionDef expansionDef = ModLister.AllExpansions.Find((ExpansionDef e) => e.linkedMod == project.modContentPack.PackageId);
			if (expansionDef != null)
			{
				GUI.DrawTexture(new Rect(Text.CalcSize(taggedString).x + 4f, rect.y, 20f, 20f), expansionDef.IconFromStatus);
			}
			return rect.yMax - yMin;
		}

		private string LabelSuffixForUnlocked(Def unlocked)
		{
			if (!ModLister.IdeologyInstalled)
			{
				return null;
			}
			tmpSuffixesForUnlocked.Clear();
			foreach (MemeDef allDef in DefDatabase<MemeDef>.AllDefs)
			{
				if (allDef.AllDesignatorBuildables.Contains(unlocked))
				{
					tmpSuffixesForUnlocked.AddUnique(allDef.LabelCap);
				}
				if (allDef.thingStyleCategories.NullOrEmpty())
				{
					continue;
				}
				foreach (ThingStyleCategoryWithPriority thingStyleCategory in allDef.thingStyleCategories)
				{
					if (thingStyleCategory.category.AllDesignatorBuildables.Contains(unlocked))
					{
						tmpSuffixesForUnlocked.AddUnique(allDef.LabelCap);
					}
				}
			}
			foreach (CultureDef allDef2 in DefDatabase<CultureDef>.AllDefs)
			{
				if (allDef2.thingStyleCategories.NullOrEmpty())
				{
					continue;
				}
				foreach (ThingStyleCategoryWithPriority thingStyleCategory2 in allDef2.thingStyleCategories)
				{
					if (thingStyleCategory2.category.AllDesignatorBuildables.Contains(unlocked))
					{
						tmpSuffixesForUnlocked.AddUnique(allDef2.LabelCap);
					}
				}
			}
			if (!tmpSuffixesForUnlocked.Any())
			{
				return null;
			}
			return " (" + tmpSuffixesForUnlocked.ToCommaList() + ")";
		}

		private List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> UnlockedDefsGroupedByPrerequisites(ResearchProjectDef project)
		{
			if (cachedUnlockedDefsGroupedByPrerequisites == null)
			{
				cachedUnlockedDefsGroupedByPrerequisites = new Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>>();
			}
			if (!cachedUnlockedDefsGroupedByPrerequisites.TryGetValue(project, out var value))
			{
				value = ResearchPrerequisitesUtility.UnlockedDefsGroupedByPrerequisites(project);
				cachedUnlockedDefsGroupedByPrerequisites.Add(project, value);
			}
			return value;
		}

		private string HeaderLabel(ResearchPrerequisitesUtility.UnlockedHeader headerProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = "";
			for (int i = 0; i < headerProject.unlockedBy.Count; i++)
			{
				ResearchProjectDef researchProjectDef = headerProject.unlockedBy[i];
				string text = researchProjectDef.LabelCap;
				if (!researchProjectDef.IsFinished)
				{
					text = text.Colorize(MissingPrerequisiteColor);
				}
				stringBuilder.Append(value).Append(text);
				value = ", ";
			}
			return stringBuilder.ToString();
		}

		private void DrawTechprintInfo(Rect rect, ref float y)
		{
			if (selectedProject.TechprintCount == 0)
			{
				return;
			}
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			string text = "ResearchTechprintsFromFactions".Translate();
			float num = Text.CalcHeight(text, rect.width);
			Widgets.Label(new Rect(rect.x, yMin, rect.width, num), text);
			rect.x += 6f;
			if (selectedProject.heldByFactionCategoryTags != null)
			{
				foreach (string heldByFactionCategoryTag in selectedProject.heldByFactionCategoryTags)
				{
					foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
					{
						if (item.def.categoryTag == heldByFactionCategoryTag)
						{
							string name = item.Name;
							Rect rect2 = new Rect(rect.x, yMin + num, rect.width, Mathf.Max(24f, Text.CalcHeight(name, rect.width - 24f - 6f)));
							Widgets.BeginGroup(rect2);
							Rect r = new Rect(0f, 0f, 24f, 24f).ContractedBy(2f);
							FactionUIUtility.DrawFactionIconWithTooltip(r, item);
							Rect rect3 = new Rect(r.xMax + 6f, 0f, rect2.width - r.width - 6f, rect2.height);
							Text.Anchor = TextAnchor.MiddleLeft;
							Text.WordWrap = false;
							Widgets.Label(rect3, item.Name);
							Text.Anchor = TextAnchor.UpperLeft;
							Text.WordWrap = true;
							Widgets.EndGroup();
							num += rect2.height;
						}
					}
				}
			}
			rect.xMin = xMin;
			y += num;
		}

		private string GetLabel(ResearchProjectDef r)
		{
			return r.LabelCap;
		}

		private void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
		{
			if (!project.IsFinished)
			{
				GUI.color = (present ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
			}
		}

		private void DrawResearchBenchFacilityRequirement(ThingDef requiredFacility, CompAffectedByFacilities bestMatchingBench, ResearchProjectDef project, ref Rect rect)
		{
			Thing thing = null;
			Thing thing2 = null;
			if (bestMatchingBench != null)
			{
				thing = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility);
				thing2 = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility && bestMatchingBench.IsFacilityActive(x));
			}
			SetPrerequisiteStatusColor(thing2 != null, project);
			string text = requiredFacility.LabelCap;
			if (thing != null && thing2 == null)
			{
				text += " (" + "InactiveFacility".Translate() + ")";
			}
			rect.height = Text.CalcHeight(text, rect.width - 24f - 6f);
			Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(requiredFacility), text);
		}

		private Building_ResearchBench FindBenchFulfillingMostRequirements(ThingDef requiredResearchBench, List<ThingDef> requiredFacilities)
		{
			tmpAllBuildings.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				tmpAllBuildings.AddRange(maps[i].listerBuildings.allBuildingsColonist);
			}
			float num = 0f;
			Building_ResearchBench building_ResearchBench = null;
			for (int j = 0; j < tmpAllBuildings.Count; j++)
			{
				if (tmpAllBuildings[j] is Building_ResearchBench building_ResearchBench2 && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
				{
					float researchBenchRequirementsScore = GetResearchBenchRequirementsScore(building_ResearchBench2, requiredFacilities);
					if (building_ResearchBench == null || researchBenchRequirementsScore > num)
					{
						num = researchBenchRequirementsScore;
						building_ResearchBench = building_ResearchBench2;
					}
				}
			}
			tmpAllBuildings.Clear();
			return building_ResearchBench;
		}

		private float GetResearchBenchRequirementsScore(Building_ResearchBench bench, List<ThingDef> requiredFacilities)
		{
			float num = 0f;
			for (int i = 0; i < requiredFacilities.Count; i++)
			{
				CompAffectedByFacilities benchComp = bench.GetComp<CompAffectedByFacilities>();
				if (benchComp != null)
				{
					List<Thing> linkedFacilitiesListForReading = benchComp.LinkedFacilitiesListForReading;
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i] && benchComp.IsFacilityActive(x)) != null)
					{
						num += 1f;
					}
					else if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i]) != null)
					{
						num += 0.6f;
					}
				}
			}
			return num;
		}

		private void UpdateSearchResults()
		{
			quickSearchWidget.noResultsMatched = false;
			matchingProjects.Clear();
			foreach (ResearchTabRecord tab2 in tabs)
			{
				tab2.Reset();
			}
			if (!quickSearchWidget.filter.Active)
			{
				return;
			}
			foreach (ResearchProjectDef visibleResearchProject in VisibleResearchProjects)
			{
				if (!visibleResearchProject.IsHidden && (quickSearchWidget.filter.Matches(GetLabel(visibleResearchProject)) || MatchesUnlockedDefs(visibleResearchProject)))
				{
					matchingProjects.Add(visibleResearchProject);
				}
			}
			quickSearchWidget.noResultsMatched = !matchingProjects.Any();
			foreach (ResearchTabRecord tab in tabs)
			{
				tab.firstMatch = (from p in matchingProjects
					where tab.def == p.tab
					orderby p.ResearchViewX
					select p).FirstOrDefault();
				if (!tab.AnyMatches)
				{
					tab.labelColor = Color.grey;
				}
			}
			if (!CurTabRecord.AnyMatches)
			{
				foreach (ResearchTabRecord tab3 in tabs)
				{
					if (tab3.AnyMatches)
					{
						CurTab = tab3.def;
						break;
					}
				}
			}
			scrollPositioner.Arm();
			if (CurTabRecord.firstMatch != null)
			{
				selectedProject = CurTabRecord.firstMatch;
			}
			bool MatchesUnlockedDefs(ResearchProjectDef proj)
			{
				foreach (Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>> item in UnlockedDefsGroupedByPrerequisites(proj))
				{
					foreach (Def item2 in item.Second)
					{
						if (MatchesUnlockedDef(item2))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		private bool MatchesUnlockedDef(Def unlocked)
		{
			return quickSearchWidget.filter.Matches(unlocked.label);
		}

		public override void Notify_ClickOutsideWindow()
		{
			base.Notify_ClickOutsideWindow();
			quickSearchWidget.Unfocus();
		}
	}
}
