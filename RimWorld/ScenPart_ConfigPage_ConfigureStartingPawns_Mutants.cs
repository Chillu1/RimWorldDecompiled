using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ConfigPage_ConfigureStartingPawns_Mutants : ScenPart_ConfigPage_ConfigureStartingPawnsBase
{
	public List<MutantCount> mutantCounts = new List<MutantCount>();

	[MustTranslate]
	public string customSummary;

	private float ElementHeight => ScenPart.RowHeight * 4f;

	protected override int TotalPawnCount => mutantCounts.Sum((MutantCount x) => x.count);

	public override string Summary(Scenario scen)
	{
		return customSummary ?? ((string)"ScenPart_StartWithSpecificColonists".Translate(mutantCounts.Select((MutantCount x) => x.Summary).ToCommaList(useAnd: true), pawnChoiceCount));
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ElementHeight * (float)mutantCounts.Count + ScenPart.RowHeight);
		scenPartRect.height = ScenPart.RowHeight;
		for (int i = 0; i < mutantCounts.Count; i++)
		{
			MutantCount mutantCount = mutantCounts[i];
			Widgets.TextFieldNumeric(scenPartRect, ref mutantCount.count, ref mutantCount.countBuffer, 1f, 10f);
			scenPartRect.y += ScenPart.RowHeight;
			Rect rect = scenPartRect;
			rect.xMax -= ScenPart.RowHeight;
			if (Widgets.ButtonText(rect, mutantCount.mutant?.LabelCap ?? "None".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("None".Translate(), delegate
				{
					mutantCount.mutant = null;
				}, null, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: false));
				foreach (MutantDef allDef in DefDatabase<MutantDef>.AllDefs)
				{
					MutantDef localMut = allDef;
					if (localMut.showInScenarioEditor)
					{
						list.Add(new FloatMenuOption(localMut.LabelCap, delegate
						{
							mutantCount.mutant = localMut;
						}, null, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: false));
					}
				}
				if (list.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list));
				}
			}
			Rect butRect = scenPartRect;
			butRect.xMin = rect.xMax;
			if (Widgets.ButtonImage(butRect, TexButton.Delete))
			{
				mutantCounts.RemoveAt(i);
				return;
			}
			scenPartRect.y += ScenPart.RowHeight;
			Rect rect2 = scenPartRect;
			TaggedString taggedString = "Required".Translate();
			Vector2 vector = Text.CalcSize(taggedString);
			rect2.width = vector.x;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect2, taggedString);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Checkbox(scenPartRect.xMin + vector.x + 4f, scenPartRect.y, ref mutantCount.requiredAtStart);
			scenPartRect.y += ScenPart.RowHeight * 2f;
		}
		if (Widgets.ButtonText(scenPartRect, "Add".Translate().CapitalizeFirst()))
		{
			mutantCounts.Add(new MutantCount
			{
				mutant = null,
				count = 1
			});
		}
		else
		{
			scenPartRect.y += ScenPart.RowHeight;
		}
	}

	protected override ScenPart CopyForEditingInner()
	{
		ScenPart_ConfigPage_ConfigureStartingPawns_Mutants scenPart_ConfigPage_ConfigureStartingPawns_Mutants = new ScenPart_ConfigPage_ConfigureStartingPawns_Mutants
		{
			def = def,
			visible = visible,
			summarized = summarized,
			pawnChoiceCount = pawnChoiceCount,
			customSummary = customSummary
		};
		foreach (MutantCount mutantCount in mutantCounts)
		{
			scenPart_ConfigPage_ConfigureStartingPawns_Mutants.mutantCounts.Add(new MutantCount
			{
				mutant = mutantCount.mutant,
				count = mutantCount.count,
				allowedDevelopmentalStages = mutantCount.allowedDevelopmentalStages,
				description = mutantCount.description,
				requiredAtStart = mutantCount.requiredAtStart
			});
		}
		return scenPart_ConfigPage_ConfigureStartingPawns_Mutants;
	}

	protected override void GenerateStartingPawns()
	{
		int num = 0;
		do
		{
			StartingPawnUtility.ClearAllStartingPawns();
			int num2 = 0;
			foreach (MutantCount mutantCount in mutantCounts)
			{
				for (int i = 0; i < mutantCount.count; i++)
				{
					PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(num2);
					generationRequest.ForcedMutant = mutantCount.mutant;
					StartingPawnUtility.SetGenerationRequest(num2, generationRequest);
					StartingPawnUtility.AddNewPawn(num2);
					num2++;
				}
			}
			num++;
		}
		while (num <= 20 && !StartingPawnUtility.WorkTypeRequirementsSatisfied());
	}

	public override void PostIdeoChosen()
	{
		Find.GameInitData.startingPawnsRequired = null;
		Find.GameInitData.startingPawnKind = null;
		Find.GameInitData.startingMutantsRequired = mutantCounts.Where((MutantCount c) => c.requiredAtStart).ToList();
		base.PostIdeoChosen();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref mutantCounts, "mutantCounts", LookMode.Deep);
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (MutantCount mutantCount in mutantCounts)
		{
			num ^= mutantCount.GetHashCode();
		}
		return num;
	}
}
