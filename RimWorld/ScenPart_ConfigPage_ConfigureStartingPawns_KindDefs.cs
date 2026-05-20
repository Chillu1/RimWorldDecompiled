using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ConfigPage_ConfigureStartingPawns_KindDefs : ScenPart_ConfigPage_ConfigureStartingPawnsBase
{
	public List<PawnKindCount> kindCounts = new List<PawnKindCount>();

	private PawnKindDef leftBehindPawnKind;

	private float ElementHeight => ScenPart.RowHeight * 4f;

	private IEnumerable<PawnKindDef> AvailableKinds => DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Humanlike && x.defaultFactionDef != null && x.defaultFactionDef.isPlayer);

	protected override int TotalPawnCount => kindCounts.Sum((PawnKindCount x) => x.count);

	public override string Summary(Scenario scen)
	{
		return "ScenPart_StartWithSpecificColonists".Translate(kindCounts.Select((PawnKindCount x) => x.Summary).ToCommaList(useAnd: true), pawnChoiceCount);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ElementHeight * (float)kindCounts.Count + ScenPart.RowHeight);
		scenPartRect.height = ScenPart.RowHeight;
		for (int i = 0; i < kindCounts.Count; i++)
		{
			PawnKindCount kindCount = kindCounts[i];
			Widgets.TextFieldNumeric(scenPartRect, ref kindCount.count, ref kindCount.countBuffer, 1f, 10f);
			scenPartRect.y += ScenPart.RowHeight;
			Rect rect = scenPartRect;
			rect.xMax -= ScenPart.RowHeight;
			if (Widgets.ButtonText(rect, kindCount.kindDef.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (PawnKindDef availableKind in AvailableKinds)
				{
					PawnKindDef localKind = availableKind;
					list.Add(new FloatMenuOption(localKind.LabelCap, delegate
					{
						kindCount.kindDef = localKind;
					}));
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
				kindCounts.RemoveAt(i);
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
			Widgets.Checkbox(scenPartRect.xMin + vector.x + 4f, scenPartRect.y, ref kindCount.requiredAtStart);
			scenPartRect.y += ScenPart.RowHeight * 2f;
		}
		if (Widgets.ButtonText(scenPartRect, "Add".Translate().CapitalizeFirst()))
		{
			kindCounts.Add(new PawnKindCount
			{
				kindDef = PawnKindDefOf.Colonist,
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
		ScenPart_ConfigPage_ConfigureStartingPawns_KindDefs scenPart_ConfigPage_ConfigureStartingPawns_KindDefs = new ScenPart_ConfigPage_ConfigureStartingPawns_KindDefs
		{
			def = def,
			visible = visible,
			summarized = summarized,
			pawnChoiceCount = pawnChoiceCount,
			leftBehindPawnKind = leftBehindPawnKind
		};
		foreach (PawnKindCount kindCount in kindCounts)
		{
			scenPart_ConfigPage_ConfigureStartingPawns_KindDefs.kindCounts.Add(new PawnKindCount
			{
				kindDef = kindCount.kindDef,
				count = kindCount.count,
				requiredAtStart = kindCount.requiredAtStart
			});
		}
		return scenPart_ConfigPage_ConfigureStartingPawns_KindDefs;
	}

	protected override void GenerateStartingPawns()
	{
		int num = 0;
		do
		{
			StartingPawnUtility.ClearAllStartingPawns();
			int num2 = 0;
			foreach (PawnKindCount kindCount in kindCounts)
			{
				for (int i = 0; i < kindCount.count; i++)
				{
					PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(num2);
					generationRequest.KindDef = kindCount.kindDef;
					if (ModsConfig.BiotechActive)
					{
						if (kindCount.kindDef.xenotypeSet != null)
						{
							generationRequest.ForcedXenotype = null;
						}
						else
						{
							generationRequest.ForcedXenotype = XenotypeDefOf.Baseliner;
						}
					}
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
		Find.GameInitData.startingPawnKind = leftBehindPawnKind;
		Find.GameInitData.startingPawnsRequired = kindCounts.Where((PawnKindCount c) => c.requiredAtStart).ToList();
		Find.GameInitData.startingXenotypesRequired = null;
		base.PostIdeoChosen();
	}

	public override bool HasNullDefs()
	{
		if (base.HasNullDefs())
		{
			return true;
		}
		foreach (PawnKindCount kindCount in kindCounts)
		{
			if (kindCount.kindDef == null)
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref kindCounts, "kindCounts", LookMode.Deep);
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (PawnKindCount kindCount in kindCounts)
		{
			num ^= kindCount.GetHashCode();
		}
		return num;
	}
}
