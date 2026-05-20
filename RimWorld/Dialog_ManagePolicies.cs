using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Dialog_ManagePolicies<T> : Window where T : Policy
{
	private readonly ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

	private readonly QuickSearchWidget quickSearch = new QuickSearchWidget();

	private Vector2 scroll;

	private T policyInt;

	private const float LeftPanelWidth = 200f;

	private const float PolicyHeight = 32f;

	protected T SelectedPolicy
	{
		get
		{
			return policyInt;
		}
		set
		{
			ValidateName();
			policyInt = value;
		}
	}

	protected abstract string TitleKey { get; }

	protected abstract string TipKey { get; }

	public override Vector2 InitialSize => new Vector2(700f, 700f);

	protected virtual float OffsetHeaderY => 0f;

	private void ValidateName()
	{
		if (SelectedPolicy != null && SelectedPolicy.label.NullOrEmpty())
		{
			SelectedPolicy.label = "UnnamedPolicy".Translate();
		}
	}

	protected Dialog_ManagePolicies(T policy)
	{
		forcePause = true;
		doCloseX = true;
		doCloseButton = true;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
		scroll = Vector2.zero;
		quickSearch.Reset();
		SelectedPolicy = policy;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		thingFilterState.quickSearch.Reset();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Rect rect = inRect;
		rect.height = 32f;
		Rect rect2 = rect;
		rect2.y = rect.yMax;
		Rect rect3 = rect2;
		rect3.y = rect2.yMax + 10f;
		rect3.height = 32f;
		rect3.width = 200f;
		Rect rect4 = rect3;
		rect4.x = rect3.xMax + 10f;
		rect4.xMax = inRect.xMax;
		Rect rect5 = rect4;
		rect5.xMax -= 100f;
		Rect rect6 = rect4;
		rect6.x = rect4.xMax - rect4.height;
		rect6.width = rect4.height;
		Rect rect7 = rect6;
		rect7.x = rect6.x - rect4.height - 10f;
		Rect rect8 = rect7;
		rect8.x = rect7.x - rect4.height - 10f;
		Rect rect9 = rect8;
		rect9.x = rect8.x - rect4.height - 10f;
		Rect leftRect = rect3;
		leftRect.y = rect3.yMax + OffsetHeaderY + 4f;
		leftRect.yMax = inRect.yMax - Window.CloseButSize.y - 10f;
		Rect rect10 = rect4;
		rect10.y = leftRect.y;
		rect10.yMax = leftRect.yMax;
		Rect rect11 = rect4;
		rect11.yMax = rect10.yMax;
		using (new TextBlock(GameFont.Medium))
		{
			Widgets.Label(rect, TitleKey.Translate());
		}
		Text.Font = GameFont.Small;
		Widgets.Label(rect2, TipKey.Translate());
		using (new TextBlock(GameFont.Medium))
		{
			Widgets.Label(rect3, "AvailablePolicies".Translate());
		}
		DoPolicyListing(leftRect);
		if (SelectedPolicy != null)
		{
			DoContentsRect(rect10);
			bool flag = GetDefaultPolicy() == SelectedPolicy;
			string text = SelectedPolicy.label;
			if (flag)
			{
				text += string.Format(" ({0})", "default".Translate()).Colorize(Color.gray);
			}
			using (new TextBlock(GameFont.Medium))
			{
				Widgets.LabelEllipses(rect5, text);
			}
			TooltipHandler.TipRegionByKey(rect6, "DeletePolicyTip");
			TooltipHandler.TipRegionByKey(rect7, "DuplicatePolicyTip");
			TooltipHandler.TipRegionByKey(rect8, "RenamePolicyTip");
			if (!flag)
			{
				TooltipHandler.TipRegionByKey(rect9, "DefaultPolicyTip");
			}
			if (Widgets.ButtonImage(rect6, TexUI.DismissTex))
			{
				if (Input.GetKey(KeyCode.LeftControl))
				{
					DeletePolicy();
				}
				else
				{
					TaggedString taggedString = "DeletePolicyConfirm".Translate(SelectedPolicy.label);
					TaggedString taggedString2 = "DeletePolicyConfirmButton".Translate();
					Find.WindowStack.Add(new Dialog_Confirm(taggedString, taggedString2, DeletePolicy));
				}
			}
			if (Widgets.ButtonImage(rect7, TexUI.CopyTex))
			{
				T val = CreateNewPolicy();
				val.CopyFrom(SelectedPolicy);
				SelectedPolicy = val;
			}
			if (Widgets.ButtonImage(rect8, TexUI.RenameTex))
			{
				Find.WindowStack.Add(new Dialog_RenamePolicy(SelectedPolicy));
			}
			if (!flag && Widgets.ButtonImage(rect9, TexUI.MakeDefault))
			{
				SetDefaultPolicy(SelectedPolicy);
			}
			return;
		}
		using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, Color.gray))
		{
			Widgets.Label(rect11, "NoPolicySelected".Translate());
		}
	}

	private void DeletePolicy()
	{
		AcceptanceReport acceptanceReport = TryDeletePolicy(SelectedPolicy);
		if (!acceptanceReport.Accepted)
		{
			Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
		}
		else
		{
			SelectedPolicy = null;
		}
	}

	private void DoPolicyListing(Rect leftRect)
	{
		Rect rect = leftRect;
		rect.y = leftRect.yMax - 24f;
		rect.height = 24f;
		Rect rect2 = leftRect;
		rect2.yMax = rect.y - 10f;
		Rect rect3 = rect2;
		rect3.xMin += 10f;
		rect3.xMax -= 10f;
		rect3.y = rect2.yMax - Window.CloseButSize.y - 10f;
		rect3.height = Window.CloseButSize.y;
		Rect outRect = rect2;
		outRect.yMax = rect3.y - 10f;
		quickSearch.OnGUI(rect);
		Widgets.DrawMenuSection(rect2);
		if (Widgets.ButtonText(rect3, "NewPolicy".Translate()))
		{
			SelectedPolicy = CreateNewPolicy();
		}
		int num = 0;
		foreach (T policy in GetPolicies())
		{
			if (quickSearch.filter.Matches(policy.label))
			{
				num++;
			}
		}
		Rect viewRect = new Rect(0f, 0f, outRect.width, (float)num * 32f);
		Widgets.AdjustRectsForScrollView(rect2, ref outRect, ref viewRect);
		Widgets.BeginScrollView(outRect, ref scroll, viewRect);
		float num2 = 0f;
		int num3 = 0;
		T defaultPolicy = GetDefaultPolicy();
		foreach (T item in from x in GetPolicies()
			orderby defaultPolicy != x, x.label
			select x)
		{
			if (quickSearch.filter.Matches(item.label))
			{
				Rect rect4 = new Rect(0f, num2, outRect.width, 32f);
				Rect rect5 = rect4;
				rect5.x += 10f;
				num2 += 32f;
				if (SelectedPolicy == item)
				{
					Widgets.DrawHighlightSelected(rect4);
				}
				else if (Mouse.IsOver(rect4))
				{
					Widgets.DrawHighlight(rect4);
				}
				else if (num3 % 2 == 1)
				{
					Widgets.DrawLightHighlight(rect4);
				}
				num3++;
				string text = item.label;
				if (defaultPolicy == item)
				{
					text += "*".Colorize(Color.gray);
				}
				using (new TextBlock(TextAnchor.MiddleLeft))
				{
					Widgets.Label(rect5, text);
				}
				if (Widgets.ButtonInvisible(rect4))
				{
					SelectedPolicy = item;
				}
			}
		}
		Widgets.EndScrollView();
	}

	protected abstract T CreateNewPolicy();

	protected abstract T GetDefaultPolicy();

	protected abstract void SetDefaultPolicy(T policy);

	protected abstract AcceptanceReport TryDeletePolicy(T policy);

	protected abstract List<T> GetPolicies();

	protected abstract void DoContentsRect(Rect rect);

	public override void PreClose()
	{
		base.PreClose();
		ValidateName();
	}
}
