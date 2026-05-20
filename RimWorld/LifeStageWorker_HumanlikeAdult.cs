using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class LifeStageWorker_HumanlikeAdult : LifeStageWorker
{
	public const int VatGrowBackstoryTicks = 1200000;

	private static readonly List<BackstoryCategoryFilter> VatgrowBackstoryFilter = new List<BackstoryCategoryFilter>
	{
		new BackstoryCategoryFilter
		{
			categories = new List<string> { "VatGrown" }
		}
	};

	private static readonly List<BackstoryCategoryFilter> BackstoryFiltersTribal = new List<BackstoryCategoryFilter>
	{
		new BackstoryCategoryFilter
		{
			categories = new List<string> { "AdultTribal" }
		}
	};

	private static readonly List<BackstoryCategoryFilter> BackstoryFiltersColonist = new List<BackstoryCategoryFilter>
	{
		new BackstoryCategoryFilter
		{
			categories = new List<string> { "AdultColonist" }
		}
	};

	public override void Notify_LifeStageStarted(Pawn pawn, LifeStageDef previousLifeStage)
	{
		base.Notify_LifeStageStarted(pawn, previousLifeStage);
		if (Current.ProgramState != ProgramState.Playing)
		{
			return;
		}
		if (pawn.Spawned && previousLifeStage != null && previousLifeStage.developmentalStage.Juvenile())
		{
			EffecterDefOf.Birthday.SpawnAttached(pawn, pawn.Map);
		}
		if (pawn.story.bodyType == BodyTypeDefOf.Child || pawn.story.bodyType == BodyTypeDefOf.Baby)
		{
			pawn.apparel?.DropAllOrMoveAllToInventory((Apparel apparel) => !apparel.def.apparel.developmentalStageFilter.Has(DevelopmentalStage.Adult));
			BodyTypeDef bodyTypeFor = PawnGenerator.GetBodyTypeFor(pawn);
			pawn.story.bodyType = bodyTypeFor;
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		if (!pawn.IsColonist)
		{
			return;
		}
		List<BackstoryCategoryFilter> backstoryCategories = ((Faction.OfPlayer.def == FactionDefOf.PlayerTribe) ? BackstoryFiltersTribal : BackstoryFiltersColonist);
		if (previousLifeStage.developmentalStage.Juvenile())
		{
			if (pawn.ageTracker.vatGrowTicks >= 1200000)
			{
				PawnBioAndNameGenerator.FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, VatgrowBackstoryFilter, pawn.Faction?.def);
			}
			else
			{
				BackstoryDef backstory = pawn.story.GetBackstory(BackstorySlot.Childhood);
				if (backstory != null && backstory.IsPlayerColonyChildBackstory)
				{
					PawnBioAndNameGenerator.FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, backstoryCategories, pawn.Faction?.def);
				}
			}
		}
		if (pawn.story.GetBackstory(BackstorySlot.Adulthood) == null)
		{
			PawnBioAndNameGenerator.FillBackstorySlotShuffled(pawn, BackstorySlot.Adulthood, backstoryCategories, pawn.Faction?.def);
		}
	}
}
