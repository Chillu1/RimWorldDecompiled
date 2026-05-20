using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Listing_Tree : Listing_Lines
{
	public float nestIndentWidth = 11f;

	protected const float OpenCloseWidgetSize = 18f;

	protected virtual float LabelWidth => base.ColumnWidth - 26f;

	protected float EditAreaWidth => base.ColumnWidth - LabelWidth;

	public override void Begin(Rect rect)
	{
		base.Begin(rect);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
	}

	public override void End()
	{
		base.End();
		Text.WordWrap = true;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	protected float XAtIndentLevel(int indentLevel)
	{
		return (float)indentLevel * nestIndentWidth;
	}

	protected void LabelLeft(string label, string tipText, int indentLevel, float widthOffset = 0f, Color? textColor = null, float leftOffset = 0f)
	{
		Rect rect = new Rect(0f, curY, base.ColumnWidth, lineHeight);
		rect.xMin = XAtIndentLevel(indentLevel) + 18f + leftOffset;
		Widgets.DrawHighlightIfMouseover(rect);
		if (!tipText.NullOrEmpty())
		{
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			TooltipHandler.TipRegion(rect, tipText);
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		GUI.color = textColor ?? Color.white;
		rect.width = LabelWidth - rect.xMin + widthOffset;
		rect.yMax += 5f;
		rect.yMin -= 5f;
		Widgets.Label(rect, label.Truncate(rect.width));
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
	}

	protected bool OpenCloseWidget(TreeNode node, int indentLevel, int openMask)
	{
		if (!node.Openable)
		{
			return false;
		}
		float x = XAtIndentLevel(indentLevel);
		float y = curY + lineHeight / 2f - 9f;
		Rect butRect = new Rect(x, y, 18f, 18f);
		bool flag = IsOpen(node, openMask);
		Texture2D tex = (flag ? TexButton.Collapse : TexButton.Reveal);
		if (Widgets.ButtonImage(butRect, tex))
		{
			if (flag)
			{
				SoundDefOf.TabClose.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.TabOpen.PlayOneShotOnCamera();
			}
			node.SetOpen(openMask, !flag);
			return true;
		}
		return false;
	}

	public virtual bool IsOpen(TreeNode node, int openMask)
	{
		if (node.IsOpen(openMask))
		{
			return true;
		}
		return false;
	}

	public void InfoText(string text, int indentLevel)
	{
		Text.WordWrap = true;
		Rect rect = new Rect(0f, curY, base.ColumnWidth, 50f);
		rect.xMin = LabelWidth;
		rect.height = Text.CalcHeight(text, rect.width);
		Widgets.Label(rect, text);
		curY += rect.height;
		Text.WordWrap = false;
	}

	public bool ButtonText(string label)
	{
		Text.WordWrap = true;
		float num = Text.CalcHeight(label, base.ColumnWidth);
		bool result = Widgets.ButtonText(new Rect(0f, curY, base.ColumnWidth, num), label);
		curY += num + 0f;
		Text.WordWrap = false;
		return result;
	}

	public WidgetRow StartWidgetsRow(int indentLevel)
	{
		WidgetRow result = new WidgetRow(LabelWidth, curY);
		curY += 24f;
		return result;
	}
}
