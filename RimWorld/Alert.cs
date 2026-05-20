using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class Alert
{
	protected AlertPriority defaultPriority;

	protected string defaultLabel;

	protected string defaultExplanation;

	protected bool requireRoyalty;

	protected bool requireIdeology;

	protected bool requireBiotech;

	protected bool requireAnomaly;

	protected float lastBellTime = -1000f;

	private int jumpToTargetCycleIndex = -1;

	private bool cachedActive;

	private string cachedLabel;

	private AlertBounce alertBounce;

	public const float Width = 154f;

	private const float TextWidth = 148f;

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
				return string.Empty;
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

	public bool EnabledWithActiveExpansions
	{
		get
		{
			if (requireRoyalty && !ModsConfig.RoyaltyActive)
			{
				return false;
			}
			if (requireIdeology && !ModsConfig.IdeologyActive)
			{
				return false;
			}
			if (requireBiotech && !ModsConfig.BiotechActive)
			{
				return false;
			}
			if (requireAnomaly && !ModsConfig.AnomalyActive)
			{
				return false;
			}
			return true;
		}
	}

	public virtual string GetJumpToTargetsText => "ClickToJumpToProblem".Translate();

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
		Widgets.BeginGroup(rect);
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(new Rect(0f, 0f, 148f, Height), Label);
		Widgets.EndGroup();
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
				expString += "\n\n(" + GetJumpToTargetsText + ")";
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
			Find.WindowStack.ImmediateWindow(138956, infoRect, WindowLayer.Super, delegate
			{
				Text.Font = GameFont.Small;
				Rect rect = infoRect.AtZero();
				Widgets.DrawWindowBackground(rect);
				Rect rect2 = rect.ContractedBy(10f);
				Widgets.BeginGroup(rect2);
				Widgets.Label(new Rect(0f, 0f, rect2.width, rect2.height), expString);
				Widgets.EndGroup();
			}, doBackground: false);
		}
	}
}
