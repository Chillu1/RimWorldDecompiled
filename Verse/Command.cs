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

		public static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

		protected const float InnerIconDrawScale = 0.85f;

		public virtual string Label => defaultLabel;

		public virtual string LabelCap => Label.CapitalizeFirst();

		public virtual string Desc => defaultDesc;

		public virtual Color IconDrawColor => defaultIconColor;

		public virtual SoundDef CurActivateSound => activateSound;

		protected virtual bool DoTooltip => true;

		public virtual string HighlightTag => tutorTag;

		public virtual string TutorTagSelect => tutorTag;

		public virtual Texture2D BGTexture => BGTex;

		public override float GetWidth(float maxWidth)
		{
			return 75f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Text.Font = GameFont.Tiny;
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			bool flag = false;
			if (Mouse.IsOver(rect))
			{
				flag = true;
				if (!disabled)
				{
					GUI.color = GenUI.MouseoverColor;
				}
			}
			MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
			Material material = disabled ? TexUI.GrayscaleGUI : null;
			GenUI.DrawTextureWithMaterial(rect, BGTexture, material);
			DrawIcon(rect, material);
			bool flag2 = false;
			KeyCode keyCode = (hotKey != null) ? hotKey.MainKey : KeyCode.None;
			if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
			{
				Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 18f), keyCode.ToStringReadable());
				GizmoGridDrawer.drawnHotKeys.Add(keyCode);
				if (hotKey.KeyDownEvent)
				{
					flag2 = true;
					Event.current.Use();
				}
			}
			if (Widgets.ButtonInvisible(rect))
			{
				flag2 = true;
			}
			string labelCap = LabelCap;
			if (!labelCap.NullOrEmpty())
			{
				float num = Text.CalcHeight(labelCap, rect.width);
				Rect rect2 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
				GUI.DrawTexture(rect2, TexUI.GrayTextBG);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect2, labelCap);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			GUI.color = Color.white;
			if (Mouse.IsOver(rect) && DoTooltip)
			{
				TipSignal tip = Desc;
				if (disabled && !disabledReason.NullOrEmpty())
				{
					ref string text = ref tip.text;
					text += "\n\n" + "DisabledCommand".Translate() + ": " + disabledReason;
				}
				TooltipHandler.TipRegion(rect, tip);
			}
			if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null || !Find.WindowStack.FloatMenu.windowRect.Overlaps(rect)))
			{
				UIHighlighter.HighlightOpportunity(rect, HighlightTag);
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
