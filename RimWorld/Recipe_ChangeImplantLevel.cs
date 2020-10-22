using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_ChangeImplantLevel : Recipe_Surgery
	{
		private bool Operable(Hediff target, RecipeDef recipe)
		{
			int hediffLevelOffset = recipe.hediffLevelOffset;
			if (hediffLevelOffset == 0)
			{
				return false;
			}
			Hediff_ImplantWithLevel hediff_ImplantWithLevel = target as Hediff_ImplantWithLevel;
			if (hediff_ImplantWithLevel == null)
			{
				return false;
			}
			int level = hediff_ImplantWithLevel.level;
			if (hediff_ImplantWithLevel.def == recipe.changesHediffLevel)
			{
				if (hediffLevelOffset <= 0)
				{
					return level > 0;
				}
				return (float)level < hediff_ImplantWithLevel.def.maxSeverity;
			}
			return false;
		}

		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (BodyPartRecord record) => pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && Operable(x, recipe)) ? true : false);
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			if (billDoer != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			}
			Hediff_ImplantWithLevel hediff_ImplantWithLevel = (Hediff_ImplantWithLevel)pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => Operable(h, recipe) && h.Part == part);
			if (hediff_ImplantWithLevel != null)
			{
				if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
				{
					ReportViolation(pawn, billDoer, pawn.FactionOrExtraMiniOrHomeFaction, -70, "GoodwillChangedReason_DowngradedImplant".Translate(hediff_ImplantWithLevel.Label));
				}
				hediff_ImplantWithLevel.ChangeLevel(recipe.hediffLevelOffset);
			}
		}
	}
}
