using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	[StaticConstructorOnStartup]
	public abstract class Command : Gizmo
	{
		public string defaultLabel;

		public string defaultDesc = "No description.";

		public Texture2D icon;

		public float iconAngle;

		public Vector2 iconProportions = Vector2.one;

		public Rect iconTexCoords = new Rect(0f, 0f, 1f, 1f);

		public float iconDrawScale = 1f;

		public Vector2 iconOffset;

		public Color defaultIconColor = Color.white;

		public KeyBindingDef hotKey;

		public SoundDef activateSound;

		public int groupKey;

		public string tutorTag = "TutorTagNotSet";

		public bool shrinkable;

		public static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

		public static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

		protected const float InnerIconDrawScale = 0.85f;

		public virtual string Label => defaultLabel;

		public virtual string LabelCap => Label.CapitalizeFirst();

		public virtual string TopRightLabel => null;

		public virtual string Desc => defaultDesc;

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

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			return GizmoOnGUIInt(new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f));
		}

		public virtual GizmoResult GizmoOnGUIShrunk(Vector2 topLeft, float size)
		{
			return GizmoOnGUIInt(new Rect(topLeft.x, topLeft.y, size, size), shrunk: true);
		}

		protected virtual GizmoResult GizmoOnGUIInt(Rect butRect, bool shrunk = false)
		{
			Text.Font = GameFont.Tiny;
			bool flag = false;
			if (Mouse.IsOver(butRect))
			{
				flag = true;
				if (!disabled)
				{
					GUI.color = GenUI.MouseoverColor;
				}
			}
			MouseoverSounds.DoRegion(butRect, SoundDefOf.Mouseover_Command);
			Material material = (disabled ? TexUI.GrayscaleGUI : null);
			GenUI.DrawTextureWithMaterial(butRect, shrunk ? BGTextureShrunk : BGTexture, material);
			DrawIcon(butRect, material);
			bool flag2 = false;
			KeyCode keyCode = ((hotKey != null) ? hotKey.MainKey : KeyCode.None);
			if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
			{
				Vector2 vector = (shrunk ? new Vector2(3f, 0f) : new Vector2(5f, 3f));
				Widgets.Label(new Rect(butRect.x + vector.x, butRect.y + vector.y, butRect.width - 10f, 18f), keyCode.ToStringReadable());
				GizmoGridDrawer.drawnHotKeys.Add(keyCode);
				if (hotKey.KeyDownEvent)
				{
					flag2 = true;
					Event.current.Use();
				}
			}
			if (Widgets.ButtonInvisible(butRect))
			{
				flag2 = true;
			}
			if (!shrunk)
			{
				string topRightLabel = TopRightLabel;
				if (!topRightLabel.NullOrEmpty())
				{
					Vector2 vector2 = Text.CalcSize(topRightLabel);
					Rect position;
					Rect rect = (position = new Rect(butRect.xMax - vector2.x - 2f, butRect.y + 3f, vector2.x, vector2.y));
					position.x -= 2f;
					position.width += 3f;
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperRight;
					GUI.DrawTexture(position, TexUI.GrayTextBG);
					Widgets.Label(rect, topRightLabel);
					Text.Anchor = TextAnchor.UpperLeft;
				}
				string labelCap = LabelCap;
				if (!labelCap.NullOrEmpty())
				{
					float num = Text.CalcHeight(labelCap, butRect.width);
					Rect rect2 = new Rect(butRect.x, butRect.yMax - num + 12f, butRect.width, num);
					GUI.DrawTexture(rect2, TexUI.GrayTextBG);
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperCenter;
					Widgets.Label(rect2, labelCap);
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
				}
				GUI.color = Color.white;
			}
			if (Mouse.IsOver(butRect) && DoTooltip)
			{
				TipSignal tip = Desc;
				if (disabled && !disabledReason.NullOrEmpty())
				{
					ref string text = ref tip.text;
					text += "\n\n" + "DisabledCommand".Translate() + ": " + disabledReason;
				}
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
						Messages.Message(disabledReason, MessageTypeDefOf.RejectInput, historical: false);
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

		protected virtual void DrawIcon(Rect rect, Material buttonMat = null)
		{
			Texture2D badTex = icon;
			if (badTex == null)
			{
				badTex = BaseContent.BadTex;
			}
			rect.position += new Vector2(iconOffset.x * rect.size.x, iconOffset.y * rect.size.y);
			GUI.color = IconDrawColor;
			Widgets.DrawTextureFitted(rect, badTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, buttonMat);
			GUI.color = Color.white;
		}

		public override bool GroupsWith(Gizmo other)
		{
			Command command = other as Command;
			if (command == null)
			{
				return false;
			}
			if (hotKey == command.hotKey && Label == command.Label && icon == command.icon)
			{
				return true;
			}
			if (groupKey == 0 || command.groupKey == 0)
			{
				return false;
			}
			if (groupKey == command.groupKey)
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
}
