using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ImmunityHandler : IExposable
	{
		public struct ImmunityInfo
		{
			public HediffDef immunity;

			public HediffDef source;
		}

		public Pawn pawn;

		private List<ImmunityRecord> immunityList = new List<ImmunityRecord>();

		private static List<ImmunityInfo> tmpNeededImmunitiesNow = new List<ImmunityInfo>();

		public ImmunityHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref immunityList, "imList", LookMode.Deep);
		}

		public float DiseaseContractChanceFactor(HediffDef diseaseDef, BodyPartRecord part = null)
		{
			HediffDef immunityCause = null;
			return DiseaseContractChanceFactor(diseaseDef, out immunityCause, part);
		}

		public float DiseaseContractChanceFactor(HediffDef diseaseDef, out HediffDef immunityCause, BodyPartRecord part = null)
		{
			immunityCause = null;
			if (!pawn.RaceProps.IsFlesh)
			{
				return 0f;
			}
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def == diseaseDef && hediffs[i].Part == part)
				{
					return 0f;
				}
			}
			for (int j = 0; j < immunityList.Count; j++)
			{
				if (immunityList[j].hediffDef == diseaseDef)
				{
					immunityCause = immunityList[j].source;
					return Mathf.Lerp(1f, 0f, immunityList[j].immunity / 0.6f);
				}
			}
			return 1f;
		}

		public float GetImmunity(HediffDef def)
		{
			for (int i = 0; i < immunityList.Count; i++)
			{
				ImmunityRecord immunityRecord = immunityList[i];
				if (immunityRecord.hediffDef == def)
				{
					return immunityRecord.immunity;
				}
			}
			return 0f;
		}

		internal void ImmunityHandlerTick()
		{
			List<ImmunityInfo> list = NeededImmunitiesNow();
			for (int i = 0; i < list.Count; i++)
			{
				TryAddImmunityRecord(list[i].immunity, list[i].source);
			}
			for (int j = 0; j < immunityList.Count; j++)
			{
				ImmunityRecord immunityRecord = immunityList[j];
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(immunityRecord.hediffDef);
				immunityRecord.ImmunityTick(pawn, firstHediffOfDef != null, firstHediffOfDef);
				if (firstHediffOfDef == null && AnyHediffMakesFullyImmuneTo(immunityRecord.hediffDef))
				{
					immunityRecord.immunity = Mathf.Clamp(0.650000036f, immunityRecord.immunity, 1f);
				}
			}
			for (int num = immunityList.Count - 1; num >= 0; num--)
			{
				if (immunityList[num].immunity <= 0f)
				{
					bool flag = false;
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].immunity == immunityList[num].hediffDef)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						immunityList.RemoveAt(num);
					}
				}
			}
		}

		private List<ImmunityInfo> NeededImmunitiesNow()
		{
			tmpNeededImmunitiesNow.Clear();
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff hediff = hediffs[i];
				ImmunityInfo item;
				if (hediff.def.PossibleToDevelopImmunityNaturally())
				{
					List<ImmunityInfo> list = tmpNeededImmunitiesNow;
					item = new ImmunityInfo
					{
						immunity = hediff.def,
						source = hediff.def
					};
					list.Add(item);
				}
				HediffStage curStage = hediff.CurStage;
				if (curStage != null && curStage.makeImmuneTo != null)
				{
					for (int j = 0; j < curStage.makeImmuneTo.Count; j++)
					{
						List<ImmunityInfo> list2 = tmpNeededImmunitiesNow;
						item = new ImmunityInfo
						{
							immunity = curStage.makeImmuneTo[j],
							source = hediff.def
						};
						list2.Add(item);
					}
				}
			}
			return tmpNeededImmunitiesNow;
		}

		private bool AnyHediffMakesFullyImmuneTo(HediffDef def)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage == null || curStage.makeImmuneTo == null)
				{
					continue;
				}
				for (int j = 0; j < curStage.makeImmuneTo.Count; j++)
				{
					if (curStage.makeImmuneTo[j] == def)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void TryAddImmunityRecord(HediffDef def, HediffDef source)
		{
			if (def.CompProps<HediffCompProperties_Immunizable>() != null && !ImmunityRecordExists(def))
			{
				ImmunityRecord immunityRecord = new ImmunityRecord();
				immunityRecord.hediffDef = def;
				immunityRecord.source = source;
				immunityList.Add(immunityRecord);
			}
		}

		public ImmunityRecord GetImmunityRecord(HediffDef def)
		{
			for (int i = 0; i < immunityList.Count; i++)
			{
				if (immunityList[i].hediffDef == def)
				{
					return immunityList[i];
				}
			}
			return null;
		}

		public bool ImmunityRecordExists(HediffDef def)
		{
			return GetImmunityRecord(def) != null;
		}
	}
}
