using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ForcedTrait : ScenPart_PawnModifier
{
	private TraitDef trait;

	private int degree;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref trait, "trait");
		Scribe_Values.Look(ref degree, "degree", 0);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
		if (Widgets.ButtonText(scenPartRect.TopPart(0.333f), trait.DataAtDegree(degree).LabelCap))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TraitDef item in DefDatabase<TraitDef>.AllDefs.OrderBy((TraitDef td) => td.label))
			{
				foreach (TraitDegreeData degreeData in item.degreeDatas)
				{
					TraitDef localDef = item;
					TraitDegreeData localDeg = degreeData;
					list.Add(new FloatMenuOption(localDeg.LabelCap, delegate
					{
						trait = localDef;
						degree = localDeg.degree;
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		DoPawnModifierEditInterface(scenPartRect.BottomPart(0.666f));
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_PawnsHaveTrait".Translate(context.ToStringHuman(), chance.ToStringPercent(), trait.DataAtDegree(degree).LabelCap).CapitalizeFirst();
	}

	public override void Randomize()
	{
		base.Randomize();
		trait = DefDatabase<TraitDef>.GetRandom();
		degree = trait.degreeDatas.RandomElement().degree;
	}

	public override bool CanCoexistWith(ScenPart other)
	{
		if (other is ScenPart_ForcedTrait scenPart_ForcedTrait && trait == scenPart_ForcedTrait.trait && context.OverlapsWith(scenPart_ForcedTrait.context))
		{
			return false;
		}
		return true;
	}

	protected override void ModifyPawnPostGenerate(Pawn pawn, bool redressed)
	{
		if (pawn.story == null || pawn.story.traits == null || this.trait == null || (pawn.story.traits.HasTrait(this.trait) && pawn.story.traits.DegreeOfTrait(this.trait) == degree))
		{
			return;
		}
		if (pawn.story.traits.HasTrait(this.trait))
		{
			pawn.story.traits.allTraits.RemoveAll((Trait tr) => tr.def == this.trait);
		}
		else
		{
			IEnumerable<Trait> source = pawn.story.traits.allTraits.Where((Trait tr) => !tr.ScenForced && !PawnHasTraitForcedByBackstory(pawn, tr.def));
			if (source.Any())
			{
				Trait trait = source.Where((Trait tr) => this.trait.ConflictsWith(tr.def)).FirstOrDefault();
				if (trait != null)
				{
					pawn.story.traits.allTraits.Remove(trait);
				}
				else
				{
					pawn.story.traits.allTraits.Remove(source.RandomElement());
				}
			}
		}
		Trait trait2 = new Trait(this.trait, degree, forced: true);
		pawn.story.traits.GainTrait(trait2);
		TraitUtility.ApplySkillGainFromTrait(pawn, trait2);
	}

	private static bool PawnHasTraitForcedByBackstory(Pawn pawn, TraitDef trait)
	{
		if (pawn.story.Childhood != null && pawn.story.Childhood.forcedTraits != null && pawn.story.Childhood.forcedTraits.Any((BackstoryTrait te) => te.def == trait))
		{
			return true;
		}
		if (pawn.story.Adulthood != null && pawn.story.Adulthood.forcedTraits != null && pawn.story.Adulthood.forcedTraits.Any((BackstoryTrait te) => te.def == trait))
		{
			return true;
		}
		return false;
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs())
		{
			return trait == null;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((trait != null) ? trait.GetHashCode() : 0) ^ degree;
	}
}
