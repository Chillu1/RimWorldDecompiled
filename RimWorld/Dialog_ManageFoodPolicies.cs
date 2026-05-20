using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_ManageFoodPolicies : Dialog_ManagePolicies<FoodPolicy>
{
	private readonly ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

	private static ThingFilter foodGlobalFilter;

	public static ThingFilter FoodGlobalFilter
	{
		get
		{
			if (foodGlobalFilter == null)
			{
				foodGlobalFilter = new ThingFilter();
				foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0f))
				{
					foodGlobalFilter.SetAllow(item, allow: true);
				}
			}
			return foodGlobalFilter;
		}
	}

	protected override string TitleKey => "FoodPolicyTitle";

	protected override string TipKey => "FoodPolicyTip";

	public override Vector2 InitialSize => new Vector2(700f, 700f);

	public Dialog_ManageFoodPolicies(FoodPolicy policy)
		: base(policy)
	{
	}

	public override void PreOpen()
	{
		base.PreOpen();
		thingFilterState.quickSearch.Reset();
	}

	protected override FoodPolicy CreateNewPolicy()
	{
		return Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
	}

	protected override FoodPolicy GetDefaultPolicy()
	{
		return Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
	}

	protected override void SetDefaultPolicy(FoodPolicy policy)
	{
		Current.Game.foodRestrictionDatabase.SetDefault(policy);
	}

	protected override AcceptanceReport TryDeletePolicy(FoodPolicy policy)
	{
		return Current.Game.foodRestrictionDatabase.TryDelete(policy);
	}

	protected override List<FoodPolicy> GetPolicies()
	{
		return Current.Game.foodRestrictionDatabase.AllFoodRestrictions;
	}

	protected override void DoContentsRect(Rect rect)
	{
		ThingFilterUI.DoThingFilterConfigWindow(rect, thingFilterState, base.SelectedPolicy.filter, FoodGlobalFilter, 1, null, HiddenSpecialThingFilters(), forceHideHitPointsConfig: true);
	}

	private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
	{
		yield return SpecialThingFilterDefOf.AllowFresh;
	}
}
