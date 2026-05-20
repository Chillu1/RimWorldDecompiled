using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class IncidentWorker_Disease : IncidentWorker
{
	protected abstract IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target);

	protected IEnumerable<Pawn> PotentialVictims(IIncidentTarget target)
	{
		return PotentialVictimCandidates(target).Where(delegate(Pawn p)
		{
			if (p.ParentHolder is Building_CryptosleepCasket)
			{
				return false;
			}
			if (p.RaceProps.Dryad)
			{
				return false;
			}
			if (!def.diseaseDevelopmentStage.Has(p.DevelopmentalStage))
			{
				return false;
			}
			if (!def.diseasePartsToAffect.NullOrEmpty())
			{
				bool flag = false;
				for (int i = 0; i < def.diseasePartsToAffect.Count; i++)
				{
					if (CanAddHediffToAnyPartOfDef(p, def.diseaseIncident, def.diseasePartsToAffect[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return p.RaceProps.IsFlesh;
		});
	}

	protected abstract IEnumerable<Pawn> ActualVictims(IncidentParms parms);

	private static bool CanAddHediffToAnyPartOfDef(Pawn pawn, HediffDef hediffDef, BodyPartDef partDef)
	{
		List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
		if (pawn.ageTracker.CurLifeStage == LifeStageDefOf.HumanlikeBaby && Find.Storyteller.difficulty.babiesAreHealthy)
		{
			return false;
		}
		if (!hediffDef.canAffectBionicOrImplant && pawn.health.hediffSet.IsBionicOrImplant(partDef))
		{
			return false;
		}
		for (int i = 0; i < allParts.Count; i++)
		{
			BodyPartRecord bodyPartRecord = allParts[i];
			if (bodyPartRecord.def == partDef && !pawn.health.hediffSet.PartIsMissing(bodyPartRecord) && !pawn.health.hediffSet.HasHediff(hediffDef, bodyPartRecord))
			{
				return true;
			}
		}
		return false;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!PotentialVictims(parms.target).Any())
		{
			return false;
		}
		return true;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		string blockedInfo;
		List<Pawn> list = ApplyToPawns(ActualVictims(parms).ToList(), out blockedInfo);
		if (!list.Any() && blockedInfo.NullOrEmpty())
		{
			return false;
		}
		TaggedString baseLetterLabel = def.letterLabel;
		TaggedString baseLetterText;
		if (list.Any())
		{
			if (def.letterSingularForm)
			{
				if (list.Count > 1)
				{
					Log.Error("Incident " + def.defName + " is marked to only generate a letter in a singular format, but multiple victims were provided.");
				}
				Pawn pawn = list[0];
				Hediff mostRecentHediff = pawn.health.hediffSet.GetMostRecentHediff(def.diseaseIncident);
				baseLetterLabel = def.letterLabel.Formatted(pawn.Named("PAWN"));
				HediffComp_SeverityPerDay hediffComp_SeverityPerDay = mostRecentHediff.TryGetComp<HediffComp_SeverityPerDay>();
				if (hediffComp_SeverityPerDay != null)
				{
					float num = hediffComp_SeverityPerDay.SeverityChangePerDay();
					int num2 = Mathf.RoundToInt(mostRecentHediff.def.maxSeverity / num);
					baseLetterText = def.letterText.Formatted(pawn.Named("PAWN"), def.diseaseIncident.label, mostRecentHediff.Part.Label, num2).Resolve();
				}
				else
				{
					baseLetterText = def.letterText.Formatted(pawn.Named("PAWN"), def.diseaseIncident.label, mostRecentHediff.Part.Label).Resolve();
				}
				if (mostRecentHediff.IsAnyStageLifeThreatening() && !string.IsNullOrEmpty(def.diseaseLethalLetterText))
				{
					baseLetterText += "\n\n" + def.diseaseLethalLetterText.Formatted(pawn.Named("PAWN"));
				}
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < list.Count; i++)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendTagged("  - " + list[i].LabelNoCountColored.Resolve());
				}
				baseLetterText = def.letterText.Formatted(list.Count.ToString(), Faction.OfPlayer.def.pawnsPlural, def.diseaseIncident.label).Resolve() + ":\n\n" + stringBuilder;
			}
		}
		else
		{
			baseLetterText = "";
		}
		if (!blockedInfo.NullOrEmpty())
		{
			if (!baseLetterText.NullOrEmpty())
			{
				baseLetterText += "\n\n";
			}
			baseLetterText += blockedInfo;
		}
		SendStandardLetter(baseLetterLabel, baseLetterText, def.letterDef, parms, list);
		return true;
	}

	public List<Pawn> ApplyToPawns(IEnumerable<Pawn> pawns, out string blockedInfo)
	{
		List<Pawn> list = new List<Pawn>();
		Dictionary<HediffDef, List<Pawn>> dictionary = new Dictionary<HediffDef, List<Pawn>>();
		foreach (Pawn pawn in pawns)
		{
			HediffDef immunityCause = null;
			if (Rand.Chance(pawn.health.immunity.DiseaseContractChanceFactor(def.diseaseIncident, out immunityCause)))
			{
				HediffGiverUtility.TryApply(pawn, def.diseaseIncident, def.diseasePartsToAffect);
				TaleRecorder.RecordTale(TaleDefOf.IllnessRevealed, pawn, def.diseaseIncident);
				list.Add(pawn);
			}
			else if (immunityCause != null)
			{
				if (!dictionary.ContainsKey(immunityCause))
				{
					dictionary[immunityCause] = new List<Pawn>();
				}
				dictionary[immunityCause].Add(pawn);
			}
		}
		blockedInfo = "";
		foreach (KeyValuePair<HediffDef, List<Pawn>> item in dictionary)
		{
			if (item.Key != def.diseaseIncident)
			{
				if (blockedInfo.Length != 0)
				{
					blockedInfo += "\n\n";
				}
				blockedInfo = blockedInfo + "LetterDisease_Blocked".Translate(item.Key.LabelCap, def.diseaseIncident.label).Resolve() + ":\n" + item.Value.Select((Pawn victim) => victim.LabelNoCountColored.Resolve()).ToLineList("  - ");
			}
		}
		return list;
	}
}
