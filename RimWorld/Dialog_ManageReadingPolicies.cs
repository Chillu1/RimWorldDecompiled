using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_ManageReadingPolicies : Dialog_ManagePolicies<ReadingPolicy>
{
	private readonly ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

	private readonly ThingFilterUI.UIState effectFilterState = new ThingFilterUI.UIState();

	private static ThingFilter policyGlobalFilter;

	private static ThingFilter PolicyGlobalFilter
	{
		get
		{
			if (policyGlobalFilter == null)
			{
				policyGlobalFilter = new ThingFilter();
				foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.HasComp<CompBook>()))
				{
					policyGlobalFilter.SetAllow(item, allow: true);
				}
			}
			return policyGlobalFilter;
		}
	}

	protected override string TitleKey => "ReadingPolicyTitle";

	protected override string TipKey => "ReadingPolicyTip";

	public override Vector2 InitialSize => new Vector2(860f, 700f);

	public Dialog_ManageReadingPolicies(ReadingPolicy policy)
		: base(policy)
	{
	}

	public override void PreOpen()
	{
		base.PreOpen();
		thingFilterState.quickSearch.Reset();
		effectFilterState.quickSearch.Reset();
	}

	protected override ReadingPolicy CreateNewPolicy()
	{
		return Current.Game.readingPolicyDatabase.MakeNewReadingPolicy();
	}

	protected override ReadingPolicy GetDefaultPolicy()
	{
		return Current.Game.readingPolicyDatabase.DefaultReadingPolicy();
	}

	protected override void SetDefaultPolicy(ReadingPolicy policy)
	{
		Current.Game.readingPolicyDatabase.SetDefault(policy);
	}

	protected override AcceptanceReport TryDeletePolicy(ReadingPolicy policy)
	{
		return Current.Game.readingPolicyDatabase.TryDelete(policy);
	}

	protected override List<ReadingPolicy> GetPolicies()
	{
		return Current.Game.readingPolicyDatabase.AllReadingPolicies;
	}

	protected override void DoContentsRect(Rect rect)
	{
		rect.SplitVerticallyWithMargin(out var left, out var right, 10f);
		ThingFilterUI.DoThingFilterConfigWindow(left, thingFilterState, base.SelectedPolicy.defFilter, PolicyGlobalFilter, 1, null, null, forceHideHitPointsConfig: true);
		ThingFilterUI.DoThingFilterConfigWindow(right, effectFilterState, base.SelectedPolicy.effectFilter, null, 1, null, null, forceHideHitPointsConfig: true, forceHideQualityConfig: true, showMentalBreakChanceRange: true);
	}
}
