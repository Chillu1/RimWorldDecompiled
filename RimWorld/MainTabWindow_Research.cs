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
		protected ResearchProjectDef selectedProject;

		private bool requiredByThisFound;

		private Vector2 leftScrollPosition = Vector2.zero;

		private float leftScrollViewHeight;

		private Vector2 rightScrollPosition;

		private float rightViewWidth;

		private float rightViewHeight;

		private ResearchTabDef curTabInt;

		private bool editMode;

		private List<ResearchProjectDef> draggingTabs = new List<ResearchProjectDef>();

		private List<TabRecord> tabs = new List<TabRecord>();

		private const float leftAreaWidthPercent = 0.22f;

		private const float LeftAreaWidthMin = 200f;

		private const int ModeSelectButHeight = 40;

		private const float ProjectTitleHeight = 50f;

		private const float ProjectTitleLeftMargin = 0f;

		private const int ResearchItemW = 140;

		private const int ResearchItemH = 50;

		private const int ResearchItemPaddingW = 50;

		private const int ResearchItemPaddingH = 50;

		private const int ColumnMaxProjects = 6;

		private const float LineOffsetFactor = 0.48f;

		private const float IndentSpacing = 6f;

		private const float RowHeight = 24f;

		private const KeyCode SelectMultipleKey = KeyCode.LeftShift;

		private const KeyCode DeselectKey = KeyCode.LeftControl;

		private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

		private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = ColoredText.RedReadable;

		private static readonly Color ProjectWithMissingPrerequisiteLabelColor = Color.gray;

		private static Dictionary<string, string> labelsWithNewlineCached = new Dictionary<string, string>();

		private static Dictionary<Pair<int, int>, string> techprintsInfoCached = new Dictionary<Pair<int, int>, string>();

		private static List<Building> tmpAllBuildings = new List<Building>();

		private ResearchTabDef CurTab
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
					Vector2 vector = ViewSize(CurTab);
					rightViewWidth = vector.x;
					rightViewHeight = vector.y;
					rightScrollPosition = Vector2.zero;
				}
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

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 initialSize = base.InitialSize;
				float b = UI.screenHeight - 35;
				float b2 = Margin + 10f + 32f + 10f + DefDatabase<ResearchTabDef>.AllDefs.Max((ResearchTabDef tab) => ViewSize(tab).y) + 10f + 10f + Margin;
				float a = Mathf.Max(initialSize.y, b2);
				initialSize.y = Mathf.Min(a, b);
				return initialSize;
			}
		}

		private Vector2 ViewSize(ResearchTabDef tab)
		{
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			float num = 0f;
			float num2 = 0f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ResearchProjectDef researchProjectDef = allDefsListForReading[i];
				if (researchProjectDef.tab == tab)
				{
					Rect rect = new Rect(0f, 0f, 140f, 0f);
					Widgets.LabelCacheHeight(ref rect, GetLabel(researchProjectDef) + "\n", renderLabel: false);
					num = Mathf.Max(num, PosX(researchProjectDef) + 140f);
					num2 = Mathf.Max(num2, PosY(researchProjectDef) + rect.height);
				}
			}
			return new Vector2(num + 20f, num2 + 20f);
		}

		public override void PreOpen()
		{
			base.PreOpen();
			selectedProject = Find.ResearchManager.currentProj;
			if (CurTab == null)
			{
				if (selectedProject != null)
				{
					CurTab = selectedProject.tab;
				}
				else
				{
					CurTab = ResearchTabDefOf.Main;
				}
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			windowRect.width = UI.screenWidth;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			float width = Mathf.Max(200f, inRect.width * 0.22f);
			Rect leftOutRect = new Rect(0f, 0f, width, inRect.height);
			Rect rightOutRect = new Rect(leftOutRect.xMax + 10f, 0f, inRect.width - leftOutRect.width - 10f, inRect.height);
			DrawLeftRect(leftOutRect);
			DrawRightRect(rightOutRect);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			Rect position = leftOutRect;
			GUI.BeginGroup(position);
			if (selectedProject != null)
			{
				Rect outRect = new Rect(0f, 0f, position.width, 520f);
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, leftScrollViewHeight);
				Widgets.BeginScrollView(outRect, ref leftScrollPosition, viewRect);
				float num = 0f;
				Text.Font = GameFont.Medium;
				GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
				Rect rect = new Rect(0f, num, viewRect.width - 0f, 50f);
				Widgets.LabelCacheHeight(ref rect, selectedProject.LabelCap);
				GenUI.ResetLabelAlign();
				Text.Font = GameFont.Small;
				num += rect.height;
				Rect rect2 = new Rect(0f, num, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect2, selectedProject.description);
				num += rect2.height;
				Rect rect3 = new Rect(0f, num, viewRect.width, 500f);
				num += DrawTechprintInfo(rect3, selectedProject);
				if ((int)selectedProject.techLevel > (int)Faction.OfPlayer.def.techLevel)
				{
					float num2 = selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
					Rect rect4 = new Rect(0f, num, viewRect.width, 0f);
					string text = "TechLevelTooLow".Translate(Faction.OfPlayer.def.techLevel.ToStringHuman(), selectedProject.techLevel.ToStringHuman(), num2.ToStringPercent());
					if (num2 != 1f)
					{
						text += " " + "ResearchCostComparison".Translate(selectedProject.baseCost.ToString("F0"), selectedProject.CostApparent.ToString("F0"));
					}
					Widgets.LabelCacheHeight(ref rect4, text);
					num += rect4.height;
				}
				if (!ColonistsHaveResearchBench)
				{
					GUI.color = ColoredText.RedReadable;
					Rect rect5 = new Rect(0f, num, viewRect.width, 0f);
					Widgets.LabelCacheHeight(ref rect5, "CannotResearchNoBench".Translate());
					num += rect5.height;
					GUI.color = Color.white;
				}
				num += DrawResearchPrereqs(rect: new Rect(0f, num, viewRect.width, 500f), project: selectedProject);
				num += DrawResearchBenchRequirements(rect: new Rect(0f, num, viewRect.width, 500f), project: selectedProject);
				Rect rect8 = new Rect(0f, num, viewRect.width, 500f);
				num += DrawUnlockableHyperlinks(rect8, selectedProject);
				num = (leftScrollViewHeight = num + 3f);
				Widgets.EndScrollView();
				Rect rect9 = new Rect(0f, outRect.yMax + 10f, position.width, 68f);
				if (selectedProject.CanStartNow && selectedProject != Find.ResearchManager.currentProj)
				{
					if (Widgets.ButtonText(rect9, "Research".Translate()))
					{
						SoundDefOf.ResearchStart.PlayOneShotOnCamera();
						Find.ResearchManager.currentProj = selectedProject;
						TutorSystem.Notify_Event("StartResearchProject");
						if (!ColonistsHaveResearchBench)
						{
							Messages.Message("MessageResearchMenuWithoutBench".Translate(), MessageTypeDefOf.CautionInput);
						}
					}
				}
				else
				{
					string text2 = "";
					if (selectedProject.IsFinished)
					{
						text2 = "Finished".Translate();
						Text.Anchor = TextAnchor.MiddleCenter;
					}
					else if (selectedProject == Find.ResearchManager.currentProj)
					{
						text2 = "InProgress".Translate();
						Text.Anchor = TextAnchor.MiddleCenter;
					}
					else
					{
						text2 = "Locked".Translate() + ":";
						if (!selectedProject.PrerequisitesCompleted)
						{
							text2 += "\n  " + "PrerequisitesNotCompleted".Translate();
						}
						if (!selectedProject.TechprintRequirementMet)
						{
							text2 += "\n  " + "InsufficientTechprintsApplied".Translate(selectedProject.TechprintsApplied, selectedProject.techprintCount);
						}
					}
					Widgets.DrawHighlight(rect9);
					Widgets.Label(rect9.ContractedBy(5f), text2);
					Text.Anchor = TextAnchor.UpperLeft;
				}
				Rect rect10 = new Rect(0f, rect9.yMax + 10f, position.width, 35f);
				Widgets.FillableBar(rect10, selectedProject.ProgressPercent, ResearchBarFillTex, ResearchBarBGTex, doBorder: true);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect10, selectedProject.ProgressApparent.ToString("F0") + " / " + selectedProject.CostApparent.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
				if (Prefs.DevMode && selectedProject != Find.ResearchManager.currentProj && !selectedProject.IsFinished && Widgets.ButtonText(new Rect(rect9.x, rect9.y - 30f, 120f, 30f), "Debug: Finish now"))
				{
					Find.ResearchManager.currentProj = selectedProject;
					Find.ResearchManager.FinishProject(selectedProject);
				}
				if (Prefs.DevMode && !selectedProject.TechprintRequirementMet && Widgets.ButtonText(new Rect(rect9.x + 120f, rect9.y - 30f, 120f, 30f), "Debug: Apply techprint"))
				{
					Find.ResearchManager.ApplyTechprint(selectedProject, null);
				}
			}
			GUI.EndGroup();
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
			foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				ResearchTabDef localTabDef = allDef;
				tabs.Add(new TabRecord(localTabDef.LabelCap, delegate
				{
					CurTab = localTabDef;
				}, () => CurTab == localTabDef));
			}
		}

		private void DrawRightRect(Rect rightOutRect)
		{
			rightOutRect.yMin += 32f;
			Widgets.DrawMenuSection(rightOutRect);
			TabDrawer.DrawTabs(rightOutRect, tabs);
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
					foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef def) => def.Debug_IsPositionModified()))
					{
						stringBuilder.AppendLine(item.defName);
						stringBuilder.AppendLine(string.Format("  <researchViewX>{0}</researchViewX>", item.ResearchViewX.ToString("F2")));
						stringBuilder.AppendLine(string.Format("  <researchViewY>{0}</researchViewY>", item.ResearchViewY.ToString("F2")));
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
			bool flag = false;
			Rect outRect = rightOutRect.ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rightViewWidth, rightViewHeight);
			rect2.ContractedBy(10f);
			rect2.width = rightViewWidth;
			Rect position = rect2.ContractedBy(10f);
			Vector2 start = default(Vector2);
			Vector2 end = default(Vector2);
			Widgets.ScrollHorizontal(outRect, ref rightScrollPosition, rect2);
			Widgets.BeginScrollView(outRect, ref rightScrollPosition, rect2);
			GUI.BeginGroup(position);
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					ResearchProjectDef researchProjectDef = allDefsListForReading[j];
					if (researchProjectDef.tab != CurTab)
					{
						continue;
					}
					start.x = PosX(researchProjectDef);
					start.y = PosY(researchProjectDef) + 25f;
					for (int k = 0; k < researchProjectDef.prerequisites.CountAllowNull(); k++)
					{
						ResearchProjectDef researchProjectDef2 = researchProjectDef.prerequisites[k];
						if (researchProjectDef2 == null || researchProjectDef2.tab != CurTab)
						{
							continue;
						}
						end.x = PosX(researchProjectDef2) + 140f;
						end.y = PosY(researchProjectDef2) + 25f;
						if (selectedProject == researchProjectDef || selectedProject == researchProjectDef2)
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
			Rect other = new Rect(rightScrollPosition.x, rightScrollPosition.y, outRect.width, outRect.height).ExpandedBy(10f);
			for (int l = 0; l < allDefsListForReading.Count; l++)
			{
				ResearchProjectDef researchProjectDef3 = allDefsListForReading[l];
				if (researchProjectDef3.tab != CurTab)
				{
					continue;
				}
				Rect source = new Rect(PosX(researchProjectDef3), PosY(researchProjectDef3), 140f, 50f);
				Rect rect3 = new Rect(source);
				string label = GetLabel(researchProjectDef3);
				Widgets.LabelCacheHeight(ref rect3, GetLabelWithNewlineCached(label));
				if (!rect3.Overlaps(other))
				{
					continue;
				}
				Color color = Widgets.NormalOptionColor;
				Color bgColor = default(Color);
				Color color2 = default(Color);
				bool flag2 = !researchProjectDef3.IsFinished && !researchProjectDef3.CanStartNow;
				if (researchProjectDef3 == Find.ResearchManager.currentProj)
				{
					bgColor = TexUI.ActiveResearchColor;
				}
				else if (researchProjectDef3.IsFinished)
				{
					bgColor = TexUI.FinishedResearchColor;
				}
				else if (flag2)
				{
					bgColor = TexUI.LockedResearchColor;
				}
				else if (researchProjectDef3.CanStartNow)
				{
					bgColor = TexUI.AvailResearchColor;
				}
				if (editMode && draggingTabs.Contains(researchProjectDef3))
				{
					color2 = Color.yellow;
				}
				else if (selectedProject == researchProjectDef3)
				{
					bgColor += TexUI.HighlightBgResearchColor;
					color2 = TexUI.HighlightBorderResearchColor;
				}
				else
				{
					color2 = TexUI.DefaultBorderResearchColor;
				}
				if (flag2)
				{
					color = ProjectWithMissingPrerequisiteLabelColor;
				}
				if (selectedProject != null)
				{
					if ((researchProjectDef3.prerequisites != null && researchProjectDef3.prerequisites.Contains(selectedProject)) || (researchProjectDef3.hiddenPrerequisites != null && researchProjectDef3.hiddenPrerequisites.Contains(selectedProject)))
					{
						color2 = TexUI.HighlightLineResearchColor;
					}
					if (!researchProjectDef3.IsFinished && ((selectedProject.prerequisites != null && selectedProject.prerequisites.Contains(researchProjectDef3)) || (selectedProject.hiddenPrerequisites != null && selectedProject.hiddenPrerequisites.Contains(researchProjectDef3))))
					{
						color2 = TexUI.DependencyOutlineResearchColor;
					}
				}
				if (requiredByThisFound)
				{
					for (int m = 0; m < researchProjectDef3.requiredByThis.CountAllowNull(); m++)
					{
						ResearchProjectDef researchProjectDef4 = researchProjectDef3.requiredByThis[m];
						if (selectedProject == researchProjectDef4)
						{
							color2 = TexUI.HighlightLineResearchColor;
						}
					}
				}
				Rect rect4 = rect3;
				Widgets.LabelCacheHeight(ref rect4, " ");
				if (Widgets.CustomButtonText(ref rect3, "", bgColor, color, color2))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					selectedProject = researchProjectDef3;
				}
				rect4.y = rect3.y + rect3.height - rect4.height;
				Rect rect5 = rect4;
				rect5.x += 10f;
				rect5.width = rect5.width / 2f - 10f;
				Rect rect6 = rect5;
				rect6.x += rect5.width;
				TextAnchor anchor = Text.Anchor;
				Color color3 = GUI.color;
				GUI.color = color;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect3, label);
				GUI.color = color;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect5, researchProjectDef3.CostApparent.ToString());
				if (researchProjectDef3.techprintCount > 0)
				{
					GUI.color = (researchProjectDef3.TechprintRequirementMet ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect6, GetTechprintsInfoCached(researchProjectDef3.TechprintsApplied, researchProjectDef3.techprintCount));
				}
				GUI.color = color3;
				Text.Anchor = anchor;
				if (!editMode || !Mouse.IsOver(rect3) || !Input.GetMouseButtonDown(0))
				{
					continue;
				}
				flag = true;
				if (Input.GetKey(KeyCode.LeftShift))
				{
					if (!draggingTabs.Contains(researchProjectDef3))
					{
						draggingTabs.Add(researchProjectDef3);
					}
				}
				else if (!Input.GetKey(KeyCode.LeftControl) && !draggingTabs.Contains(researchProjectDef3))
				{
					draggingTabs.Clear();
					draggingTabs.Add(researchProjectDef3);
				}
				if (Input.GetKey(KeyCode.LeftControl) && draggingTabs.Contains(researchProjectDef3))
				{
					draggingTabs.Remove(researchProjectDef3);
				}
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
			if (!editMode)
			{
				return;
			}
			if (!flag && Input.GetMouseButtonDown(0))
			{
				draggingTabs.Clear();
			}
			if (draggingTabs.NullOrEmpty())
			{
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				for (int n = 0; n < draggingTabs.Count; n++)
				{
					draggingTabs[n].Debug_SnapPositionData();
				}
			}
			else if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && Event.current.type == EventType.Layout)
			{
				for (int num = 0; num < draggingTabs.Count; num++)
				{
					draggingTabs[num].Debug_ApplyPositionDelta(new Vector2(PixelsToCoordX(Event.current.delta.x), PixelsToCoordY(Event.current.delta.y)));
				}
			}
		}

		private float DrawResearchPrereqs(ResearchProjectDef project, Rect rect)
		{
			if (project.prerequisites.NullOrEmpty())
			{
				return 0f;
			}
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
			rect.yMin += rect.height;
			rect.xMin += 6f;
			for (int i = 0; i < project.prerequisites.Count; i++)
			{
				SetPrerequisiteStatusColor(project.prerequisites[i].IsFinished, project);
				Widgets.LabelCacheHeight(ref rect, project.prerequisites[i].LabelCap);
				rect.yMin += rect.height;
			}
			if (project.hiddenPrerequisites != null)
			{
				for (int j = 0; j < project.hiddenPrerequisites.Count; j++)
				{
					SetPrerequisiteStatusColor(project.hiddenPrerequisites[j].IsFinished, project);
					Widgets.LabelCacheHeight(ref rect, project.hiddenPrerequisites[j].LabelCap);
					rect.yMin += rect.height;
				}
			}
			GUI.color = Color.white;
			rect.xMin = xMin;
			return rect.yMin - yMin;
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
				techprintsInfoCached.Add(key, $"{applied.ToString()} / {total.ToString()}");
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
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.allBuildingsColonist.Find((Building x) => x.def == project.requiredResearchBuilding) != null)
					{
						present = true;
						break;
					}
				}
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":");
				rect.xMin += 6f;
				rect.yMin += rect.height;
				SetPrerequisiteStatusColor(present, project);
				Widgets.LabelCacheHeight(ref rect, project.requiredResearchBuilding.LabelCap);
				rect.yMin += rect.height;
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
				for (int j = 0; j < project.requiredResearchFacilities.Count; j++)
				{
					DrawResearchBenchFacilityRequirement(project.requiredResearchFacilities[j], bestMatchingBench, project, ref rect);
					rect.yMin += rect.height;
				}
			}
			GUI.color = Color.white;
			rect.xMin = xMin;
			return rect.yMin - yMin;
		}

		private float DrawUnlockableHyperlinks(Rect rect, ResearchProjectDef project)
		{
			List<Dialog_InfoCard.Hyperlink> infoCardHyperlinks = project.InfoCardHyperlinks;
			if (infoCardHyperlinks.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			Widgets.LabelCacheHeight(ref rect, "Unlocks".Translate() + ":");
			rect.x += 6f;
			rect.yMin += rect.height;
			for (int i = 0; i < infoCardHyperlinks.Count; i++)
			{
				Widgets.HyperlinkWithIcon(new Rect(rect.x, rect.yMin, rect.width, 24f), infoCardHyperlinks[i]);
				rect.yMin += 24f;
			}
			return rect.yMin - yMin;
		}

		private float DrawTechprintInfo(Rect rect, ResearchProjectDef project)
		{
			if (selectedProject.techprintCount == 0)
			{
				return 0f;
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
							Rect rect2 = new Rect(rect.x, yMin + num, rect.width, Text.CalcHeight(name, rect.width));
							Widgets.Label(rect2, name);
							num += rect2.height;
						}
					}
				}
			}
			rect.xMin = xMin;
			return num;
		}

		private string GetLabel(ResearchProjectDef r)
		{
			return r.LabelCap;
		}

		private void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
		{
			if (!project.IsFinished)
			{
				if (present)
				{
					GUI.color = FulfilledPrerequisiteColor;
				}
				else
				{
					GUI.color = MissingPrerequisiteColor;
				}
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
			Widgets.LabelCacheHeight(ref rect, text);
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
				Building_ResearchBench building_ResearchBench2 = tmpAllBuildings[j] as Building_ResearchBench;
				if (building_ResearchBench2 != null && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
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
	}
}
