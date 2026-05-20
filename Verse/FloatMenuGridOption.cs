using System;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class FloatMenuGridOption
	{
		public Texture2D texture;

		public Color color = Color.white;

		public Action action;

		public TipSignal? tooltip;

		public Rect iconTexCoords = new Rect(0f, 0f, 1f, 1f);

		public Action<Rect> postDrawAction;

		public bool Disabled => action == null;

		public MenuOptionPriority Priority
		{
			get
			{
				if (Disabled)
				{
					return MenuOptionPriority.DisabledOption;
				}
				return MenuOptionPriority.Default;
			}
		}

		public FloatMenuGridOption(Texture2D texture, Action action, Color? color = null, TipSignal? tooltip = null)
		{
			this.texture = texture;
			this.action = action;
			this.color = color ?? Color.white;
			this.tooltip = tooltip;
		}

		public bool OnGUI(Rect rect)
		{
			bool flag = !Disabled && Mouse.IsOver(rect);
			if (!Disabled)
			{
				MouseoverSounds.DoRegion(rect);
			}
			if (tooltip.HasValue)
			{
				TooltipHandler.TipRegion(rect, tooltip.Value);
			}
			Color color = GUI.color;
			if (Disabled)
			{
				GUI.color = FloatMenuOption.ColorBGDisabled * color;
			}
			else if (flag)
			{
				GUI.color = FloatMenuOption.ColorBGActiveMouseover * color;
			}
			else
			{
				GUI.color = FloatMenuOption.ColorBGActive * color;
			}
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = ((!Disabled) ? FloatMenuOption.ColorTextActive : FloatMenuOption.ColorTextDisabled) * color;
			Widgets.DrawAtlas(rect, TexUI.FloatMenuOptionBG);
			GUI.color = new Color(this.color.r, this.color.g, this.color.b, this.color.a * color.a);
			Rect rect2 = rect.ContractedBy(2f);
			if (!flag)
			{
				rect2 = rect2.ContractedBy(2f);
			}
			Material mat = (Disabled ? TexUI.GrayscaleGUI : null);
			Widgets.DrawTextureFitted(rect2, texture, 1f, new Vector2(1f, 1f), iconTexCoords, 0f, mat);
			GUI.color = color;
			postDrawAction?.Invoke(rect2);
			if (Widgets.ButtonInvisible(rect))
			{
				Chosen();
				return true;
			}
			return false;
		}

		public void Chosen()
		{
			if (!Disabled)
			{
				action?.Invoke();
			}
			else
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
			}
		}
	}
}
