using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	public class Page_ModsConfig : Page
	{
		public ModMetaData selectedMod;

		private Vector2 modListScrollPosition = Vector2.zero;

		private Vector2 modDescriptionScrollPosition = Vector2.zero;

		private int activeModsWhenOpenedHash = -1;

		private int activeModsHash = -1;

		private bool displayFullfilledRequirements;

		protected string filter = "";

		private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();

		private static List<string> modWarningsCached = new List<string>();

		private List<ModRequirement> visibleReqsCached = new List<ModRequirement>();

		private bool anyReqsCached;

		private bool anyReqsInfoToShowCached;

		private bool anyUnfulfilledReqsCached;

		private bool anyOrderingIssuesCached;

		private float modRequirementsHeightCached;

		private bool modsInListOrderDirty;

		private static List<ModMetaData> modsInListOrderCached = new List<ModMetaData>();

		private const float ModListAreaWidth = 350f;

		private const float ModsListButtonHeight = 30f;

		private const float ModsFolderButHeight = 30f;

		private const float ButtonsGap = 4f;

		private const float UploadRowHeight = 40f;

		private const float PreviewMaxHeight = 300f;

		private const float VersionWidth = 30f;

		private const float ModRowHeight = 26f;

		private const float RequirementBoxInnerOffset = 10f;

		private static readonly Color RequirementBoxOutlineColor = new Color(0.25f, 0.25f, 0.25f);

		private static readonly Color UnmetRequirementBoxOutlineColor = new Color(0.62f, 0.18f, 0.18f);

		private static readonly Color UnmetRequirementBoxBGColor = new Color(0.1f, 0.065f, 0.072f);

		private static readonly Color RequirementRowColor = new Color(0.13f, 0.13f, 0.13f);

		private static readonly Color UnmetRequirementRowColor = new Color(0.23f, 0.15f, 0.15f);

		private static readonly Color UnmetRequirementRowColorHighlighted = new Color(0.27f, 0.18f, 0.18f);

		private Dictionary<string, string> truncatedStringCache = new Dictionary<string, string>();

		public Page_ModsConfig()
		{
			doCloseButton = true;
			closeOnCancel = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			ModLister.RebuildModList();
			modsInListOrderDirty = true;
			selectedMod = ModsInListOrder().FirstOrDefault();
			activeModsWhenOpenedHash = ModLister.InstalledModsListHash(activeOnly: true);
			RecacheSelectedModRequirements();
		}

		private List<ModMetaData> ModsInListOrder()
		{
			if (modsInListOrderDirty)
			{
				modsInListOrderCached.Clear();
				modsInListOrderCached.AddRange(ModsConfig.ActiveModsInLoadOrder);
				modsInListOrderCached.AddRange(from x in ModLister.AllInstalledMods
					where !x.Active
					select x into m
					orderby m.VersionCompatible descending
					select m);
				modsInListOrderDirty = false;
			}
			return modsInListOrderCached;
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect mainRect = GetMainRect(rect, 0f, ignoreTitle: true);
			GUI.BeginGroup(mainRect);
			Text.Font = GameFont.Small;
			float num = 0f;
			if (Widgets.ButtonText(new Rect(17f, num, 316f, 30f), "OpenSteamWorkshop".Translate()))
			{
				SteamUtility.OpenSteamWorkshopPage();
			}
			num += 30f;
			if (Widgets.ButtonText(new Rect(17f, num, 316f, 30f), "GetModsFromForum".Translate()))
			{
				Application.OpenURL("http://rimworldgame.com/getmods");
			}
			num += 30f;
			num += 17f;
			filter = Widgets.TextField(new Rect(0f, num, 350f, 30f), filter);
			num += 30f;
			num += 10f;
			float num2 = 47f;
			Rect rect2 = new Rect(0f, num, 350f, mainRect.height - num - num2);
			Widgets.DrawMenuSection(rect2);
			float height = (float)ModLister.AllInstalledMods.Count() * 26f + 8f;
			Rect rect3 = new Rect(0f, 0f, rect2.width - 16f, height);
			Widgets.BeginScrollView(rect2, ref modListScrollPosition, rect3);
			Rect rect4 = rect3.ContractedBy(4f);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = rect4.width;
			float num3 = modListScrollPosition.y - 26f;
			float num4 = modListScrollPosition.y + rect2.height;
			listing_Standard.Begin(rect4);
			int num5 = ReorderableWidget.NewGroup(delegate(int from, int to)
			{
				ModsConfig.Reorder(from, to);
				modsInListOrderDirty = true;
			}, ReorderableDirection.Vertical);
			int num6 = 0;
			foreach (ModMetaData item in ModsInListOrder())
			{
				float num7 = (float)num6 * 26f;
				bool active = item.Active;
				Rect rect5 = new Rect(0f, (float)num6 * 26f, listing_Standard.ColumnWidth, 26f);
				if (active)
				{
					ReorderableWidget.Reorderable(num5, rect5);
				}
				if (num7 >= num3 && num7 <= num4)
				{
					DoModRow(rect5, item, num6, num5);
				}
				num6++;
			}
			int downloadingItemsCount = WorkshopItems.DownloadingItemsCount;
			for (int i = 0; i < downloadingItemsCount; i++)
			{
				DoModRowDownloading(listing_Standard, num6);
				num6++;
			}
			listing_Standard.End();
			Widgets.EndScrollView();
			num += rect2.height;
			num += 10f;
			if (Widgets.ButtonText(new Rect(17f, num, 316f, 30f), "ResolveModOrder".Translate()))
			{
				ModsConfig.TrySortMods();
				modsInListOrderDirty = true;
			}
			Rect position = new Rect(rect2.xMax + 17f, 0f, mainRect.width - rect2.width - 17f, mainRect.height);
			GUI.BeginGroup(position);
			if (selectedMod != null)
			{
				Text.Font = GameFont.Medium;
				Rect rect6 = new Rect(0f, 0f, position.width, 40f);
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect6, selectedMod.Name.Truncate(rect6.width));
				Text.Anchor = TextAnchor.UpperLeft;
				Rect position2 = new Rect(0f, rect6.yMax, 0f, 20f);
				if (selectedMod.PreviewImage != null)
				{
					position2.width = Mathf.Min(selectedMod.PreviewImage.width, position.width);
					position2.height = (float)selectedMod.PreviewImage.height * (position2.width / (float)selectedMod.PreviewImage.width);
					float num8 = Mathf.Ceil(position.height * 0.37f);
					if (position2.height > num8)
					{
						float height2 = position2.height;
						position2.height = num8;
						position2.width *= position2.height / height2;
					}
					if (position2.height > 300f)
					{
						position2.width *= 300f / position2.height;
						position2.height = 300f;
					}
					position2.x = position.width / 2f - position2.width / 2f;
					GUI.DrawTexture(position2, selectedMod.PreviewImage, ScaleMode.ScaleToFit);
				}
				float num9 = position2.yMax + 10f;
				Text.Font = GameFont.Small;
				float num10 = num9;
				if (!selectedMod.Author.NullOrEmpty())
				{
					Widgets.Label(new Rect(0f, num10, position.width / 2f, Text.LineHeight), "Author".Translate() + ": " + selectedMod.Author);
					num10 += Text.LineHeight;
				}
				if (!selectedMod.PackageId.NullOrEmpty())
				{
					GUI.color = Color.gray;
					Widgets.Label(new Rect(0f, num10, position.width / 2f, Text.LineHeight), "ModPackageId".Translate() + ": " + selectedMod.PackageIdPlayerFacing);
					num10 += Text.LineHeight;
					GUI.color = Color.white;
				}
				float num11 = num9;
				WidgetRow widgetRow = new WidgetRow(position.width, num11, UIDirection.LeftThenUp);
				if (SteamManager.Initialized && selectedMod.OnSteamWorkshop)
				{
					if (widgetRow.ButtonText("Unsubscribe".Translate()))
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(selectedMod.Name), delegate
						{
							selectedMod.enabled = false;
							Workshop.Unsubscribe(selectedMod);
							Notify_SteamItemUnsubscribed(selectedMod.GetPublishedFileId());
						}, destructive: true));
					}
					if (widgetRow.ButtonText("WorkshopPage".Translate()))
					{
						SteamUtility.OpenWorkshopPage(selectedMod.GetPublishedFileId());
					}
					num11 += 25f;
				}
				if (!selectedMod.IsCoreMod)
				{
					Text.Anchor = TextAnchor.UpperRight;
					Rect rect7 = new Rect(position.width - 300f, num11, 300f, Text.LineHeight);
					if (!selectedMod.VersionCompatible)
					{
						GUI.color = Color.red;
					}
					Widgets.Label(rect7, "ModTargetVersion".Translate() + ": " + selectedMod.SupportedVersionsReadOnly.Select(delegate(System.Version v)
					{
						string text = VersionControl.IsCompatible(v) ? "<color=green>" : "<color=red>";
						string text2 = "</color>";
						return (v.Build > 0) ? $"{text}{v.Major.ToString()}.{v.Minor.ToString()}.{v.Build.ToString()}{text2}" : $"{text}{v.Major.ToString()}.{v.Minor.ToString()}{text2}";
					}).ToCommaList());
					GUI.color = Color.white;
					num11 += Text.LineHeight;
				}
				if (anyReqsCached)
				{
					Text.Anchor = TextAnchor.MiddleRight;
					TaggedString taggedString = "ModDisplayFulfilledRequirements".Translate();
					float num12 = Text.CalcSize(taggedString).x + 24f + 4f;
					Rect rect8 = new Rect(position.width - num12, num11, num12, 24f);
					bool flag = displayFullfilledRequirements;
					Widgets.CheckboxLabeled(rect8, taggedString, ref displayFullfilledRequirements);
					if (flag != displayFullfilledRequirements)
					{
						RecacheSelectedModRequirements();
					}
					num11 += 34f;
				}
				Text.Anchor = TextAnchor.UpperLeft;
				float num13 = Mathf.Max(num10, num11) + (anyReqsCached ? 10f : 17f);
				Rect outRect = new Rect(0f, num13, position.width, position.height - num13 - 40f);
				float width = outRect.width - 16f;
				float num14 = Text.CalcHeight(selectedMod.Description, width);
				num14 = Mathf.Min(num14 * 1.25f, num14 + 200f);
				Rect viewRect = new Rect(0f, 0f, width, num14 + modRequirementsHeightCached + (anyReqsInfoToShowCached ? 10f : 0f));
				float num15 = (viewRect.height > outRect.height) ? 16f : 0f;
				Widgets.BeginScrollView(outRect, ref modDescriptionScrollPosition, viewRect);
				float num16 = 0f;
				if (anyReqsInfoToShowCached)
				{
					num16 = DoRequirementSection(position.width - num15);
					num16 += 10f;
				}
				Widgets.Label(new Rect(0f, num16, viewRect.width - num15, viewRect.height - num16), selectedMod.Description);
				Widgets.EndScrollView();
				if (Prefs.DevMode && SteamManager.Initialized && selectedMod.CanToUploadToWorkshop() && Widgets.ButtonText(new Rect(0f, position.yMax - 40f, 200f, 40f), Workshop.UploadButtonLabel(selectedMod.GetPublishedFileId())))
				{
					List<string> list = selectedMod.loadFolders?.GetIssueList(selectedMod);
					if (selectedMod.HadIncorrectlyFormattedVersionInMetadata)
					{
						Messages.Message("MessageModNeedsWellFormattedTargetVersion".Translate(VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor), MessageTypeDefOf.RejectInput, historical: false);
					}
					else if (selectedMod.HadIncorrectlyFormattedPackageId)
					{
						Find.WindowStack.Add(new Dialog_MessageBox("MessageModNeedsWellFormattedPackageId".Translate()));
					}
					else if (!list.NullOrEmpty())
					{
						Find.WindowStack.Add(new Dialog_MessageBox("ModHadLoadFolderIssues".Translate() + "\n" + list.ToLineList("  - ")));
					}
					else
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate
						{
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
							Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate
							{
								SoundDefOf.Tick_High.PlayOneShotOnCamera();
								Workshop.Upload(selectedMod);
							}, destructive: true);
							dialog_MessageBox.buttonAText = "Yes".Translate();
							dialog_MessageBox.buttonBText = "No".Translate();
							dialog_MessageBox.interactionDelay = 6f;
							Find.WindowStack.Add(dialog_MessageBox);
						}, destructive: true));
					}
				}
				if (!selectedMod.Url.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.MiddleLeft;
					float num17 = Mathf.Min(position.width / 2f, Text.CalcSize(selectedMod.Url).x);
					if (Widgets.ButtonText(new Rect(position.width - num17, outRect.yMax, num17, position.yMax - outRect.yMax), selectedMod.Url.Truncate(num17), drawBackground: false))
					{
						Application.OpenURL(selectedMod.Url);
					}
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			GUI.EndGroup();
			GUI.EndGroup();
			Text.Font = GameFont.Tiny;
			TaggedString taggedString2 = "GameVersionIndicator".Translate() + ": " + VersionControl.CurrentVersionString;
			float x = Text.CalcSize(taggedString2).x;
			Widgets.Label(new Rect(0f, rect.height - 15f, x, Text.LineHeight), taggedString2);
			Text.Font = GameFont.Small;
			int num18 = ModLister.InstalledModsListHash(activeOnly: true);
			if (activeModsHash == -1 || activeModsHash != num18)
			{
				modWarningsCached = ModsConfig.GetModWarnings();
				RecacheSelectedModRequirements();
				activeModsHash = num18;
				modsInListOrderDirty = true;
			}
		}

		private void DoModRow(Rect r, ModMetaData mod, int index, int reorderableGroup)
		{
			bool active = mod.Active;
			Action clickAction = null;
			if (mod.Source == ContentSource.SteamWorkshop)
			{
				clickAction = delegate
				{
					SteamUtility.OpenWorkshopPage(mod.GetPublishedFileId());
				};
			}
			ContentSourceUtility.DrawContentSource(r, mod.Source, clickAction);
			r.xMin += 28f;
			bool selected = mod == selectedMod;
			Rect rect = r;
			if (mod.enabled)
			{
				string text = "";
				if (active)
				{
					text += "DragToReorder".Translate() + ".\n";
				}
				if (!mod.VersionCompatible)
				{
					GUI.color = Color.yellow;
					if (!text.NullOrEmpty())
					{
						text += "\n";
					}
					text = ((!mod.MadeForNewerVersion) ? ((string)(text + "ModNotMadeForThisVersion".Translate())) : ((string)(text + "ModNotMadeForThisVersion_Newer".Translate())));
				}
				if (active && !modWarningsCached.NullOrEmpty() && !modWarningsCached[index].NullOrEmpty())
				{
					GUI.color = Color.red;
					if (!text.NullOrEmpty())
					{
						text += "\n";
					}
					text += modWarningsCached[index];
				}
				GUI.color = FilteredColor(GUI.color, mod.Name);
				if (!text.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, new TipSignal(text, mod.GetHashCode() * 3311));
				}
				float num = rect.width - 24f;
				if (active)
				{
					GUI.DrawTexture(new Rect(rect.xMax - 48f + 2f, rect.y, 24f, 24f), TexButton.DragHash);
					num -= 24f;
				}
				Text.Font = GameFont.Small;
				string label = mod.Name.Truncate(num, truncatedModNamesCache);
				bool checkOn = active;
				if (Widgets.CheckboxLabeledSelectable(rect, label, ref selected, ref checkOn))
				{
					selectedMod = mod;
					RecacheSelectedModRequirements();
				}
				if (active && !checkOn && mod.IsCoreMod)
				{
					ModMetaData coreMod = mod;
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisableCoreMod".Translate(), delegate
					{
						coreMod.Active = false;
						truncatedModNamesCache.Clear();
					}));
				}
				else
				{
					if (!active && checkOn)
					{
						foreach (ModMetaData item in ModsConfig.ActiveModsInLoadOrder)
						{
							if (item.PackageIdNonUnique.Equals(mod.PackageIdNonUnique, StringComparison.InvariantCultureIgnoreCase))
							{
								Find.WindowStack.Add(new Dialog_MessageBox("MessageModWithPackageIdAlreadyEnabled".Translate(mod.PackageIdPlayerFacing, item.Name)));
								return;
							}
						}
					}
					if (checkOn != active)
					{
						mod.Active = checkOn;
					}
					truncatedModNamesCache.Clear();
				}
			}
			else
			{
				GUI.color = FilteredColor(Color.gray, mod.Name);
				Widgets.Label(rect, mod.Name);
			}
			GUI.color = Color.white;
		}

		private void DoModRowDownloading(Listing_Standard listing, int index)
		{
			Rect rect = new Rect(0f, (float)index * 26f, listing.ColumnWidth, 26f);
			ContentSourceUtility.DrawContentSource(rect, ContentSource.SteamWorkshop);
			rect.xMin += 28f;
			Widgets.Label(rect, "Downloading".Translate() + GenText.MarchingEllipsis());
		}

		private float DoRequirementSection(float width)
		{
			float num = 0f;
			if (visibleReqsCached.Count > 0 || anyOrderingIssuesCached)
			{
				bool num2 = anyUnfulfilledReqsCached || anyOrderingIssuesCached;
				Rect rect = new Rect(0f, 0f, width, modRequirementsHeightCached);
				if (num2)
				{
					Widgets.DrawBoxSolid(rect, UnmetRequirementBoxBGColor);
				}
				GUI.color = (num2 ? UnmetRequirementBoxOutlineColor : RequirementBoxOutlineColor);
				Widgets.DrawBox(rect);
				GUI.color = Color.white;
				num += 10f;
				Text.Anchor = TextAnchor.MiddleLeft;
				for (int i = 0; i < visibleReqsCached.Count; i++)
				{
					DrawRequirementEntry(entryRect: new Rect(10f, num, width - 20f, 26f), entry: visibleReqsCached[i], y: ref num);
				}
				if (anyOrderingIssuesCached)
				{
					num += 4f;
					Widgets.Label(new Rect(10f, num, width - 20f, Text.LineHeight * 2f), "ModOrderingWarning".Translate());
					num += Text.LineHeight * 2f;
				}
				num += 10f;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			return num;
		}

		private void DrawRequirementEntry(ModRequirement entry, Rect entryRect, ref float y)
		{
			Widgets.DrawBoxSolid(entryRect, entry.IsSatisfied ? RequirementRowColor : (Mouse.IsOver(entryRect) ? UnmetRequirementRowColorHighlighted : UnmetRequirementRowColor));
			Rect rect = entryRect;
			rect.x += 4f;
			rect.width = 200f;
			Widgets.Label(rect, entry.RequirementTypeLabel.Truncate(rect.width, truncatedStringCache));
			Rect rect2 = entryRect;
			rect2.x = rect.xMax + 4f;
			rect2.width -= rect2.x + 24f;
			Widgets.Label(rect2, entry.displayName.Truncate(rect2.width, truncatedStringCache));
			if (Widgets.ButtonInvisible(entryRect))
			{
				entry.OnClicked(this);
			}
			Rect position = default(Rect);
			position.xMin = entryRect.xMax - 24f - 4f;
			position.y = entryRect.y + 1f;
			position.width = 24f;
			position.height = 24f;
			GUI.DrawTexture(position, entry.StatusIcon);
			TooltipHandler.TipRegion(entryRect, new TipSignal(entry.Tooltip));
			y += 30f;
		}

		private void RecacheSelectedModRequirements()
		{
			anyReqsCached = false;
			anyReqsInfoToShowCached = false;
			anyUnfulfilledReqsCached = false;
			anyOrderingIssuesCached = false;
			visibleReqsCached.Clear();
			if (selectedMod == null)
			{
				return;
			}
			foreach (ModRequirement item in (from r in selectedMod.GetRequirements()
				orderby r.IsSatisfied, r.RequirementTypeLabel
				select r).ToList())
			{
				bool isSatisfied = item.IsSatisfied;
				if (!isSatisfied || displayFullfilledRequirements)
				{
					visibleReqsCached.Add(item);
					if (!isSatisfied)
					{
						anyUnfulfilledReqsCached = true;
					}
				}
				anyReqsCached = true;
				anyReqsInfoToShowCached = true;
			}
			anyOrderingIssuesCached = ModsConfig.ModHasAnyOrderingIssues(selectedMod);
			if (visibleReqsCached.Any() || anyOrderingIssuesCached)
			{
				anyReqsInfoToShowCached = true;
				modRequirementsHeightCached = (float)visibleReqsCached.Count * 30f + 20f;
				if (anyOrderingIssuesCached)
				{
					modRequirementsHeightCached += Text.LineHeight * 2f + 4f;
				}
			}
			else
			{
				modRequirementsHeightCached = 0f;
			}
		}

		public void Notify_ModsListChanged()
		{
			string selModId = selectedMod.PackageId;
			selectedMod = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData m) => m.SamePackageId(selModId));
			RecacheSelectedModRequirements();
			modsInListOrderDirty = true;
		}

		internal void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid)
		{
			if (selectedMod != null && selectedMod.FolderName == pfid.ToString())
			{
				selectedMod = null;
			}
			RecacheSelectedModRequirements();
			modsInListOrderDirty = true;
		}

		public void SelectMod(ModMetaData mod)
		{
			selectedMod = mod;
			RecacheSelectedModRequirements();
		}

		public override void PostClose()
		{
			ModsConfig.Save();
			foreach (ModMetaData item in ModsConfig.ActiveModsInLoadOrder)
			{
				item.UnsetPreviewImage();
			}
			Resources.UnloadUnusedAssets();
			if (activeModsWhenOpenedHash != ModLister.InstalledModsListHash(activeOnly: true))
			{
				ModsConfig.RestartFromChangedMods();
			}
		}

		private Color FilteredColor(Color color, string label)
		{
			if (filter.NullOrEmpty())
			{
				return color;
			}
			if (label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return color;
			}
			return color * new Color(1f, 1f, 1f, 0.3f);
		}
	}
}
