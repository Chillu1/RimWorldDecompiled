using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Page_SelectStorytellerInGame : Page
{
	private Listing_Standard selectedStorytellerInfoListing = new Listing_Standard();

	public override string PageTitle => "ChooseAIStoryteller".Translate();

	public Page_SelectStorytellerInGame()
	{
		doCloseButton = true;
		doCloseX = true;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		StorytellerUI.ResetStorytellerSelectionInterface();
	}

	public override void DoWindowContents(Rect rect)
	{
		DrawPageTitle(rect);
		Rect mainRect = GetMainRect(rect);
		Storyteller storyteller = Current.Game.storyteller;
		StorytellerDef def = Current.Game.storyteller.def;
		StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref storyteller.def, ref storyteller.difficultyDef, ref storyteller.difficulty, selectedStorytellerInfoListing);
		if (storyteller.def != def)
		{
			storyteller.Notify_DefChanged();
		}
	}

	public override void PreClose()
	{
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.costListForDifficulty != null))
		{
			item.costListForDifficulty.RecacheApplies();
		}
		RecipeDefGenerator.ResetRecipeIngredientsForDifficulty();
	}
}
