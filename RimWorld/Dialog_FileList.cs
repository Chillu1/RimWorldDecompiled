using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_FileList : Window
	{
		protected string interactButLabel = "Error";

		protected float bottomAreaHeight;

		protected List<SaveFileInfo> files = new List<SaveFileInfo>();

		protected Vector2 scrollPosition = Vector2.zero;

		protected string typingName = "";

		private bool focusedNameArea;

		protected const float EntryHeight = 40f;

		protected const float FileNameLeftMargin = 8f;

		protected const float FileNameRightMargin = 4f;

		protected const float FileInfoWidth = 94f;

		protected const float InteractButWidth = 100f;

		protected const float InteractButHeight = 36f;

		protected const float DeleteButSize = 36f;

		private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);

		protected const float NameTextFieldWidth = 400f;

		protected const float NameTextFieldHeight = 35f;

		protected const float NameTextFieldButtonSpace = 20f;

		public override Vector2 InitialSize => new Vector2(620f, 700f);

		protected virtual bool ShouldDoTypeInField => false;

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
			inRect.height -= 45f;
			float y = vector.y;
			float height = (float)files.Count * y;
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
			Rect outRect = new Rect(inRect.AtZero());
			outRect.height -= bottomAreaHeight;
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			float num = 0f;
			int num2 = 0;
			foreach (SaveFileInfo file in files)
			{
				if (num + vector.y >= scrollPosition.y && num <= scrollPosition.y + outRect.height)
				{
					Rect rect = new Rect(0f, num, vector.x, vector.y);
					if (num2 % 2 == 0)
					{
						Widgets.DrawAltRect(rect);
					}
					GUI.BeginGroup(rect);
					Rect rect2 = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
					if (Widgets.ButtonImage(rect2, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
					{
						FileInfo localFile = file.FileInfo;
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate
						{
							localFile.Delete();
							ReloadFiles();
						}, destructive: true));
					}
					TooltipHandler.TipRegionByKey(rect2, "DeleteThisSavegame");
					Text.Font = GameFont.Small;
					Rect rect3 = new Rect(rect2.x - 100f, (rect.height - 36f) / 2f, 100f, 36f);
					if (Widgets.ButtonText(rect3, interactButLabel))
					{
						DoFileInteraction(Path.GetFileNameWithoutExtension(file.FileInfo.Name));
					}
					Rect rect4 = new Rect(rect3.x - 94f, 0f, 94f, rect.height);
					DrawDateAndVersion(file, rect4);
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = FileNameColor(file);
					Rect rect5 = new Rect(8f, 0f, rect4.x - 8f - 4f, rect.height);
					Text.Anchor = TextAnchor.MiddleLeft;
					Text.Font = GameFont.Small;
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileInfo.Name);
					Widgets.Label(rect5, fileNameWithoutExtension.Truncate(rect5.width * 1.8f));
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.EndGroup();
				}
				num += vector.y;
				num2++;
			}
			Widgets.EndScrollView();
			if (ShouldDoTypeInField)
			{
				DoTypeInField(inRect.AtZero());
			}
		}

		protected abstract void DoFileInteraction(string fileName);

		protected abstract void ReloadFiles();

		protected virtual void DoTypeInField(Rect rect)
		{
			GUI.BeginGroup(rect);
			bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
			float y = rect.height - 52f;
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
					DoFileInteraction(typingName);
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
		}

		protected virtual Color FileNameColor(SaveFileInfo sfi)
		{
			return DefaultFileTextColor;
		}

		public static void DrawDateAndVersion(SaveFileInfo sfi, Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(0f, 2f, rect.width, rect.height / 2f);
			GUI.color = SaveFileInfo.UnimportantTextColor;
			Widgets.Label(rect2, sfi.FileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
			Rect rect3 = new Rect(0f, rect2.yMax, rect.width, rect.height / 2f);
			GUI.color = sfi.VersionColor;
			Widgets.Label(rect3, sfi.GameVersion);
			if (Mouse.IsOver(rect3))
			{
				TooltipHandler.TipRegion(rect3, sfi.CompatibilityTip);
			}
			GUI.EndGroup();
		}
	}
}
