using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

[StaticConstructorOnStartup]
public class FloatMenuOption
{
	private string labelInt;

	public Action action;

	private MenuOptionPriority priorityInt = MenuOptionPriority.Default;

	public int orderInPriority;

	public bool autoTakeable;

	public float autoTakeablePriority;

	public Action<Rect> mouseoverGuiAction;

	public Thing revalidateClickTarget;

	public bool targetsDespawned;

	public WorldObject revalidateWorldClickTarget;

	public float extraPartWidth;

	public Func<Rect, bool> extraPartOnGUI;

	public string tutorTag;

	public ThingStyleDef thingStyle;

	public bool forceBasicStyle;

	public TipSignal? tooltip;

	public bool extraPartRightJustified;

	public int? graphicIndexOverride;

	public bool isGoto;

	private FloatMenuSizeMode sizeMode;

	private float cachedRequiredHeight;

	private float cachedRequiredWidth;

	private bool drawPlaceHolderIcon;

	private bool playSelectionSound = true;

	private ThingDef shownItem;

	public Thing iconThing;

	private Texture2D iconTex;

	public Rect iconTexCoords = new Rect(0f, 0f, 1f, 1f);

	private HorizontalJustification iconJustification;

	public Color iconColor = Color.white;

	public Color? forceThingColor;

	public const float MaxWidth = 300f;

	private const float TinyVerticalMargin = 1f;

	private const float NormalHorizontalMargin = 6f;

	private const float TinyHorizontalMargin = 3f;

	private const float MouseOverLabelShift = 4f;

	public static readonly Color ColorBGActive = new ColorInt(21, 25, 29).ToColor;

	public static readonly Color ColorBGActiveMouseover = new ColorInt(29, 45, 50).ToColor;

	public static readonly Color ColorBGDisabled = new ColorInt(40, 40, 40).ToColor;

	public static readonly Color ColorTextActive = Color.white;

	public static readonly Color ColorTextDisabled = new Color(0.9f, 0.9f, 0.9f);

	public const float ExtraPartHeight = 30f;

	private const float ItemIconSize = 27f;

	private const float ItemIconSizeTiny = 16f;

	private const float ItemIconMargin = 4f;

	private static float NormalVerticalMargin => SteamDeck.IsSteamDeck ? 10 : 4;

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
			return NormalVerticalMargin;
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
			if (shownItem == null && !drawPlaceHolderIcon && !(iconTex != null) && iconThing == null)
			{
				return 0f;
			}
			return CurIconSize;
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

	private float CurIconSize
	{
		get
		{
			if (sizeMode != FloatMenuSizeMode.Tiny)
			{
				return 27f;
			}
			return 16f;
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

	public FloatMenuOption(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0)
	{
		Label = label;
		this.action = action;
		priorityInt = priority;
		this.revalidateClickTarget = revalidateClickTarget;
		this.mouseoverGuiAction = mouseoverGuiAction;
		this.extraPartWidth = extraPartWidth;
		this.extraPartOnGUI = extraPartOnGUI;
		this.revalidateWorldClickTarget = revalidateWorldClickTarget;
		this.playSelectionSound = playSelectionSound;
		this.orderInPriority = orderInPriority;
	}

	public FloatMenuOption(string label, Action action, ThingDef shownItemForIcon, ThingStyleDef thingStyle = null, bool forceBasicStyle = false, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0, int? graphicIndexOverride = null)
		: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
	{
		shownItem = shownItemForIcon;
		this.thingStyle = thingStyle;
		this.forceBasicStyle = forceBasicStyle;
		this.graphicIndexOverride = graphicIndexOverride;
		if (shownItemForIcon == null)
		{
			drawPlaceHolderIcon = true;
		}
	}

	public FloatMenuOption(string label, Action action, ThingDef shownItemForIcon, Texture2D iconTex, ThingStyleDef thingStyle = null, bool forceBasicStyle = false, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0, int? graphicIndexOverride = null)
		: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
	{
		this.iconTex = iconTex;
		shownItem = shownItemForIcon;
		this.thingStyle = thingStyle;
		this.forceBasicStyle = forceBasicStyle;
		this.graphicIndexOverride = graphicIndexOverride;
		if (shownItemForIcon == null && iconTex == null)
		{
			drawPlaceHolderIcon = true;
		}
	}

	public FloatMenuOption(string label, Action action, Texture2D iconTex, Color iconColor, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0, HorizontalJustification iconJustification = HorizontalJustification.Left, bool extraPartRightJustified = false)
		: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
	{
		this.iconTex = iconTex;
		this.iconColor = iconColor;
		this.iconJustification = iconJustification;
		this.extraPartRightJustified = extraPartRightJustified;
	}

	public FloatMenuOption(string label, Action action, Thing iconThing, Color iconColor, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, bool playSelectionSound = true, int orderInPriority = 0)
		: this(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority)
	{
		this.iconThing = iconThing;
		this.iconColor = iconColor;
	}

	public static FloatMenuOption CheckboxLabeled(string label, Action checkboxStateChanged, bool currentState)
	{
		return new FloatMenuOption(label, checkboxStateChanged, Widgets.GetCheckboxTexture(currentState), Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, playSelectionSound: true, 0, HorizontalJustification.Right);
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
				if (colonistOrdering && playSelectionSound)
				{
					SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
				}
				action();
			}
		}
		else if (playSelectionSound)
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
		if (tooltip.HasValue)
		{
			TooltipHandler.TipRegion(rect, tooltip.Value);
		}
		Rect rect3 = rect;
		if (iconJustification == HorizontalJustification.Left)
		{
			rect3.xMin += 4f;
			rect3.xMax = rect.x + CurIconSize;
			rect3.yMin += 4f;
			rect3.yMax = rect.y + CurIconSize;
			if (flag)
			{
				rect3.x += 4f;
			}
		}
		Rect rect4 = rect;
		rect4.xMin += HorizontalMargin;
		rect4.xMax -= HorizontalMargin;
		rect4.xMax -= 4f;
		rect4.xMax -= extraPartWidth + IconOffset;
		if (iconJustification == HorizontalJustification.Left)
		{
			rect4.x += IconOffset;
		}
		if (flag)
		{
			rect4.x += 4f;
		}
		float num = Mathf.Min(Text.CalcSize(Label).x, rect4.width - 4f);
		float num2 = rect4.xMin + num;
		if (iconJustification == HorizontalJustification.Right)
		{
			rect3.x = num2 + 4f;
			rect3.width = CurIconSize;
			rect3.yMin += 4f;
			rect3.yMax = rect.y + CurIconSize;
			num2 += CurIconSize;
		}
		Rect rect5 = default(Rect);
		if (extraPartWidth != 0f)
		{
			if (extraPartRightJustified)
			{
				num2 = rect.xMax - extraPartWidth;
			}
			rect5 = new Rect(num2, rect4.yMin, extraPartWidth, 30f);
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
		GUI.color = new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * GUI.color.a);
		if (shownItem != null || drawPlaceHolderIcon)
		{
			ThingStyleDef thingStyleDef = thingStyle ?? ((shownItem == null || Find.World == null) ? null : Faction.OfPlayer.ideos?.PrimaryIdeo?.GetStyleFor(shownItem));
			if (forceBasicStyle)
			{
				thingStyleDef = null;
			}
			Color value = (forceThingColor.HasValue ? forceThingColor.Value : ((shownItem == null) ? Color.white : (shownItem.MadeFromStuff ? shownItem.GetColorForStuff(GenStuff.DefaultStuffFor(shownItem)) : shownItem.uiIconColor)));
			value.a *= color.a;
			Widgets.DefIcon(rect3, shownItem, null, 1f, thingStyleDef, drawPlaceHolderIcon, value, null, graphicIndexOverride);
		}
		else if ((bool)iconTex)
		{
			Widgets.DrawTextureFitted(rect3, iconTex, 1f, new Vector2(1f, 1f), iconTexCoords);
		}
		else if (iconThing != null)
		{
			Widgets.ThingIcon(rect3, iconThing, color.a);
		}
		GUI.color = color;
		if (extraPartOnGUI != null)
		{
			bool num3 = extraPartOnGUI(rect5);
			GUI.color = color;
			if (num3)
			{
				return true;
			}
		}
		if (flag && mouseoverGuiAction != null)
		{
			mouseoverGuiAction(rect);
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
