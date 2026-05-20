using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class FloatMenuGrid : Window
	{
		private List<FloatMenuGridOption> options;

		private int calculatedSquareSize;

		private Color baseColor = Color.white;

		public Action onCloseCallback;

		private static readonly Vector2 OptionSize = new Vector2(34f, 34f);

		public override Vector2 InitialSize => new Vector2(TotalWidth, TotalHeight);

		public float TotalWidth => (float)calculatedSquareSize * (OptionSize.x - 1f);

		public float TotalHeight => (float)calculatedSquareSize * (OptionSize.y - 1f);

		protected override float Margin => 0f;

		public FloatMenuGrid(List<FloatMenuGridOption> options)
		{
			this.options = options.OrderByDescending((FloatMenuGridOption op) => op.Priority).ToList();
			layer = WindowLayer.Super;
			closeOnClickedOutside = true;
			doWindowBackground = false;
			drawShadow = false;
			preventCameraMotion = false;
			calculatedSquareSize = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(Mathf.Round(Mathf.Sqrt(options.Count)), 2f)));
			SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera();
		}

		protected override void SetInitialSizeAndPosition()
		{
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			if (mousePositionOnUIInverted.x + InitialSize.x > (float)UI.screenWidth)
			{
				mousePositionOnUIInverted.x = (float)UI.screenWidth - InitialSize.x;
			}
			if (mousePositionOnUIInverted.y > (float)UI.screenHeight)
			{
				mousePositionOnUIInverted.y = UI.screenHeight;
			}
			windowRect = new Rect(mousePositionOnUIInverted.x, mousePositionOnUIInverted.y - InitialSize.y, InitialSize.x, InitialSize.y);
		}

		public override void DoWindowContents(Rect rect)
		{
			UpdateBaseColor();
			GUI.color = baseColor;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < options.Count; i++)
			{
				FloatMenuGridOption floatMenuGridOption = options[i];
				float num3 = (float)num * OptionSize.x;
				float num4 = (float)num2 * OptionSize.y;
				if (num3 > 0f)
				{
					num3 -= (float)num;
				}
				if (num4 > 0f)
				{
					num4 -= (float)num2;
				}
				if (floatMenuGridOption.OnGUI(new Rect(num3, num4, OptionSize.x, OptionSize.y)))
				{
					Find.WindowStack.TryRemove(this);
					break;
				}
				num++;
				if (num >= calculatedSquareSize)
				{
					num = 0;
					num2++;
				}
			}
			GUI.color = Color.white;
		}

		private void UpdateBaseColor()
		{
			baseColor = Color.white;
			Rect r = new Rect(0f, 0f, TotalWidth, TotalHeight).ExpandedBy(5f);
			if (!r.Contains(Event.current.mousePosition))
			{
				float num = GenUI.DistFromRect(r, Event.current.mousePosition);
				baseColor = new Color(1f, 1f, 1f, 1f - num / 95f);
				if (num > 95f)
				{
					Close(doCloseSound: false);
					SoundDefOf.FloatMenu_Cancel.PlayOneShotOnCamera();
					Find.WindowStack.TryRemove(this);
				}
			}
		}

		public override void PostClose()
		{
			base.PostClose();
			onCloseCallback?.Invoke();
		}
	}
}
