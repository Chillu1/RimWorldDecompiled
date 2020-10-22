using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
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
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + list[i].LabelNoCountColored.Resolve());
			}
			string text = ((!list.Any()) ? "" : string.Format(def.letterText, list.Count.ToString(), Faction.OfPlayer.def.pawnsPlural, def.diseaseIncident.label, stringBuilder.ToString()));
			if (!blockedInfo.NullOrEmpty())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += blockedInfo;
			}
			SendStandardLetter(def.letterLabel, text, def.letterDef, parms, list);
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
					blockedInfo += "LetterDisease_Blocked".Translate(item.Key.LabelCap, def.diseaseIncident.label, item.Value.Select((Pawn victim) => victim.LabelShort).ToLineList("  - "));
				}
			}
			return list;
		}
	}
}
