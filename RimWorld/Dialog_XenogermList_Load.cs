using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_XenogermList_Load : Window
{
	protected float bottomAreaHeight;

	protected Vector2 scrollPosition = Vector2.zero;

	private Action<CustomXenogerm> loadAction;

	protected const float EntryHeight = 40f;

	protected const float NameLeftMargin = 8f;

	protected const float NameRightMargin = 4f;

	protected const float InfoWidth = 94f;

	protected const float InteractButWidth = 100f;

	protected const float InteractButHeight = 36f;

	protected const float DeleteButSize = 36f;

	public Dialog_XenogermList_Load(Action<CustomXenogerm> loadAction)
	{
		doCloseButton = true;
		doCloseX = true;
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnAccept = false;
		this.loadAction = loadAction;
	}

	public override void DoWindowContents(Rect inRect)
	{
		List<CustomXenogerm> customXenogermsForReading = Find.CustomXenogermDatabase.CustomXenogermsForReading;
		Vector2 vector = new Vector2(inRect.width - 16f, 40f);
		float y = vector.y;
		float height = (float)customXenogermsForReading.Count * y;
		Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
		float height2 = inRect.height - Window.CloseButSize.y - bottomAreaHeight - 18f;
		Rect outRect = inRect.TopPartPixels(height2);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float num = 0f;
		for (int num2 = customXenogermsForReading.Count - 1; num2 >= 0; num2--)
		{
			CustomXenogerm xenogerm = customXenogermsForReading[num2];
			if (num + vector.y >= scrollPosition.y && num <= scrollPosition.y + outRect.height)
			{
				Rect rect = new Rect(0f, num, vector.x, vector.y);
				if (num2 % 2 == 0)
				{
					Widgets.DrawAltRect(rect);
				}
				Widgets.BeginGroup(rect);
				Rect rect2 = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
				if (Widgets.ButtonImage(rect2, TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(xenogerm.name), delegate
					{
						Find.CustomXenogermDatabase.Remove(xenogerm);
					}, destructive: true));
				}
				TooltipHandler.TipRegionByKey(rect2, "DeleteThisXenogerm");
				Text.Font = GameFont.Small;
				if (Widgets.ButtonText(new Rect(rect2.x - 100f, (rect.height - 36f) / 2f, 100f, 36f), "LoadGameButton".Translate()))
				{
					loadAction(xenogerm);
					Close();
				}
				GUI.color = Color.white;
				Rect rect3 = new Rect(8f, 0f, rect.width - 4f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Text.Font = GameFont.Small;
				Widgets.Label(rect3, xenogerm.name.Truncate(rect3.width * 1.8f));
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.EndGroup();
			}
			num += vector.y;
		}
		Widgets.EndScrollView();
	}
}
