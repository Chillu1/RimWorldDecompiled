using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class LifeStageWorker_HumanlikeChild : LifeStageWorker
{
	private static readonly List<BackstoryCategoryFilter> ChildBackstoryFilters = new List<BackstoryCategoryFilter>
	{
		new BackstoryCategoryFilter
		{
			categories = new List<string> { "Child" }
		}
	};

	public static readonly List<BackstoryCategoryFilter> ChildTribalBackstoryFilters = new List<BackstoryCategoryFilter>
	{
		new BackstoryCategoryFilter
		{
			categories = new List<string> { "ChildTribal" }
		}
	};

	public override void Notify_LifeStageStarted(Pawn pawn, LifeStageDef previousLifeStage)
	{
		base.Notify_LifeStageStarted(pawn, previousLifeStage);
		if (Current.ProgramState != ProgramState.Playing || previousLifeStage == null || !previousLifeStage.developmentalStage.Baby())
		{
			return;
		}
		if (pawn.story.bodyType != BodyTypeDefOf.Child)
		{
			pawn.apparel?.DropAllOrMoveAllToInventory((Apparel apparel) => !apparel.def.apparel.developmentalStageFilter.Has(DevelopmentalStage.Child));
			BodyTypeDef bodyTypeFor = PawnGenerator.GetBodyTypeFor(pawn);
			pawn.story.bodyType = bodyTypeFor;
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		if (ModsConfig.IdeologyActive && pawn.Faction != null)
		{
			Pawn pawn2 = pawn.GetMother();
			if (pawn2?.Faction != pawn.Faction)
			{
				Pawn father = pawn.GetFather();
				pawn2 = ((father == null || father.Faction != pawn.Faction) ? null : father);
			}
			if (pawn2 != null && pawn2.IsSlave)
			{
				pawn.guest.SetGuestStatus(pawn.Faction, GuestStatus.Slave);
			}
		}
		bool flag = pawn.Ideo == null && pawn.ideo.TryJoinIdeoFromExposures();
		if (!ModsConfig.IdeologyActive)
		{
			flag = false;
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			List<WorkTypeDef> list = new List<WorkTypeDef>();
			List<LifeStageWorkSettings> lifeStageWorkSettings = pawn.RaceProps.lifeStageWorkSettings;
			for (int num = 0; num < lifeStageWorkSettings.Count; num++)
			{
				if (lifeStageWorkSettings[num].minAge <= pawn.ageTracker.AgeBiologicalYears)
				{
					list.Add(lifeStageWorkSettings[num].workType);
				}
			}
			TaggedString text = "LetterBecameChild".Translate(pawn) + "\n\n" + list.Select((WorkTypeDef wt) => wt.labelShort.CapitalizeFirst()).ToLineList(" - ") + "\n\n" + "LetterBecameChildChanges".Translate();
			if (ModsConfig.IdeologyActive)
			{
				text += "\n\n" + ((flag && !Find.IdeoManager.classicMode) ? ("LetterChildFollowIdeo".Translate(pawn, pawn.Ideo) + "\n\n") : TaggedString.Empty) + "LetterChildLegalStatus".Translate(pawn);
				ChoiceLetter_BabyToChild choiceLetter_BabyToChild = (ChoiceLetter_BabyToChild)LetterMaker.MakeLetter("LetterLabelBecameChild".Translate(pawn), text, LetterDefOf.BabyToChild, pawn);
				choiceLetter_BabyToChild.Start();
				Find.LetterStack.ReceiveLetter(choiceLetter_BabyToChild);
			}
			else
			{
				ChoiceLetter choiceLetter = LetterMaker.MakeLetter("LetterLabelBecameChild".Translate(pawn), text, LetterDefOf.PositiveEvent, pawn);
				Find.LetterStack.ReceiveLetter(choiceLetter);
			}
			if (pawn.Spawned)
			{
				EffecterDefOf.Birthday.SpawnAttached(pawn, pawn.Map);
			}
		}
		List<BackstoryCategoryFilter> backstoryCategories = ChildBackstoryFilters;
		if (pawn.IsColonist && Faction.OfPlayer.def == FactionDefOf.PlayerTribe)
		{
			backstoryCategories = ChildTribalBackstoryFilters;
		}
		PawnBioAndNameGenerator.FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, backstoryCategories, pawn.Faction?.def);
		pawn.Notify_DisabledWorkTypesChanged();
	}
}
