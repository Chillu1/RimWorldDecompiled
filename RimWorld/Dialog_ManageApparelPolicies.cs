using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_ManageApparelPolicies : Dialog_ManagePolicies<ApparelPolicy>
{
	private readonly ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

	private static ThingFilter apparelGlobalFilter;

	private static ThingFilter ApparelGlobalFilter
	{
		get
		{
			if (apparelGlobalFilter == null)
			{
				apparelGlobalFilter = new ThingFilter();
				apparelGlobalFilter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
			}
			return apparelGlobalFilter;
		}
	}

	protected override string TitleKey => "ApparelPolicyTitle";

	protected override string TipKey => "ApparelPolicyTip";

	public override Vector2 InitialSize => new Vector2(700f, 700f);

	public Dialog_ManageApparelPolicies(ApparelPolicy policy)
		: base(policy)
	{
	}

	public override void PreOpen()
	{
		base.PreOpen();
		thingFilterState.quickSearch.Reset();
	}

	protected override ApparelPolicy CreateNewPolicy()
	{
		return Current.Game.outfitDatabase.MakeNewOutfit();
	}

	protected override ApparelPolicy GetDefaultPolicy()
	{
		return Current.Game.outfitDatabase.DefaultOutfit();
	}

	protected override void SetDefaultPolicy(ApparelPolicy policy)
	{
		Current.Game.outfitDatabase.SetDefault(policy);
	}

	protected override AcceptanceReport TryDeletePolicy(ApparelPolicy policy)
	{
		return Current.Game.outfitDatabase.TryDelete(policy);
	}

	protected override List<ApparelPolicy> GetPolicies()
	{
		return Current.Game.outfitDatabase.AllOutfits;
	}

	protected override void DoContentsRect(Rect rect)
	{
		ThingFilterUI.DoThingFilterConfigWindow(rect, thingFilterState, base.SelectedPolicy.filter, ApparelGlobalFilter, 16, null, HiddenSpecialThingFilters());
	}

	private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
	{
		yield return SpecialThingFilterDefOf.AllowNonDeadmansApparel;
		if (ModsConfig.IdeologyActive)
		{
			yield return SpecialThingFilterDefOf.AllowVegetarian;
			yield return SpecialThingFilterDefOf.AllowCarnivore;
			yield return SpecialThingFilterDefOf.AllowCannibal;
			yield return SpecialThingFilterDefOf.AllowInsectMeat;
		}
	}
}
