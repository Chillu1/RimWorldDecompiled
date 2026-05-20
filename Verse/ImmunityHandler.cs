using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class ImmunityHandler : IExposable
{
	public struct ImmunityInfo
	{
		public HediffDef immunity;

		public HediffDef source;
	}

	public Pawn pawn;

	private List<ImmunityRecord> immunityList = new List<ImmunityRecord>();

	private const float ForcedImmunityLevel = 0.65000004f;

	private static readonly List<ImmunityInfo> tmpNeededImmunitiesNow = new List<ImmunityInfo>();

	public List<ImmunityRecord> ImmunityListForReading => immunityList;

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
		if (!pawn.RaceProps.IsFlesh || pawn.RaceProps.isImmuneToInfections)
		{
			return 0f;
		}
		if (pawn.IsMutant && pawn.mutant.Def.preventIllnesses && pawn.mutant.Def.isImmuneToInfections)
		{
			return 0f;
		}
		if (AnyHediffMakesFullyImmuneTo(diseaseDef, out var sourceHediff))
		{
			immunityCause = sourceHediff.def;
			return 0f;
		}
		if (AnyGeneMakesFullyImmuneTo(diseaseDef))
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

	public float GetImmunity(HediffDef def, bool naturalImmunityOnly = false)
	{
		float num = 0f;
		for (int i = 0; i < immunityList.Count; i++)
		{
			ImmunityRecord immunityRecord = immunityList[i];
			if (immunityRecord.hediffDef == def)
			{
				num = immunityRecord.immunity;
				break;
			}
		}
		if (!naturalImmunityOnly && (AnyHediffMakesFullyImmuneTo(def, out var _) || AnyGeneMakesFullyImmuneTo(def)) && num < 0.65000004f)
		{
			num = 0.65000004f;
		}
		return num;
	}

	internal void ImmunityHandlerTickInterval(int delta)
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
			immunityRecord.ImmunityTickInterval(pawn, firstHediffOfDef != null, firstHediffOfDef, delta);
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
			if (hediff.def.PossibleToDevelopImmunityNaturally())
			{
				tmpNeededImmunitiesNow.Add(new ImmunityInfo
				{
					immunity = hediff.def,
					source = hediff.def
				});
			}
		}
		return tmpNeededImmunitiesNow;
	}

	private bool AnyHediffMakesFullyImmuneTo(HediffDef def, out Hediff sourceHediff)
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
					sourceHediff = hediffs[i];
					return true;
				}
			}
		}
		sourceHediff = null;
		return false;
	}

	public bool AnyGeneMakesFullyImmuneTo(HediffDef def)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return false;
		}
		for (int i = 0; i < pawn.genes.GenesListForReading.Count; i++)
		{
			Gene gene = pawn.genes.GenesListForReading[i];
			if (gene.def.makeImmuneTo == null)
			{
				continue;
			}
			for (int j = 0; j < gene.def.makeImmuneTo.Count; j++)
			{
				if (gene.def.makeImmuneTo[j] == def)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TryAddImmunityRecord(HediffDef def, HediffDef source)
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
		ImmunityRecord immunityRecord = null;
		for (int i = 0; i < immunityList.Count; i++)
		{
			if (immunityList[i].hediffDef == def)
			{
				immunityRecord = immunityList[i];
				break;
			}
		}
		if (AnyHediffMakesFullyImmuneTo(def, out var sourceHediff) && (immunityRecord == null || immunityRecord.immunity < 0.65000004f))
		{
			immunityRecord = new ImmunityRecord
			{
				immunity = 0.65000004f,
				hediffDef = def,
				source = sourceHediff.def
			};
		}
		return immunityRecord;
	}

	public bool ImmunityRecordExists(HediffDef def)
	{
		return GetImmunityRecord(def) != null;
	}
}
