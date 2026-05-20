using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class FloatMenu : Window
{
	public bool givesColonistOrders;

	public bool vanishIfMouseDistant = true;

	public Action onCloseCallback;

	protected List<FloatMenuOption> options;

	private string title;

	private bool needSelection;

	private Color baseColor = Color.white;

	private Vector2 scrollPosition;

	private static readonly Vector2 TitleOffset = new Vector2(30f, -25f);

	private const float OptionSpacing = -1f;

	private const float MaxScreenHeightPercent = 0.9f;

	private const float MinimumColumnWidth = 70f;

	private static readonly Vector2 InitialPositionShift = new Vector2(4f, 0f);

	public const float FadeStartMouseDist = 5f;

	private const float FadeFinishMouseDist = 100f;

	public const float FinishDistFromStartDist = 95f;

	protected override float Margin => 0f;

	public override Vector2 InitialSize => new Vector2(TotalWidth, TotalWindowHeight);

	private float MaxWindowHeight => (float)UI.screenHeight * 0.9f;

	private float TotalWindowHeight => Mathf.Min(TotalViewHeight, MaxWindowHeight) + 1f;

	private float MaxViewHeight
	{
		get
		{
			if (UsingScrollbar)
			{
				float num = 0f;
				float num2 = 0f;
				for (int i = 0; i < options.Count; i++)
				{
					float requiredHeight = options[i].RequiredHeight;
					if (requiredHeight > num)
					{
						num = requiredHeight;
					}
					num2 += requiredHeight + -1f;
				}
				int columnCount = ColumnCount;
				num2 += (float)columnCount * num;
				return num2 / (float)columnCount;
			}
			return MaxWindowHeight;
		}
	}

	private float TotalViewHeight
	{
		get
		{
			float num = 0f;
			float num2 = 0f;
			float maxViewHeight = MaxViewHeight;
			for (int i = 0; i < options.Count; i++)
			{
				float requiredHeight = options[i].RequiredHeight;
				if (num2 + requiredHeight + -1f > maxViewHeight)
				{
					if (num2 > num)
					{
						num = num2;
					}
					num2 = requiredHeight;
				}
				else
				{
					num2 += requiredHeight + -1f;
				}
			}
			return Mathf.Max(num, num2);
		}
	}

	private float TotalWidth
	{
		get
		{
			float num = (float)ColumnCount * ColumnWidth;
			if (UsingScrollbar)
			{
				num += 16f;
			}
			return num;
		}
	}

	private float ColumnWidth
	{
		get
		{
			float num = 70f;
			for (int i = 0; i < options.Count; i++)
			{
				float requiredWidth = options[i].RequiredWidth;
				if (requiredWidth >= 300f)
				{
					return 300f;
				}
				if (requiredWidth > num)
				{
					num = requiredWidth;
				}
			}
			return Mathf.Round(num);
		}
	}

	private int MaxColumns => Mathf.FloorToInt(((float)UI.screenWidth - 16f) / ColumnWidth);

	private bool UsingScrollbar => ColumnCountIfNoScrollbar > MaxColumns;

	private int ColumnCount => Mathf.Min(ColumnCountIfNoScrollbar, MaxColumns);

	private int ColumnCountIfNoScrollbar
	{
		get
		{
			if (options == null)
			{
				return 1;
			}
			Text.Font = GameFont.Small;
			int num = 1;
			float num2 = 0f;
			float maxWindowHeight = MaxWindowHeight;
			for (int i = 0; i < options.Count; i++)
			{
				float requiredHeight = options[i].RequiredHeight;
				if (num2 + requiredHeight + -1f > maxWindowHeight)
				{
					num2 = requiredHeight;
					num++;
				}
				else
				{
					num2 += requiredHeight + -1f;
				}
			}
			return num;
		}
	}

	public FloatMenuSizeMode SizeMode
	{
		get
		{
			if (options.Count > 60)
			{
				return FloatMenuSizeMode.Tiny;
			}
			return FloatMenuSizeMode.Normal;
		}
	}

	public FloatMenu(List<FloatMenuOption> options)
	{
		if (options.NullOrEmpty())
		{
			Log.Error("Created FloatMenu with no options. Closing.");
			Close();
		}
		this.options = (from op in options
			orderby op.Priority descending, op.orderInPriority descending
			select op).ToList();
		for (int num = 0; num < options.Count; num++)
		{
			options[num].SetSizeMode(SizeMode);
		}
		layer = WindowLayer.Super;
		closeOnClickedOutside = true;
		doWindowBackground = false;
		drawShadow = false;
		preventCameraMotion = false;
		SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera();
	}

	public FloatMenu(List<FloatMenuOption> options, string title, bool needSelection = false)
		: this(options)
	{
		this.title = title;
		this.needSelection = needSelection;
	}

	protected override void SetInitialSizeAndPosition()
	{
		Vector2 vector = UI.MousePositionOnUIInverted + InitialPositionShift;
		if (vector.x + InitialSize.x > (float)UI.screenWidth)
		{
			vector.x = (float)UI.screenWidth - InitialSize.x;
		}
		if (vector.y + InitialSize.y > (float)UI.screenHeight)
		{
			vector.y = (float)UI.screenHeight - InitialSize.y;
		}
		windowRect = new Rect(vector.x, vector.y, InitialSize.x, InitialSize.y);
	}

	public override void ExtraOnGUI()
	{
		base.ExtraOnGUI();
		if (!title.NullOrEmpty())
		{
			Vector2 vector = new Vector2(windowRect.x, windowRect.y);
			Text.Font = GameFont.Small;
			float width = Mathf.Max(150f, 15f + Text.CalcSize(title).x);
			Rect titleRect = new Rect(vector.x + TitleOffset.x, vector.y + TitleOffset.y, width, 23f);
			Find.WindowStack.ImmediateWindow(6830963, titleRect, WindowLayer.Super, delegate
			{
				GUI.color = baseColor;
				Text.Font = GameFont.Small;
				Rect position = titleRect.AtZero();
				position.width = 150f;
				GUI.DrawTexture(position, TexUI.TextBGBlack);
				Rect rect = titleRect.AtZero();
				rect.x += 15f;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect, title);
				Text.Anchor = TextAnchor.UpperLeft;
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}
	}

	public override void DoWindowContents(Rect rect)
	{
		if (needSelection && Find.Selector.SingleSelectedThing == null)
		{
			Find.WindowStack.TryRemove(this);
			return;
		}
		UpdateBaseColor();
		bool usingScrollbar = UsingScrollbar;
		GUI.color = baseColor;
		Text.Font = GameFont.Small;
		Vector2 zero = Vector2.zero;
		float maxViewHeight = MaxViewHeight;
		float columnWidth = ColumnWidth;
		if (usingScrollbar)
		{
			rect.width -= 10f;
			Widgets.BeginScrollView(rect, ref scrollPosition, new Rect(0f, 0f, TotalWidth - 16f, TotalViewHeight));
		}
		for (int i = 0; i < options.Count; i++)
		{
			FloatMenuOption floatMenuOption = options[i];
			float requiredHeight = floatMenuOption.RequiredHeight;
			if (zero.y + requiredHeight + -1f > maxViewHeight)
			{
				zero.y = 0f;
				zero.x += columnWidth + -1f;
			}
			Rect rect2 = new Rect(zero.x, zero.y, columnWidth, requiredHeight);
			zero.y += requiredHeight + -1f;
			if (floatMenuOption.DoGUI(rect2, givesColonistOrders, this))
			{
				Find.WindowStack.TryRemove(this);
				break;
			}
		}
		if (usingScrollbar)
		{
			Widgets.EndScrollView();
		}
		if (Event.current.type == EventType.MouseDown)
		{
			Event.current.Use();
			Close();
		}
		GUI.color = Color.white;
	}

	public override void PostClose()
	{
		base.PostClose();
		if (onCloseCallback != null)
		{
			onCloseCallback();
		}
	}

	public void Cancel()
	{
		SoundDefOf.FloatMenu_Cancel.PlayOneShotOnCamera();
		Find.WindowStack.TryRemove(this);
	}

	public virtual void PreOptionChosen(FloatMenuOption opt)
	{
	}

	private void UpdateBaseColor()
	{
		baseColor = Color.white;
		if (!vanishIfMouseDistant)
		{
			return;
		}
		Rect r = new Rect(0f, 0f, TotalWidth, TotalWindowHeight).ContractedBy(-5f);
		if (!r.Contains(Event.current.mousePosition))
		{
			float num = GenUI.DistFromRect(r, Event.current.mousePosition);
			baseColor = new Color(1f, 1f, 1f, 1f - num / 95f);
			if (num > 95f)
			{
				Close(doCloseSound: false);
				Cancel();
			}
		}
	}
}
