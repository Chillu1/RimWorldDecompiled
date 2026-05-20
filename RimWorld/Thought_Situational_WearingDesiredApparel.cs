using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Thought_Situational_WearingDesiredApparel : Thought_Situational
{
	public bool multipleApparelItems;

	public override string LabelCap => base.CurStage.label.Formatted(multipleApparelItems ? "WearingDesiredApparelMulti".Translate().Named("APPAREL") : MetApparelPrecepts().FirstOrFallback(SourceApparelPrecept).apparelDef.label.Named("APPAREL")).CapitalizeFirst();

	public override string Description => base.CurStage.description.Formatted(multipleApparelItems ? "WearingDesiredApparelMulti".Translate().Named("APPAREL") : Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(MetApparelPrecepts().FirstOrFallback(SourceApparelPrecept).apparelDef.label).Named("APPAREL"), pawn.Named("PAWN")).CapitalizeFirst() + base.CausedByBeliefInPrecept;

	private Precept_Apparel SourceApparelPrecept => (Precept_Apparel)sourcePrecept;

	protected override ThoughtState CurrentStateInternal()
	{
		if (!ModsConfig.IdeologyActive || pawn.Ideo == null)
		{
			return ThoughtState.Inactive;
		}
		List<Precept> allPreceptsAllowingSituationalThought = pawn.Ideo.GetAllPreceptsAllowingSituationalThought(def);
		if (allPreceptsAllowingSituationalThought.Count == 0)
		{
			return ThoughtState.Inactive;
		}
		int num = 0;
		foreach (Precept item in allPreceptsAllowingSituationalThought)
		{
			if (item is Precept_Apparel precept_Apparel && (precept_Apparel.TargetGender == Gender.None || precept_Apparel.TargetGender == pawn.gender))
			{
				if (!HasApparel(precept_Apparel.apparelDef))
				{
					return ThoughtState.Inactive;
				}
				num++;
			}
		}
		multipleApparelItems = num > 1;
		if (num != 0)
		{
			return base.CurrentStateInternal();
		}
		return ThoughtState.Inactive;
	}

	private IEnumerable<Precept_Apparel> MetApparelPrecepts()
	{
		if (!ModsConfig.IdeologyActive || pawn.Ideo == null)
		{
			yield break;
		}
		List<Precept> allPreceptsAllowingSituationalThought = pawn.Ideo.GetAllPreceptsAllowingSituationalThought(def);
		foreach (Precept item in allPreceptsAllowingSituationalThought)
		{
			if (item is Precept_Apparel precept_Apparel && (precept_Apparel.TargetGender == Gender.None || precept_Apparel.TargetGender == pawn.gender) && HasApparel(precept_Apparel.apparelDef))
			{
				yield return precept_Apparel;
			}
		}
	}

	private bool HasApparel(ThingDef thingDef)
	{
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (item.def == thingDef)
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref multipleApparelItems, "multipleApparelItems", defaultValue: false);
	}
}
