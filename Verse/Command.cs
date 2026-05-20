using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

[StaticConstructorOnStartup]
public abstract class Command : Gizmo
{
	public string defaultLabel;

	public string defaultDesc;

	public string defaultDescPostfix;

	public string mouseText;

	public Texture icon;

	public float iconAngle;

	public Vector2 iconProportions = Vector2.one;

	public Rect iconTexCoords = new Rect(0f, 0f, 1f, 1f);

	public float iconDrawScale = 1f;

	public Vector2 iconOffset;

	public Color defaultIconColor = Color.white;

	public KeyBindingDef hotKey;

	public SoundDef activateSound;

	public int groupKey = -1;

	public int groupKeyIgnoreContent = -1;

	public string tutorTag = "TutorTagNotSet";

	public bool shrinkable;

	public bool groupable = true;

	public bool hideMouseIcon;

	public Material overrideMaterial;

	public static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

	public static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

	public static readonly Color LowLightBgColor = new Color(0.8f, 0.8f, 0.7f, 0.5f);

	public static readonly Color LowLightIconColor = new Color(0.8f, 0.8f, 0.7f, 0.6f);

	public static readonly Color LowLightLabelColor = Color.white;

	public const float LowLightIconAlpha = 0.6f;

	protected const float InnerIconDrawScale = 0.85f;

	public virtual string Label => defaultLabel;

	public virtual string LabelCap => Label.CapitalizeFirst();

	public virtual string TopRightLabel => null;

	public virtual string Desc => defaultDesc;

	public virtual string DescPostfix => defaultDescPostfix;

	public virtual Color IconDrawColor => defaultIconColor;

	public virtual SoundDef CurActivateSound => activateSound;

	protected virtual bool DoTooltip => true;

	public virtual string HighlightTag => tutorTag;

	public virtual string TutorTagSelect => tutorTag;

	public virtual Texture2D BGTexture => BGTex;

	public virtual Texture2D BGTextureShrunk => BGTexShrunk;

	public float GetShrunkSize => 36f;

	public override float GetWidth(float maxWidth)
	{
		return 75f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		return GizmoOnGUIInt(new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f), parms);
	}

	public virtual GizmoResult GizmoOnGUIShrunk(Vector2 topLeft, float size, GizmoRenderParms parms)
	{
		parms.shrunk = true;
		return GizmoOnGUIInt(new Rect(topLeft.x, topLeft.y, size, size), parms);
	}

	protected virtual GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
	{
		Text.Font = GameFont.Tiny;
		Color color = Color.white;
		bool flag = false;
		if (Mouse.IsOver(butRect))
		{
			flag = true;
			if (!disabled)
			{
				color = GenUI.MouseoverColor;
			}
		}
		MouseoverSounds.DoRegion(butRect, SoundDefOf.Mouseover_Command);
		if (parms.highLight)
		{
			Widgets.DrawStrongHighlight(butRect.ExpandedBy(4f));
		}
		if (disabled)
		{
			parms.lowLight = true;
		}
		Material material = (parms.lowLight ? TexUI.GrayscaleGUI : null);
		GUI.color = (parms.lowLight ? LowLightBgColor : color);
		GenUI.DrawTextureWithMaterial(butRect, parms.shrunk ? BGTextureShrunk : BGTexture, material);
		GUI.color = color;
		DrawIcon(butRect, material, parms);
		bool flag2 = false;
		GUI.color = Color.white;
		if (parms.lowLight)
		{
			GUI.color = LowLightLabelColor;
		}
		Vector2 vector = (parms.shrunk ? new Vector2(3f, 0f) : new Vector2(5f, 3f));
		Rect rect = new Rect(butRect.x + vector.x, butRect.y + vector.y, butRect.width - 10f, Text.LineHeight);
		if (SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			if (parms.isFirst)
			{
				GUI.DrawTexture(new Rect(rect.x, rect.y, 21f, 21f), TexUI.SteamDeck_ButtonA);
				if (KeyBindingDefOf.Accept.KeyDownEvent)
				{
					flag2 = true;
					Event.current.Use();
				}
			}
		}
		else
		{
			KeyCode keyCode = ((hotKey != null) ? hotKey.MainKey : KeyCode.None);
			if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
			{
				Widgets.Label(rect, keyCode.ToStringReadable());
				GizmoGridDrawer.drawnHotKeys.Add(keyCode);
				if (hotKey.KeyDownEvent)
				{
					flag2 = true;
					Event.current.Use();
				}
			}
		}
		if (GizmoGridDrawer.customActivator != null && GizmoGridDrawer.customActivator(this))
		{
			flag2 = true;
		}
		if (Widgets.ButtonInvisible(butRect))
		{
			flag2 = true;
		}
		if (!parms.shrunk)
		{
			string topRightLabel = TopRightLabel;
			if (!topRightLabel.NullOrEmpty())
			{
				Vector2 vector2 = Text.CalcSize(topRightLabel);
				Rect position;
				Rect rect2 = (position = new Rect(butRect.xMax - vector2.x - 2f, butRect.y + 3f, vector2.x, vector2.y));
				position.x -= 2f;
				position.width += 3f;
				Text.Anchor = TextAnchor.UpperRight;
				GUI.DrawTexture(position, TexUI.GrayTextBG);
				Widgets.Label(rect2, topRightLabel);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			string labelCap = LabelCap;
			if (!labelCap.NullOrEmpty())
			{
				float num = Text.CalcHeight(labelCap, butRect.width + 0.1f);
				Rect rect3 = new Rect(butRect.x, butRect.yMax - num + 12f, butRect.width, num);
				GUI.DrawTexture(rect3, TexUI.GrayTextBG);
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect3, labelCap);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			GUI.color = Color.white;
		}
		if (Mouse.IsOver(butRect) && DoTooltip)
		{
			TipSignal tip = Desc;
			if (disabled && !disabledReason.NullOrEmpty())
			{
				tip.text += ("\n\n" + "DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable);
			}
			tip.text += DescPostfix;
			TooltipHandler.TipRegion(butRect, tip);
		}
		if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null || !Find.WindowStack.FloatMenu.windowRect.Overlaps(butRect)))
		{
			UIHighlighter.HighlightOpportunity(butRect, HighlightTag);
		}
		Text.Font = GameFont.Small;
		if (flag2)
		{
			if (disabled)
			{
				if (!disabledReason.NullOrEmpty())
				{
					Messages.Message("DisabledCommand".Translate() + ": " + disabledReason, MessageTypeDefOf.RejectInput, historical: false);
				}
				return new GizmoResult(GizmoState.Mouseover, null);
			}
			GizmoResult result;
			if (Event.current.button == 1)
			{
				result = new GizmoResult(GizmoState.OpenedFloatMenu, Event.current);
			}
			else
			{
				if (!TutorSystem.AllowAction(TutorTagSelect))
				{
					return new GizmoResult(GizmoState.Mouseover, null);
				}
				result = new GizmoResult(GizmoState.Interacted, Event.current);
				TutorSystem.Notify_Event(TutorTagSelect);
			}
			return result;
		}
		if (flag)
		{
			return new GizmoResult(GizmoState.Mouseover, null);
		}
		return new GizmoResult(GizmoState.Clear, null);
	}

	public virtual void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
	{
		Texture badTex = icon;
		if (badTex == null)
		{
			badTex = BaseContent.BadTex;
		}
		rect.position += new Vector2(iconOffset.x * rect.size.x, iconOffset.y * rect.size.y);
		if (!disabled || parms.lowLight)
		{
			GUI.color = IconDrawColor;
		}
		else
		{
			GUI.color = IconDrawColor.SaturationChanged(0f);
		}
		if (parms.lowLight)
		{
			GUI.color = GUI.color.ToTransparent(0.6f);
		}
		Widgets.DrawTextureFitted(rect, badTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, overrideMaterial ?? buttonMat);
		GUI.color = Color.white;
	}

	public override bool GroupsWith(Gizmo other)
	{
		if (!groupable)
		{
			return false;
		}
		if (!(other is Command { groupable: not false } command))
		{
			return false;
		}
		if (hotKey == command.hotKey && Label == command.Label && icon == command.icon && groupKey == command.groupKey)
		{
			return true;
		}
		if (groupKeyIgnoreContent == -1 || command.groupKeyIgnoreContent == -1)
		{
			return false;
		}
		if (groupKeyIgnoreContent == command.groupKeyIgnoreContent)
		{
			return true;
		}
		return false;
	}

	public override void ProcessInput(Event ev)
	{
		if (CurActivateSound != null)
		{
			CurActivateSound.PlayOneShotOnCamera();
		}
	}

	public override string ToString()
	{
		return "Command(label=" + defaultLabel + ", defaultDesc=" + defaultDesc + ")";
	}
}
