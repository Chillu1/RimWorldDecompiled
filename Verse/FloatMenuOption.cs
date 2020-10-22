using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class FloatMenuOption
	{
		private string labelInt;

		public Action action;

		private MenuOptionPriority priorityInt = MenuOptionPriority.Default;

		public bool autoTakeable;

		public float autoTakeablePriority;

		public Action mouseoverGuiAction;

		public Thing revalidateClickTarget;

		public WorldObject revalidateWorldClickTarget;

		public float extraPartWidth;

		public Func<Rect, bool> extraPartOnGUI;

		public string tutorTag;

		private FloatMenuSizeMode sizeMode;

		private float cachedRequiredHeight;

		private float cachedRequiredWidth;

		private bool drawPlaceHolderIcon;

		private ThingDef shownItem;

		private Texture2D itemIcon;

		private Color iconColor = Color.white;

		public const float MaxWidth = 300f;

		private const float NormalVerticalMargin = 4f;

		private const float TinyVerticalMargin = 1f;

		private const float NormalHorizontalMargin = 6f;

		private const float TinyHorizontalMargin = 3f;

		private const float MouseOverLabelShift = 4f;

		private static readonly Color ColorBGActive = new ColorInt(21, 25, 29).ToColor;

		private static readonly Color ColorBGActiveMouseover = new ColorInt(29, 45, 50).ToColor;

		private static readonly Color ColorBGDisabled = new ColorInt(40, 40, 40).ToColor;

		private static readonly Color ColorTextActive = Color.white;

		private static readonly Color ColorTextDisabled = new Color(0.9f, 0.9f, 0.9f);

		public const float ExtraPartHeight = 30f;

		private const float ItemIconSize = 27f;

		private const float ItemIconMargin = 4f;

		public string Label
		{
			get
			{
				return labelInt;
			}
			set
			{
				if (value.NullOrEmpty())
				{
					value = "(missing label)";
				}
				labelInt = value.TrimEnd();
				SetSizeMode(sizeMode);
			}
		}

		private float VerticalMargin
		{
			get
			{
				if (sizeMode != FloatMenuSizeMode.Normal)
				{
					return 1f;
				}
				return 4f;
			}
		}

		private float HorizontalMargin
		{
			get
			{
				if (sizeMode != FloatMenuSizeMode.Normal)
				{
					return 3f;
				}
				return 6f;
			}
		}

		private float IconOffset
		{
			get
			{
				if (shownItem == null && !drawPlaceHolderIcon && !(itemIcon != null))
				{
					return 0f;
				}
				return 27f;
			}
		}

		private GameFont CurrentFont
		{
			get
			{
				if (sizeMode != FloatMenuSizeMode.Normal)
				{
					return GameFont.Tiny;
				}
				return GameFont.Small;
			}
		}

		public bool Disabled
		{
			get
			{
				return action == null;
			}
			set
			{
				if (value)
				{
					action = null;
				}
			}
		}

		public float RequiredHeight => cachedRequiredHeight;

		public float RequiredWidth => cachedRequiredWidth;

		public MenuOptionPriority Priority
		{
			get
			{
				if (Disabled)
				{
					return MenuOptionPriority.DisabledOption;
				}
				return priorityInt;
			}
			set
			{
				if (Disabled)
				{
					Log.Error("Setting priority on disabled FloatMenuOption: " + Label);
				}
				priorityInt = value;
			}
		}

		public FloatMenuOption(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Default, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null)
		{
			Label = label;
			this.action = action;
			priorityInt = priority;
			this.revalidateClickTarget = revalidateClickTarget;
			this.mouseoverGuiAction = mouseoverGuiAction;
			this.extraPartWidth = extraPartWidth;
			this.extraPartOnGUI = extraPartOnGUI;
			this.revalidateWorldClickTarget = revalidateWorldClickTarget;
		}

		public FloatMenuOption(string label, Action action, ThingDef shownItemForIcon, MenuOptionPriority priority = MenuOptionPriority.Default, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null)
			: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget)
		{
			shownItem = shownItemForIcon;
			if (shownItemForIcon == null)
			{
				drawPlaceHolderIcon = true;
			}
		}

		public FloatMenuOption(string label, Action action, Texture2D itemIcon, Color iconColor, MenuOptionPriority priority = MenuOptionPriority.Default, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null)
			: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget)
		{
			this.itemIcon = itemIcon;
			this.iconColor = iconColor;
		}

		public void SetSizeMode(FloatMenuSizeMode newSizeMode)
		{
			sizeMode = newSizeMode;
			GameFont font = Text.Font;
			Text.Font = CurrentFont;
			float width = 300f - (2f * HorizontalMargin + 4f + extraPartWidth + IconOffset);
			cachedRequiredHeight = 2f * VerticalMargin + Text.CalcHeight(Label, width);
			cachedRequiredWidth = HorizontalMargin + 4f + Text.CalcSize(Label).x + extraPartWidth + HorizontalMargin + IconOffset + 4f;
			Text.Font = font;
		}

		public void Chosen(bool colonistOrdering, FloatMenu floatMenu)
		{
			floatMenu?.PreOptionChosen(this);
			if (!Disabled)
			{
				if (action != null)
				{
					if (colonistOrdering)
					{
						SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
					}
					action();
				}
			}
			else
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
			}
		}

		public virtual bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
		{
			Rect rect2 = rect;
			rect2.height--;
			bool flag = !Disabled && Mouse.IsOver(rect2);
			bool flag2 = false;
			Text.Font = CurrentFont;
			Rect rect3 = rect;
			rect3.xMin += 4f;
			rect3.xMax = rect.x + 27f;
			rect3.yMin += 4f;
			rect3.yMax = rect.y + 27f;
			if (flag)
			{
				rect3.x += 4f;
			}
			Rect rect4 = rect;
			rect4.xMin += HorizontalMargin;
			rect4.xMax -= HorizontalMargin;
			rect4.xMax -= 4f;
			rect4.xMax -= extraPartWidth + IconOffset;
			rect4.x += IconOffset;
			if (flag)
			{
				rect4.x += 4f;
			}
			Rect rect5 = default(Rect);
			if (extraPartWidth != 0f)
			{
				float num = Mathf.Min(Text.CalcSize(Label).x, rect4.width - 4f);
				rect5 = new Rect(rect4.xMin + num, rect4.yMin, extraPartWidth, 30f);
				flag2 = Mouse.IsOver(rect5);
			}
			if (!Disabled)
			{
				MouseoverSounds.DoRegion(rect2);
			}
			Color color = GUI.color;
			if (Disabled)
			{
				GUI.color = ColorBGDisabled * color;
			}
			else if (flag && !flag2)
			{
				GUI.color = ColorBGActiveMouseover * color;
			}
			else
			{
				GUI.color = ColorBGActive * color;
			}
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = ((!Disabled) ? ColorTextActive : ColorTextDisabled) * color;
			if (sizeMode == FloatMenuSizeMode.Tiny)
			{
				rect4.y += 1f;
			}
			Widgets.DrawAtlas(rect, TexUI.FloatMenuOptionBG);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect4, Label);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = iconColor;
			if (shownItem != null || drawPlaceHolderIcon)
			{
				Widgets.DefIcon(rect3, shownItem, null, 1f, drawPlaceHolderIcon);
			}
			else if ((bool)itemIcon)
			{
				GUI.DrawTexture(rect3, itemIcon);
			}
			GUI.color = color;
			if (extraPartOnGUI != null)
			{
				bool num2 = extraPartOnGUI(rect5);
				GUI.color = color;
				if (num2)
				{
					return true;
				}
			}
			if (flag && mouseoverGuiAction != null)
			{
				mouseoverGuiAction();
			}
			if (tutorTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, tutorTag);
			}
			if (Widgets.ButtonInvisible(rect2))
			{
				if (tutorTag != null && !TutorSystem.AllowAction(tutorTag))
				{
					return false;
				}
				Chosen(colonistOrdering, floatMenu);
				if (tutorTag != null)
				{
					TutorSystem.Notify_Event(tutorTag);
				}
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return "FloatMenuOption(" + Label + ", " + (Disabled ? "disabled" : "enabled") + ")";
		}
	}
}
