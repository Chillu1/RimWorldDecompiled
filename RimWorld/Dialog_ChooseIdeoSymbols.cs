using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_ChooseIdeoSymbols : Window
	{
		private Ideo ideo;

		private string newName;

		private string newAdjective;

		private string newMemberName;

		private string newWorshipRoomLabel;

		private IdeoIconDef newIconDef;

		private ColorDef newColorDef;

		private Vector2 scrollPos;

		private float viewHeight;

		private static List<ColorDef> allColors;

		private const int IconSize = 40;

		private const int IconPadding = 5;

		private const int IconMargin = 5;

		private const int ColorSize = 22;

		private const int ColorPadding = 2;

		private static readonly Vector2 ButSize = new Vector2(150f, 38f);

		private static readonly Color IconColor = new Color(0.95f, 0.95f, 0.95f);

		private static readonly float EditFieldHeight = 30f;

		private static readonly float ResetButtonWidth = 120f;

		private static readonly Regex ValidSymbolRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

		private const int MaxSymbolLength = 40;

		public override Vector2 InitialSize => new Vector2(740f, 700f);

		private static List<ColorDef> IdeoColorsSorted
		{
			get
			{
				if (allColors == null)
				{
					allColors = new List<ColorDef>();
					allColors.AddRange(DefDatabase<ColorDef>.AllDefsListForReading.Where((ColorDef x) => x.colorType == ColorType.Ideo));
					allColors.SortByColor((ColorDef x) => x.color);
				}
				return allColors;
			}
		}

		public Dialog_ChooseIdeoSymbols(Ideo ideo)
		{
			this.ideo = ideo;
			absorbInputAroundWindow = true;
			newName = ideo.name;
			newAdjective = ideo.adjective;
			newMemberName = ideo.memberName;
			newWorshipRoomLabel = ideo.WorshipRoomLabel;
			newIconDef = ideo.iconDef;
			newColorDef = ideo.colorDef;
		}

		public override void OnAcceptKeyPressed()
		{
			TryAccept();
			Event.current.Use();
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height -= Window.CloseButSize.y;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(rect2.x, rect2.y, rect.width, 35f), "EditSymbols".Translate());
			Text.Font = GameFont.Small;
			rect2.yMin += 45f;
			float y = rect2.y;
			float num = rect2.x + rect2.width / 3f;
			float width = rect2.xMax - num - ResetButtonWidth - 10f;
			float curY = y;
			Widgets.Label(rect2.x, ref curY, rect2.width, "Name".Translate());
			newName = Widgets.TextField(new Rect(num, y, width, EditFieldHeight), newName, 40, ValidSymbolRegex);
			y += EditFieldHeight + 10f;
			float curY2 = y;
			Widgets.Label(rect2.x, ref curY2, rect2.width, "Adjective".Translate());
			newAdjective = Widgets.TextField(new Rect(num, y, width, EditFieldHeight), newAdjective, 40, ValidSymbolRegex);
			y += EditFieldHeight + 10f;
			float curY3 = y;
			Widgets.Label(rect2.x, ref curY3, rect2.width, "IdeoMembers".Translate());
			newMemberName = Widgets.TextField(new Rect(num, y, width, EditFieldHeight), newMemberName, 40, ValidSymbolRegex);
			y += EditFieldHeight + 10f;
			float curY4 = y;
			Widgets.Label(rect2.x, ref curY4, rect2.width, "WorshipRoom".Translate());
			Rect rect3 = new Rect(num, y, width, EditFieldHeight);
			Rect rect4 = new Rect(rect3.xMax + 10f, y, ResetButtonWidth, EditFieldHeight);
			newWorshipRoomLabel = Widgets.TextField(rect3, newWorshipRoomLabel, 40, ValidSymbolRegex);
			if (Widgets.ButtonText(rect4, "Reset".Translate()))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				ideo.WorshipRoomLabel = null;
				newWorshipRoomLabel = ideo.WorshipRoomLabel;
			}
			y += EditFieldHeight + 10f;
			Rect mainRect = rect2;
			mainRect.yMax -= 4f;
			Widgets.Label(mainRect.x, ref y, mainRect.width, "Icon".Translate());
			mainRect.yMin = y;
			DoColorSelector(mainRect, ref y);
			mainRect.yMin = y;
			DoIconSelector(mainRect);
			if (Widgets.ButtonText(new Rect(0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Back".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(rect.width - ButSize.x, rect.height - ButSize.y, ButSize.x, ButSize.y), "DoneButton".Translate()))
			{
				TryAccept();
			}
		}

		private void DoIconSelector(Rect mainRect)
		{
			int num = 50;
			Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, viewHeight);
			Widgets.BeginScrollView(mainRect, ref scrollPos, viewRect);
			IEnumerable<IdeoIconDef> allDefs = DefDatabase<IdeoIconDef>.AllDefs;
			int num2 = Mathf.FloorToInt(viewRect.width / (float)(num + 5));
			int num3 = allDefs.Count();
			int num4 = 0;
			foreach (IdeoIconDef item in allDefs)
			{
				int num5 = num4 / num2;
				int num6 = num4 % num2;
				int num7 = ((num4 >= num3 - num3 % num2) ? (num3 % num2) : num2);
				float num8 = (viewRect.width - (float)(num7 * num) - (float)((num7 - 1) * 5)) / 2f;
				Rect rect = new Rect(num8 + (float)(num6 * num) + (float)(num6 * 5), num5 * num + num5 * 5, num, num);
				Widgets.DrawLightHighlight(rect);
				Widgets.DrawHighlightIfMouseover(rect);
				if (item == newIconDef)
				{
					Widgets.DrawBox(rect);
				}
				GUI.color = newColorDef.color;
				GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 5f, 40f, 40f), item.Icon);
				GUI.color = Color.white;
				if (Widgets.ButtonInvisible(rect))
				{
					newIconDef = item;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				viewHeight = Mathf.Max(viewHeight, rect.yMax);
				num4++;
			}
			GUI.color = Color.white;
			Widgets.EndScrollView();
		}

		private void DoColorSelector(Rect mainRect, ref float curY)
		{
			int num = 26;
			float num2 = 98f;
			int num3 = Mathf.FloorToInt((mainRect.width - num2) / (float)(num + 2));
			int num4 = Mathf.CeilToInt((float)IdeoColorsSorted.Count / (float)num3);
			Widgets.BeginGroup(mainRect);
			GUI.color = newColorDef.color;
			GUI.DrawTexture(new Rect(5f, 5f, 88f, 88f), newIconDef.Icon);
			GUI.color = Color.white;
			curY += num2;
			int num5 = 0;
			foreach (ColorDef item in IdeoColorsSorted)
			{
				int num6 = num5 / num3;
				int num7 = num5 % num3;
				float num8 = (num2 - (float)(num * num4) - 2f) / 2f;
				Rect rect = new Rect(num2 + (float)(num7 * num) + (float)(num7 * 2), num8 + (float)(num6 * num) + (float)(num6 * 2), num, num);
				Widgets.DrawLightHighlight(rect);
				Widgets.DrawHighlightIfMouseover(rect);
				if (newColorDef == item)
				{
					Widgets.DrawBox(rect);
				}
				Widgets.DrawBoxSolid(new Rect(rect.x + 2f, rect.y + 2f, 22f, 22f), item.color);
				if (Widgets.ButtonInvisible(rect))
				{
					newColorDef = item;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				curY = Mathf.Max(curY, mainRect.yMin + rect.yMax);
				num5++;
			}
			Widgets.EndGroup();
			curY += 4f;
		}

		private void TryAccept()
		{
			if (!newName.NullOrEmpty())
			{
				newName = newName.Trim();
			}
			if (!newAdjective.NullOrEmpty())
			{
				newAdjective = newAdjective.Trim();
			}
			if (!newMemberName.NullOrEmpty())
			{
				newMemberName = newMemberName.Trim();
			}
			if (!newWorshipRoomLabel.NullOrEmpty())
			{
				newWorshipRoomLabel = newWorshipRoomLabel.Trim();
			}
			bool num = ideo.name != newName || ideo.adjective != newAdjective || ideo.memberName != newMemberName;
			if (!newName.NullOrEmpty())
			{
				ideo.name = newName;
			}
			if (!newAdjective.NullOrEmpty())
			{
				ideo.adjective = newAdjective;
			}
			if (!newMemberName.NullOrEmpty())
			{
				ideo.memberName = newMemberName;
			}
			if (ideo.WorshipRoomLabel != newWorshipRoomLabel && !newWorshipRoomLabel.NullOrEmpty())
			{
				ideo.WorshipRoomLabel = newWorshipRoomLabel;
			}
			ideo.SetIcon(newIconDef, newColorDef, newColorDef != ideo.colorDef);
			if (num)
			{
				ideo.MakeMemeberNamePluralDirty();
				ideo.RegenerateAllPreceptNames();
			}
			Close();
		}
	}
}
