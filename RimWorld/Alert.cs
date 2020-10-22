using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Alert
	{
		protected AlertPriority defaultPriority;

		protected string defaultLabel;

		protected string defaultExplanation;

		protected float lastBellTime = -1000f;

		private int jumpToTargetCycleIndex;

		private bool cachedActive;

		private string cachedLabel;

		private AlertBounce alertBounce;

		public const float Width = 154f;

		private const float TextWidth = 148f;

		private const float ItemPeekWidth = 30f;

		public const float InfoRectWidth = 330f;

		private static readonly Texture2D AlertBGTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

		private static readonly Texture2D AlertBGTexHighlight = TexUI.HighlightTex;

		private static List<GlobalTargetInfo> tmpTargets = new List<GlobalTargetInfo>();

		public virtual AlertPriority Priority => defaultPriority;

		protected virtual Color BGColor => Color.clear;

		public bool Active => cachedActive;

		public string Label
		{
			get
			{
				if (!Active)
				{
					return "";
				}
				return cachedLabel;
			}
		}

		public float Height
		{
			get
			{
				Text.Font = GameFont.Small;
				return Text.CalcHeight(Label, 148f);
			}
		}

		public abstract AlertReport GetReport();

		public virtual TaggedString GetExplanation()
		{
			return defaultExplanation;
		}

		public virtual string GetLabel()
		{
			return defaultLabel;
		}

		public void Notify_Started()
		{
			if ((int)Priority >= 1)
			{
				if (alertBounce == null)
				{
					alertBounce = new AlertBounce();
				}
				alertBounce.DoAlertStartEffect();
				if (Time.timeSinceLevelLoad > 1f && Time.realtimeSinceStartup > lastBellTime + 0.5f)
				{
					SoundDefOf.TinyBell.PlayOneShotOnCamera();
					lastBellTime = Time.realtimeSinceStartup;
				}
			}
		}

		public void Recalculate()
		{
			AlertReport report = GetReport();
			cachedActive = report.active;
			if (report.active)
			{
				cachedLabel = GetLabel();
			}
		}

		public virtual void AlertActiveUpdate()
		{
		}

		public virtual Rect DrawAt(float topY, bool minimized)
		{
			Rect rect = new Rect((float)UI.screenWidth - 154f, topY, 154f, Height);
			if (alertBounce != null)
			{
				rect.x -= alertBounce.CalculateHorizontalOffset();
			}
			GUI.color = BGColor;
			GUI.DrawTexture(rect, AlertBGTex);
			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(new Rect(0f, 0f, 148f, Height), Label);
			GUI.EndGroup();
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, AlertBGTexHighlight);
			}
			if (Widgets.ButtonInvisible(rect))
			{
				OnClick();
			}
			Text.Anchor = TextAnchor.UpperLeft;
			return rect;
		}

		protected virtual void OnClick()
		{
			IEnumerable<GlobalTargetInfo> allCulprits = GetReport().AllCulprits;
			if (allCulprits == null)
			{
				return;
			}
			tmpTargets.Clear();
			foreach (GlobalTargetInfo item in allCulprits)
			{
				if (item.IsValid)
				{
					tmpTargets.Add(item);
				}
			}
			if (tmpTargets.Any())
			{
				if (Event.current.button == 1)
				{
					jumpToTargetCycleIndex--;
				}
				else
				{
					jumpToTargetCycleIndex++;
				}
				CameraJumper.TryJumpAndSelect(tmpTargets[GenMath.PositiveMod(jumpToTargetCycleIndex, tmpTargets.Count)]);
				tmpTargets.Clear();
			}
		}

		public void DrawInfoPane()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Recalculate();
			if (!Active)
			{
				return;
			}
			TaggedString expString = GetExplanation();
			if (!expString.NullOrEmpty())
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				if (GetReport().AnyCulpritValid)
				{
					expString += "\n\n(" + "ClickToJumpToProblem".Translate() + ")";
				}
				float num = Text.CalcHeight(expString, 310f);
				num += 20f;
				Rect infoRect = new Rect((float)UI.screenWidth - 154f - 330f - 8f, Mathf.Max(Mathf.Min(Event.current.mousePosition.y, (float)UI.screenHeight - num), 0f), 330f, num);
				if (infoRect.yMax > (float)UI.screenHeight)
				{
					infoRect.y -= (float)UI.screenHeight - infoRect.yMax;
				}
				if (infoRect.y < 0f)
				{
					infoRect.y = 0f;
				}
				Find.WindowStack.ImmediateWindow(138956, infoRect, WindowLayer.GameUI, delegate
				{
					Text.Font = GameFont.Small;
					Rect rect = infoRect.AtZero();
					Widgets.DrawWindowBackground(rect);
					Rect position = rect.ContractedBy(10f);
					GUI.BeginGroup(position);
					Widgets.Label(new Rect(0f, 0f, position.width, position.height), expString);
					GUI.EndGroup();
				}, doBackground: false);
			}
		}
	}
}
