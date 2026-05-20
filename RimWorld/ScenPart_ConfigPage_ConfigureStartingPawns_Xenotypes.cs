using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes : ScenPart_ConfigPage_ConfigureStartingPawnsBase
{
	public List<XenotypeCount> xenotypeCounts = new List<XenotypeCount>();

	public List<XenotypePawnKind> overrideKinds = new List<XenotypePawnKind>();

	[MustTranslate]
	public string customSummary;

	private float ElementHeight => ScenPart.RowHeight * 4f;

	protected override int TotalPawnCount => xenotypeCounts.Sum((XenotypeCount x) => x.count);

	public override string Summary(Scenario scen)
	{
		return customSummary ?? ((string)"ScenPart_StartWithSpecificColonists".Translate(xenotypeCounts.Select((XenotypeCount x) => x.Summary).ToCommaList(useAnd: true), pawnChoiceCount));
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ElementHeight * (float)xenotypeCounts.Count + ScenPart.RowHeight);
		scenPartRect.height = ScenPart.RowHeight;
		for (int i = 0; i < xenotypeCounts.Count; i++)
		{
			XenotypeCount xenotypeCount = xenotypeCounts[i];
			Widgets.TextFieldNumeric(scenPartRect, ref xenotypeCount.count, ref xenotypeCount.countBuffer, 1f, 10f);
			scenPartRect.y += ScenPart.RowHeight;
			Rect rect = scenPartRect;
			rect.xMax -= ScenPart.RowHeight;
			if (Widgets.ButtonText(rect, xenotypeCount.xenotype.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (XenotypeDef allDef in DefDatabase<XenotypeDef>.AllDefs)
				{
					XenotypeDef localXen = allDef;
					list.Add(new FloatMenuOption(localXen.LabelCap, delegate
					{
						xenotypeCount.xenotype = localXen;
					}, localXen.Icon, Color.white));
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
				xenotypeCounts.RemoveAt(i);
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
			Widgets.Checkbox(scenPartRect.xMin + vector.x + 4f, scenPartRect.y, ref xenotypeCount.requiredAtStart);
			scenPartRect.y += ScenPart.RowHeight * 2f;
		}
		if (Widgets.ButtonText(scenPartRect, "Add".Translate().CapitalizeFirst()))
		{
			xenotypeCounts.Add(new XenotypeCount
			{
				xenotype = XenotypeDefOf.Baseliner,
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
		ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes scenPart_ConfigPage_ConfigureStartingPawns_Xenotypes = new ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes
		{
			def = def,
			visible = visible,
			summarized = summarized,
			pawnChoiceCount = pawnChoiceCount,
			customSummary = customSummary
		};
		foreach (XenotypeCount xenotypeCount in xenotypeCounts)
		{
			scenPart_ConfigPage_ConfigureStartingPawns_Xenotypes.xenotypeCounts.Add(new XenotypeCount
			{
				xenotype = xenotypeCount.xenotype,
				count = xenotypeCount.count,
				allowedDevelopmentalStages = xenotypeCount.allowedDevelopmentalStages,
				description = xenotypeCount.description,
				requiredAtStart = xenotypeCount.requiredAtStart
			});
		}
		foreach (XenotypePawnKind overrideKind in overrideKinds)
		{
			scenPart_ConfigPage_ConfigureStartingPawns_Xenotypes.overrideKinds.Add(new XenotypePawnKind
			{
				xenotype = overrideKind.xenotype,
				pawnKind = overrideKind.pawnKind
			});
		}
		return scenPart_ConfigPage_ConfigureStartingPawns_Xenotypes;
	}

	protected override void GenerateStartingPawns()
	{
		int num = 0;
		do
		{
			StartingPawnUtility.ClearAllStartingPawns();
			int num2 = 0;
			foreach (XenotypeCount xenotypeCount in xenotypeCounts)
			{
				for (int i = 0; i < xenotypeCount.count; i++)
				{
					PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(num2);
					generationRequest.ForcedXenotype = xenotypeCount.xenotype;
					if (xenotypeCount.xenotype != null)
					{
						generationRequest.PawnKindDefGetter = delegate(XenotypeDef t)
						{
							foreach (XenotypePawnKind overrideKind in overrideKinds)
							{
								if (t == overrideKind.xenotype)
								{
									return overrideKind.pawnKind;
								}
							}
							return Find.GameInitData.startingPawnKind ?? Faction.OfPlayer.def.basicMemberKind;
						};
					}
					else
					{
						generationRequest.PawnKindDefGetter = null;
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
		Find.GameInitData.startingPawnsRequired = null;
		Find.GameInitData.startingPawnKind = null;
		Find.GameInitData.startingXenotypesRequired = xenotypeCounts.Where((XenotypeCount c) => c.requiredAtStart).ToList();
		base.PostIdeoChosen();
	}

	public override bool HasNullDefs()
	{
		if (base.HasNullDefs())
		{
			return true;
		}
		foreach (XenotypeCount xenotypeCount in xenotypeCounts)
		{
			if (xenotypeCount.xenotype == null)
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref xenotypeCounts, "xenotypeCounts", LookMode.Deep);
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (XenotypeCount xenotypeCount in xenotypeCounts)
		{
			num ^= xenotypeCount.GetHashCode();
		}
		return num;
	}
}
