using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Dialog_FileList : Window
{
	protected string interactButLabel = "Error";

	protected float bottomAreaHeight;

	protected readonly List<SaveFileInfo> files = new List<SaveFileInfo>();

	private readonly QuickSearchWidget search = new QuickSearchWidget();

	private bool focusedSearch;

	private bool focusedNameArea;

	private Vector2 scrollPosition = Vector2.zero;

	protected string typingName = "";

	protected string deleteTipKey = "DeleteThisSavegame";

	protected const float EntryHeight = 40f;

	protected const float FileNameLeftMargin = 8f;

	protected const float FileNameRightMargin = 4f;

	protected const float FileInfoWidth = 94f;

	protected const float InteractButWidth = 100f;

	protected const float InteractButHeight = 36f;

	protected const float DeleteButSize = 36f;

	protected const float NameTextFieldWidth = 400f;

	protected const float NameTextFieldHeight = 35f;

	protected const float NameTextFieldButtonSpace = 20f;

	protected static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);

	public override Vector2 InitialSize => new Vector2(620f, 700f);

	protected virtual bool ShouldDoTypeInField => false;

	protected virtual bool FocusSearchField => false;

	public Dialog_FileList()
	{
		doCloseButton = true;
		doCloseX = true;
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnAccept = false;
		ReloadFiles();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Vector2 vector = new Vector2(inRect.width - 16f, 40f);
		float y = vector.y;
		float height = (float)FilesMatchingFilter() * y;
		Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
		Rect rect = inRect.LeftHalf();
		rect.height = 24f;
		search.OnGUI(rect);
		if (!focusedSearch && FocusSearchField)
		{
			focusedSearch = true;
			search.Focus();
		}
		Rect outRect = inRect;
		outRect.yMin = rect.yMax + 10f;
		outRect.yMax -= Window.CloseButSize.y + bottomAreaHeight + 10f;
		if (ShouldDoTypeInField)
		{
			outRect.yMax -= 53f;
		}
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float num = 0f;
		int num2 = 0;
		foreach (SaveFileInfo file in files)
		{
			if (!search.filter.Matches(file.FileName))
			{
				continue;
			}
			if (num + vector.y >= scrollPosition.y && num <= scrollPosition.y + outRect.height)
			{
				Rect rect2 = new Rect(0f, num, vector.x, vector.y);
				if (num2 % 2 == 1)
				{
					Widgets.DrawAltRect(rect2);
				}
				Widgets.BeginGroup(rect2);
				Rect rect3 = new Rect(rect2.width - 36f, (rect2.height - 36f) / 2f, 36f, 36f);
				if (Widgets.ButtonImage(rect3, TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor))
				{
					FileInfo localFile = file.FileInfo;
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate
					{
						localFile.Delete();
						ReloadFiles();
					}, destructive: true));
				}
				TooltipHandler.TipRegionByKey(rect3, deleteTipKey);
				Text.Font = GameFont.Small;
				Rect rect4 = new Rect(rect3.x - 100f, (rect2.height - 36f) / 2f, 100f, 36f);
				if (Widgets.ButtonText(rect4, interactButLabel))
				{
					DoFileInteraction(Path.GetFileNameWithoutExtension(file.FileName));
				}
				Rect rect5 = new Rect(rect4.x - 94f, 0f, 94f, rect2.height);
				DrawDateAndVersion(file, rect5);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = FileNameColor(file);
				Rect rect6 = new Rect(8f, 0f, rect5.x - 8f - 4f, rect2.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Text.Font = GameFont.Small;
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
				Widgets.Label(rect6, fileNameWithoutExtension.Truncate(rect6.width * 1.8f));
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.EndGroup();
			}
			num += vector.y;
			num2++;
		}
		Widgets.EndScrollView();
		if (ShouldDoTypeInField)
		{
			DoTypeInField(inRect.TopPartPixels(inRect.height - Window.CloseButSize.y - 18f));
		}
	}

	private int FilesMatchingFilter()
	{
		int num = 0;
		for (int i = 0; i < files.Count; i++)
		{
			if (search.filter.Matches(files[i].FileName))
			{
				num++;
			}
		}
		return num;
	}

	protected abstract void DoFileInteraction(string fileName);

	protected abstract void ReloadFiles();

	protected virtual void DoTypeInField(Rect rect)
	{
		Widgets.BeginGroup(rect);
		bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
		float y = rect.height - 35f;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		GUI.SetNextControlName("MapNameField");
		string str = Widgets.TextField(new Rect(5f, y, 400f, 35f), typingName);
		if (GenText.IsValidFilename(str))
		{
			typingName = str;
		}
		if (!focusedNameArea)
		{
			UI.FocusControl("MapNameField", this);
			focusedNameArea = true;
		}
		if (Widgets.ButtonText(new Rect(420f, y, rect.width - 400f - 20f, 35f), "SaveGameButton".Translate()) || flag)
		{
			if (typingName.NullOrEmpty())
			{
				Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				DoFileInteraction(typingName?.Trim());
			}
		}
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.EndGroup();
	}

	protected virtual Color FileNameColor(SaveFileInfo sfi)
	{
		return DefaultFileTextColor;
	}

	public static void DrawDateAndVersion(SaveFileInfo sfi, Rect rect)
	{
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect2 = new Rect(0f, 2f, rect.width, rect.height / 2f);
		GUI.color = SaveFileInfo.UnimportantTextColor;
		Widgets.Label(rect2, sfi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
		Rect rect3 = new Rect(0f, rect2.yMax, rect.width, rect.height / 2f);
		GUI.color = sfi.VersionColor;
		Widgets.Label(rect3, sfi.GameVersion);
		if (Mouse.IsOver(rect3))
		{
			TooltipHandler.TipRegion(rect3, sfi.CompatibilityTip);
		}
		Widgets.EndGroup();
	}
}
