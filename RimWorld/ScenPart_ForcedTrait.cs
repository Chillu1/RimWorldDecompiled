using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
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
			ScenPart_ForcedTrait scenPart_ForcedTrait = other as ScenPart_ForcedTrait;
			if (scenPart_ForcedTrait != null && trait == scenPart_ForcedTrait.trait && context.OverlapsWith(scenPart_ForcedTrait.context))
			{
				return false;
			}
			return true;
		}

		protected override void ModifyPawnPostGenerate(Pawn pawn, bool redressed)
		{
			if (pawn.story == null || pawn.story.traits == null || (pawn.story.traits.HasTrait(this.trait) && pawn.story.traits.DegreeOfTrait(this.trait) == degree))
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
			pawn.story.traits.GainTrait(new Trait(this.trait, degree, forced: true));
		}

		private static bool PawnHasTraitForcedByBackstory(Pawn pawn, TraitDef trait)
		{
			if (pawn.story.childhood != null && pawn.story.childhood.forcedTraits != null && pawn.story.childhood.forcedTraits.Any((TraitEntry te) => te.def == trait))
			{
				return true;
			}
			if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null && pawn.story.adulthood.forcedTraits.Any((TraitEntry te) => te.def == trait))
			{
				return true;
			}
			return false;
		}
	}
}
