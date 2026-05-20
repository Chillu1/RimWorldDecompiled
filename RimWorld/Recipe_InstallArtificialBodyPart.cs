using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Recipe_InstallArtificialBodyPart : Recipe_Surgery
{
	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate(BodyPartRecord record)
		{
			IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == record);
			if (typeof(Hediff_AddedPart).IsAssignableFrom(recipe.addsHediff.hediffClass))
			{
				if (source.Count() == 1 && source.First().def == recipe.addsHediff)
				{
					return false;
				}
			}
			else if (source.Any((Hediff hd) => hd.def == recipe.addsHediff))
			{
				return false;
			}
			if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
			{
				return false;
			}
			return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
		});
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		bool flag = MedicalRecipesUtility.IsClean(pawn, part);
		bool flag2 = !PawnGenerator.IsBeingGenerated(pawn) && IsViolationOnPawn(pawn, part, Faction.OfPlayer);
		Hediff hediff = null;
		if (billDoer != null)
		{
			if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				return;
			}
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			hediff = pawn.health.hediffSet.GetDirectlyAddedPartFor(part);
			if (part != null)
			{
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
			}
			if (flag && flag2 && part.def.spawnThingOnRemoved != null)
			{
				ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
			}
			if (flag2)
			{
				ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
			}
			if (ModsConfig.IdeologyActive)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledProsthetic, billDoer.Named(HistoryEventArgsNames.Doer)));
			}
		}
		else if (pawn.Map != null)
		{
			if (part != null)
			{
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
			}
		}
		else if (part != null)
		{
			pawn.health.RestorePart(part);
		}
		pawn.health.AddHediff(recipe.addsHediff, part);
		hediff?.Notify_SurgicallyReplaced(billDoer);
	}

	public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
	{
		if ((pawn.Faction == billDoerFaction || pawn.Faction == null) && !pawn.IsQuestLodger())
		{
			return false;
		}
		if (recipe.addsHediff.addedPartProps != null && recipe.addsHediff.addedPartProps.betterThanNatural)
		{
			return false;
		}
		return HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
	}
}
