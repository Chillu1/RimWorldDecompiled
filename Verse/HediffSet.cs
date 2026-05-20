using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffSet : IExposable
{
	public Pawn pawn;

	public List<Hediff> hediffs = new List<Hediff>();

	private Dictionary<DamageDef, float> cachedDamageFactors = new Dictionary<DamageDef, float>();

	private List<Hediff_MissingPart> cachedMissingPartsCommonAncestors;

	private Dictionary<NeedDef, Hediff> cachedEnabledNeeds;

	private Dictionary<NeedDef, Hediff> cachedDisabledNeeds;

	private float cachedPain = -1f;

	private float cachedBleedRate = -1f;

	private bool? cachedHasHead;

	private bool? cachedCanCrawl;

	private bool? cachedHasRegeneration;

	private bool? cachedRemoveRoamMtb;

	private bool? cachedPreventVacuumBurns;

	private List<Verb> tmpHediffVerbs = new List<Verb>();

	private static List<BodyPartRecord> tmpInjuredParts = new List<BodyPartRecord>();

	private static List<BodyPartRecord> tmpNaturallyHealingInjuredParts = new List<BodyPartRecord>();

	private Stack<BodyPartRecord> coveragePartsStack = new Stack<BodyPartRecord>();

	private HashSet<BodyPartRecord> coverageRejectedPartsSet = new HashSet<BodyPartRecord>();

	private readonly Queue<BodyPartRecord> missingPartsCommonAncestorsQueue = new Queue<BodyPartRecord>();

	private float cachedMaxPctConditionalThoughtsNullified = -1f;

	private Hediff cachedConditionalThoughtNullifyingHediff;

	private float cachedMaxPctAllThoughtsNullified = -1f;

	private Hediff cachedAllThoughtNullifyingHediff;

	public float PainTotal
	{
		get
		{
			if (cachedPain < 0f)
			{
				cachedPain = CalculatePain();
			}
			return cachedPain;
		}
	}

	public float BleedRateTotal
	{
		get
		{
			if (cachedBleedRate < 0f)
			{
				cachedBleedRate = CalculateBleedRate();
			}
			return cachedBleedRate;
		}
	}

	public bool HasHead
	{
		get
		{
			bool valueOrDefault = cachedHasHead == true;
			if (!cachedHasHead.HasValue)
			{
				valueOrDefault = GetNotMissingParts().Any((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
				cachedHasHead = valueOrDefault;
			}
			return cachedHasHead.Value;
		}
	}

	public bool PreventVacuumBurns
	{
		get
		{
			bool valueOrDefault = cachedPreventVacuumBurns == true;
			if (!cachedPreventVacuumBurns.HasValue)
			{
				valueOrDefault = hediffs.Any((Hediff hd) => hd.CurStage?.preventVacuumBurns ?? false);
				cachedPreventVacuumBurns = valueOrDefault;
			}
			return cachedPreventVacuumBurns.Value;
		}
	}

	public bool HasRegeneration
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			bool valueOrDefault = cachedHasRegeneration == true;
			if (!cachedHasRegeneration.HasValue)
			{
				valueOrDefault = hediffs.Any(delegate(Hediff hd)
				{
					HediffStage curStage = hd.CurStage;
					return curStage != null && curStage.regeneration > 0f;
				});
				cachedHasRegeneration = valueOrDefault;
			}
			return cachedHasRegeneration.Value;
		}
	}

	public bool RemoveRoamMtb
	{
		get
		{
			bool valueOrDefault = cachedRemoveRoamMtb == true;
			if (!cachedRemoveRoamMtb.HasValue)
			{
				valueOrDefault = hediffs.Any((Hediff hd) => hd.CurStage?.removeRoamMtb ?? false);
				cachedRemoveRoamMtb = valueOrDefault;
			}
			return cachedRemoveRoamMtb.Value;
		}
	}

	public bool HasPreventsDeath
	{
		get
		{
			foreach (Hediff hediff in hediffs)
			{
				if (hediff.def.preventsDeath)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnyHediffPreventsCrawling
	{
		get
		{
			bool valueOrDefault = cachedCanCrawl == true;
			if (!cachedCanCrawl.HasValue)
			{
				valueOrDefault = hediffs.Any((Hediff hd) => hd.def.preventsCrawling);
				cachedCanCrawl = valueOrDefault;
			}
			return cachedCanCrawl.Value;
		}
	}

	public float HungerRateFactor => GetHungerRateFactor();

	public float RestFallFactor
	{
		get
		{
			float num = 1f;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null)
				{
					num *= curStage.restFallFactor;
				}
			}
			for (int j = 0; j < hediffs.Count; j++)
			{
				HediffStage curStage2 = hediffs[j].CurStage;
				if (curStage2 != null)
				{
					num += curStage2.restFallFactorOffset;
				}
			}
			return Mathf.Max(num, 0f);
		}
	}

	public float? OverrideMoodBase
	{
		get
		{
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].CurStage != null && hediffs[i].CurStage.overrideMoodBase >= 0f)
				{
					return hediffs[i].CurStage.overrideMoodBase;
				}
			}
			return null;
		}
	}

	public bool AnyHediffMakesSickThought
	{
		get
		{
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.makesSickThought && hediffs[i].Visible)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsPsychicallySuppressed
	{
		get
		{
			if (ModsConfig.RoyaltyActive)
			{
				return HasHediff(HediffDefOf.PsychicSuppression);
			}
			return false;
		}
	}

	public HediffSet(Pawn newPawn)
	{
		pawn = newPawn;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref hediffs, "hediffs", LookMode.Deep);
		if ((Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs) && hediffs.RemoveAll((Hediff x) => x == null) != 0)
		{
			Log.Error(pawn.ToStringSafe() + " had some null hediffs.");
		}
		if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
		{
			return;
		}
		for (int num = 0; num < hediffs.Count; num++)
		{
			hediffs[num].pawn = pawn;
		}
		hediffs.RemoveAll(delegate(Hediff hediff)
		{
			if (hediff?.def != null)
			{
				return false;
			}
			Log.Error(hediff.ToStringSafe() + " on " + pawn.ToStringSafe() + " had a null def.");
			return true;
		});
		DirtyCache();
	}

	public float FactorForDamage(DamageInfo dinfo)
	{
		if (dinfo.Def == null || hediffs.NullOrEmpty())
		{
			return 1f;
		}
		if (cachedDamageFactors.TryGetValue(dinfo.Def, out var value))
		{
			return value;
		}
		float num = 1f;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].CurStage == null || hediffs[i].CurStage.damageFactors.NullOrEmpty())
			{
				continue;
			}
			for (int j = 0; j < hediffs[i].CurStage.damageFactors.Count; j++)
			{
				DamageFactor damageFactor = hediffs[i].CurStage.damageFactors[j];
				if (damageFactor.damageDef == dinfo.Def)
				{
					num *= damageFactor.factor;
				}
			}
		}
		cachedDamageFactors.Add(dinfo.Def, num);
		return num;
	}

	public bool HasBodyPart(BodyPartRecord part)
	{
		return GetNotMissingParts().Contains(part);
	}

	public void AddDirect(Hediff hediff, DamageInfo? dinfo = null, DamageWorker.DamageResult damageResult = null)
	{
		if (hediff.def == null)
		{
			Log.Error("Tried to add health diff with null def. Canceling.");
			return;
		}
		if (hediff.Part != null && !GetNotMissingParts().Contains(hediff.Part))
		{
			Log.Error("Tried to add health diff to missing part " + hediff.Part);
			return;
		}
		hediff.ageTicks = 0;
		hediff.pawn = pawn;
		bool flag = false;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].TryMergeWith(hediff))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			hediffs.Add(hediff);
			hediff.PostAdd(dinfo);
			if (pawn.needs != null && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
		}
		bool flag2 = hediff is Hediff_MissingPart;
		if (!(hediff is Hediff_MissingPart) && hediff.Part != null && hediff.Part != pawn.RaceProps.body.corePart && GetPartHealth(hediff.Part) == 0f && hediff.Part != pawn.RaceProps.body.corePart)
		{
			bool flag3 = HasDirectlyAddedPartFor(hediff.Part);
			Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
			hediff_MissingPart.IsFresh = !flag3;
			hediff_MissingPart.lastInjury = hediff.def;
			pawn.health.AddHediff(hediff_MissingPart, hediff.Part, dinfo);
			damageResult?.AddHediff(hediff_MissingPart);
			if (flag3)
			{
				if (dinfo.HasValue)
				{
					hediff_MissingPart.lastInjury = HealthUtility.GetHediffDefFromDamage(dinfo.Value.Def, pawn, hediff.Part);
				}
				else
				{
					hediff_MissingPart.lastInjury = null;
				}
			}
			flag2 = true;
		}
		DirtyCache();
		if (flag2 && pawn.apparel != null)
		{
			pawn.apparel.Notify_LostBodyPart();
		}
		HediffStage curStage = hediff.CurStage;
		if (hediff.def.chemicalNeed != null && !pawn.Dead)
		{
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
		else if (curStage != null && (!curStage.enablesNeeds.NullOrEmpty() || !curStage.disablesNeeds.NullOrEmpty()) && !pawn.Dead)
		{
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
		if (hediff.def.HasDefinedGraphicProperties || hediff.def.forceRenderTreeRecache)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public void DirtyCache()
	{
		CacheMissingPartsCommonAncestors();
		CacheThoughtsNullified();
		CacheNeeds();
		pawn.Drawer.renderer.WoundOverlays.ClearCache();
		PortraitsCache.SetDirty(pawn);
		GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
		cachedPain = -1f;
		cachedBleedRate = -1f;
		cachedHasHead = null;
		cachedCanCrawl = null;
		cachedHasRegeneration = null;
		cachedRemoveRoamMtb = null;
		cachedPreventVacuumBurns = null;
		cachedDamageFactors.Clear();
		pawn.health.capacities.Notify_CapacityLevelsDirty();
		pawn.health.summaryHealth.Notify_HealthChanged();
	}

	public void DirtyPainCache()
	{
		cachedPain = -1f;
	}

	public void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		for (int num = hediffs.Count - 1; num >= 0; num--)
		{
			hediffs[num].Notify_PawnDied(dinfo, culprit);
		}
	}

	public void Notify_Regenerated(float amount)
	{
		foreach (Hediff hediff in hediffs)
		{
			hediff.Notify_Regenerated(amount);
		}
	}

	public float GetHungerRateFactor(HediffDef ignore = null)
	{
		float num = 1f;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def != ignore)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null)
				{
					num *= curStage.hungerRateFactor;
				}
			}
		}
		for (int j = 0; j < hediffs.Count; j++)
		{
			if (hediffs[j].def != ignore)
			{
				HediffStage curStage2 = hediffs[j].CurStage;
				if (curStage2 != null)
				{
					num += curStage2.hungerRateFactorOffset;
				}
			}
		}
		return Mathf.Max(num, 0f);
	}

	public int GetHediffCount(HediffDef def)
	{
		int num = 0;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def)
			{
				num++;
			}
		}
		return num;
	}

	public void GetHediffs<T>(ref List<T> resultHediffs, Predicate<T> filter = null) where T : Hediff
	{
		resultHediffs.Clear();
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is T val && (filter == null || filter(val)))
			{
				resultHediffs.Add(val);
			}
		}
	}

	public Hediff GetMostRecentHediff(HediffDef def)
	{
		Hediff hediff = null;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def && (hediff == null || hediffs[i].ageTicks < hediff.ageTicks))
			{
				hediff = hediffs[i];
			}
		}
		return hediff;
	}

	public T GetFirstHediff<T>() where T : Hediff
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public bool TryGetHediff(HediffDef def, out Hediff hediff)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def)
			{
				hediff = hediffs[i];
				return true;
			}
		}
		hediff = null;
		return false;
	}

	public bool TryGetHediff<T>(out T hediff) where T : Hediff
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is T val)
			{
				hediff = val;
				return true;
			}
		}
		hediff = null;
		return false;
	}

	public Hediff GetFirstHediffOfDef(HediffDef def, bool mustBeVisible = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def && (!mustBeVisible || hediffs[i].Visible))
			{
				return hediffs[i];
			}
		}
		return null;
	}

	public IEnumerable<T> GetHediffComps<T>() where T : HediffComp
	{
		int i = 0;
		while (i < hediffs.Count)
		{
			if (hediffs[i].TryGetComp<T>(out var comp))
			{
				yield return comp;
			}
			int num = i + 1;
			i = num;
		}
	}

	public T GetFirstHediffMatchingPart<T>(BodyPartRecord part) where T : Hediff
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is T val && val.Part == part)
			{
				return val;
			}
		}
		return null;
	}

	public bool PartIsMissing(BodyPartRecord part)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].Part == part && hediffs[i] is Hediff_MissingPart)
			{
				return true;
			}
		}
		return false;
	}

	public float GetPartHealth(BodyPartRecord part)
	{
		if (part == null)
		{
			return 0f;
		}
		float num = part.def.GetMaxHealth(pawn);
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is Hediff_MissingPart && hediffs[i].Part == part)
			{
				return 0f;
			}
			if (hediffs[i].Part == part && hediffs[i] is Hediff_Injury hediff_Injury)
			{
				num = ((!hediff_Injury.destroysBodyParts) ? Mathf.Max(num - hediff_Injury.Severity, 1f) : (num - hediff_Injury.Severity));
			}
		}
		num = Mathf.Max(num, 0f);
		if (!part.def.destroyableByDamage)
		{
			num = Mathf.Max(num, 1f);
		}
		return Mathf.RoundToInt(num);
	}

	public BodyPartRecord GetBrain()
	{
		foreach (BodyPartRecord notMissingPart in GetNotMissingParts())
		{
			if (notMissingPart.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
			{
				return notMissingPart;
			}
		}
		return null;
	}

	public bool HasHediff(HediffDef def, bool mustBeVisible = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def && (!mustBeVisible || hediffs[i].Visible))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHediff<T>(bool mustBeVisible = false) where T : Hediff
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is T && (!mustBeVisible || hediffs[i].Visible))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetNeedEnablingHediff(NeedDef def, out Hediff hediff)
	{
		if (cachedEnabledNeeds == null)
		{
			hediff = null;
			return false;
		}
		return cachedEnabledNeeds.TryGetValue(def, out hediff);
	}

	public bool TryGetNeedDisablingHediff(NeedDef def, out Hediff hediff)
	{
		if (cachedDisabledNeeds == null)
		{
			hediff = null;
			return false;
		}
		return cachedDisabledNeeds.TryGetValue(def, out hediff);
	}

	public bool EnablesNeed(NeedDef def)
	{
		return cachedEnabledNeeds?.ContainsKey(def) ?? false;
	}

	public bool DisablesNeed(NeedDef def)
	{
		return cachedDisabledNeeds?.ContainsKey(def) ?? false;
	}

	public bool HasHediffOrWillBecome(HediffDef def, bool mustBeVisible = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (!mustBeVisible || hediffs[i].Visible)
			{
				if (hediffs[i].def == def)
				{
					return true;
				}
				if (CheckHediffDefAndChildren(def, hediffs[i].def))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckHediffDefAndChildren(HediffDef toFind, HediffDef hediff)
	{
		if (hediff == toFind)
		{
			return true;
		}
		if (!hediff.comps.NullOrEmpty())
		{
			foreach (HediffCompProperties comp in hediff.comps)
			{
				if (!(comp is HediffCompProperties_ReplaceHediff hediffCompProperties_ReplaceHediff))
				{
					continue;
				}
				foreach (HediffCompProperties_ReplaceHediff.TriggeredHediff hediff2 in hediffCompProperties_ReplaceHediff.hediffs)
				{
					if (CheckHediffDefAndChildren(toFind, hediff2.hediff))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasHediff(HediffDef def, BodyPartRecord bodyPart, bool mustBeVisible = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == def && hediffs[i].Part == bodyPart && (!mustBeVisible || hediffs[i].Visible))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetBodyPartRecord(BodyPartDef def, out BodyPartRecord record)
	{
		record = GetBodyPartRecord(def);
		return record != null;
	}

	public BodyPartRecord GetBodyPartRecord(BodyPartDef partDef)
	{
		foreach (BodyPartRecord notMissingPart in GetNotMissingParts())
		{
			if (notMissingPart.def == partDef)
			{
				return notMissingPart;
			}
		}
		return null;
	}

	public bool IsBionicOrImplant(BodyPartDef partDef)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			Hediff hediff = hediffs[i];
			if (hediff.Part != null && hediff.Part.def == partDef && hediff.def.countsAsAddedPartOrImplant)
			{
				return true;
			}
		}
		return false;
	}

	public List<Verb> GetHediffsVerbs()
	{
		tmpHediffVerbs.Clear();
		for (int i = 0; i < hediffs.Count; i++)
		{
			HediffComp_VerbGiver hediffComp_VerbGiver = hediffs[i].TryGetComp<HediffComp_VerbGiver>();
			if (hediffComp_VerbGiver != null)
			{
				List<Verb> allVerbs = hediffComp_VerbGiver.VerbTracker.AllVerbs;
				for (int j = 0; j < allVerbs.Count; j++)
				{
					tmpHediffVerbs.Add(allVerbs[j]);
				}
			}
		}
		return tmpHediffVerbs;
	}

	public IEnumerable<Hediff> GetHediffsTendable()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].TendableNow())
			{
				yield return hediffs[i];
			}
		}
	}

	public bool HasTendableHediff(bool forAlert = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((!forAlert || hediffs[i].def.makesAlert) && hediffs[i].TendableNow())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHediffBlocksSleeping()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.blocksSleeping)
			{
				return true;
			}
			HediffStage curStage = hediffs[i].CurStage;
			if (curStage != null && curStage.blocksSleeping)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHediffPreventsPregnancy()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.preventsPregnancy)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPregnancyHediff()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.pregnant)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<HediffComp> GetAllComps()
	{
		foreach (Hediff hediff in hediffs)
		{
			if (!(hediff is HediffWithComps hediffWithComps))
			{
				continue;
			}
			foreach (HediffComp comp in hediffWithComps.comps)
			{
				yield return comp;
			}
		}
	}

	public bool HasHediffShowGizmosOnCorpse()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.showGizmosOnCorpse)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNaturallyHealingInjury()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is Hediff_Injury hd && hd.CanHealNaturally())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTendedAndHealingInjury()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is Hediff_Injury hediff_Injury && hediff_Injury.CanHealFromTending() && hediff_Injury.Severity > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTemperatureInjury(TemperatureInjuryStage minStage)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((hediffs[i].def == HediffDefOf.Hypothermia || hediffs[i].def == HediffDefOf.Heatstroke) && hediffs[i].CurStageIndex >= (int)minStage)
			{
				return true;
			}
		}
		return false;
	}

	public List<BodyPartRecord> GetInjuredParts()
	{
		tmpInjuredParts.Clear();
		foreach (Hediff hediff in hediffs)
		{
			if (hediff is Hediff_Injury hediff_Injury && !tmpInjuredParts.Contains(hediff_Injury.Part))
			{
				tmpInjuredParts.Add(hediff_Injury.Part);
			}
		}
		return tmpInjuredParts;
	}

	public List<BodyPartRecord> GetNaturallyHealingInjuredParts()
	{
		tmpNaturallyHealingInjuredParts.Clear();
		foreach (BodyPartRecord injuredPart in GetInjuredParts())
		{
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_Injury hd && hediffs[i].Part == injuredPart && hd.CanHealNaturally())
				{
					tmpNaturallyHealingInjuredParts.Add(injuredPart);
					break;
				}
			}
		}
		return tmpNaturallyHealingInjuredParts;
	}

	public List<Hediff_MissingPart> GetMissingPartsCommonAncestors()
	{
		if (cachedMissingPartsCommonAncestors == null)
		{
			CacheMissingPartsCommonAncestors();
		}
		return cachedMissingPartsCommonAncestors;
	}

	public bool HasMissingPartFor(BodyPartRecord part)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].Part == part && hediffs[i] is Hediff_MissingPart)
			{
				return true;
			}
		}
		return false;
	}

	public Hediff GetMissingPartFor(BodyPartRecord part)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].Part == part && hediffs[i] is Hediff_MissingPart)
			{
				return hediffs[i];
			}
		}
		return null;
	}

	public IEnumerable<BodyPartRecord> GetNotMissingParts(BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartTagDef tag = null, BodyPartRecord partParent = null)
	{
		List<BodyPartRecord> allPartsList = pawn.def.race.body.AllParts;
		for (int i = 0; i < allPartsList.Count; i++)
		{
			BodyPartRecord bodyPartRecord = allPartsList[i];
			if (!PartIsMissing(bodyPartRecord) && (height == BodyPartHeight.Undefined || bodyPartRecord.height == height) && (depth == BodyPartDepth.Undefined || bodyPartRecord.depth == depth) && (tag == null || bodyPartRecord.def.tags.Contains(tag)) && (partParent == null || bodyPartRecord.parent == partParent))
			{
				yield return bodyPartRecord;
			}
		}
	}

	public BodyPartRecord GetRandomNotMissingPart(DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
	{
		IEnumerable<BodyPartRecord> enumerable = null;
		if (GetNotMissingParts(height, depth, null, partParent).Any((BodyPartRecord p) => p.coverageAbs > 0f))
		{
			enumerable = GetNotMissingParts(height, depth, null, partParent);
		}
		else
		{
			if (!GetNotMissingParts(BodyPartHeight.Undefined, depth, null, partParent).Any((BodyPartRecord p) => p.coverageAbs > 0f))
			{
				return null;
			}
			enumerable = GetNotMissingParts(BodyPartHeight.Undefined, depth, null, partParent);
		}
		if (enumerable.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs * x.def.GetHitChanceFactorFor(damDef), out var result))
		{
			return result;
		}
		if (enumerable.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out result))
		{
			return result;
		}
		return null;
	}

	public float GetCoverageOfNotMissingNaturalParts(BodyPartRecord part)
	{
		if (PartIsMissing(part))
		{
			return 0f;
		}
		if (PartOrAnyAncestorHasDirectlyAddedParts(part))
		{
			return 0f;
		}
		coverageRejectedPartsSet.Clear();
		List<Hediff_MissingPart> missingPartsCommonAncestors = GetMissingPartsCommonAncestors();
		for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
		{
			coverageRejectedPartsSet.Add(missingPartsCommonAncestors[i].Part);
		}
		for (int j = 0; j < hediffs.Count; j++)
		{
			if (hediffs[j] is Hediff_AddedPart)
			{
				coverageRejectedPartsSet.Add(hediffs[j].Part);
			}
		}
		float num = 0f;
		coveragePartsStack.Clear();
		coveragePartsStack.Push(part);
		while (coveragePartsStack.Any())
		{
			BodyPartRecord bodyPartRecord = coveragePartsStack.Pop();
			num += bodyPartRecord.coverageAbs;
			for (int k = 0; k < bodyPartRecord.parts.Count; k++)
			{
				if (!coverageRejectedPartsSet.Contains(bodyPartRecord.parts[k]))
				{
					coveragePartsStack.Push(bodyPartRecord.parts[k]);
				}
			}
		}
		coveragePartsStack.Clear();
		coverageRejectedPartsSet.Clear();
		return num;
	}

	private void CacheMissingPartsCommonAncestors()
	{
		if (cachedMissingPartsCommonAncestors == null)
		{
			cachedMissingPartsCommonAncestors = new List<Hediff_MissingPart>();
		}
		else
		{
			cachedMissingPartsCommonAncestors.Clear();
		}
		missingPartsCommonAncestorsQueue.Clear();
		missingPartsCommonAncestorsQueue.Enqueue(pawn.def.race.body.corePart);
		while (missingPartsCommonAncestorsQueue.Count != 0)
		{
			BodyPartRecord bodyPartRecord = missingPartsCommonAncestorsQueue.Dequeue();
			if (PartOrAnyAncestorHasDirectlyAddedParts(bodyPartRecord))
			{
				continue;
			}
			Hediff_MissingPart firstHediffMatchingPart = GetFirstHediffMatchingPart<Hediff_MissingPart>(bodyPartRecord);
			if (firstHediffMatchingPart != null)
			{
				cachedMissingPartsCommonAncestors.Add(firstHediffMatchingPart);
				continue;
			}
			for (int i = 0; i < bodyPartRecord.parts.Count; i++)
			{
				missingPartsCommonAncestorsQueue.Enqueue(bodyPartRecord.parts[i]);
			}
		}
	}

	private void CacheNeeds()
	{
		cachedDisabledNeeds?.Clear();
		cachedEnabledNeeds?.Clear();
		for (int i = 0; i < hediffs.Count; i++)
		{
			Hediff hediff = hediffs[i];
			if (hediff.def.chemicalNeed != null)
			{
				if (cachedEnabledNeeds == null)
				{
					cachedEnabledNeeds = new Dictionary<NeedDef, Hediff>();
				}
				cachedEnabledNeeds[hediff.def.chemicalNeed] = hediff;
			}
			if (hediff.CurStage == null)
			{
				continue;
			}
			if (!hediff.CurStage.enablesNeeds.NullOrEmpty())
			{
				for (int j = 0; j < hediff.CurStage.enablesNeeds.Count; j++)
				{
					NeedDef key = hediff.CurStage.enablesNeeds[j];
					if (cachedEnabledNeeds == null)
					{
						cachedEnabledNeeds = new Dictionary<NeedDef, Hediff>();
					}
					cachedEnabledNeeds[key] = hediff;
				}
			}
			if (hediff.CurStage.disablesNeeds.NullOrEmpty())
			{
				continue;
			}
			for (int k = 0; k < hediff.CurStage.disablesNeeds.Count; k++)
			{
				NeedDef key2 = hediff.CurStage.disablesNeeds[k];
				if (cachedDisabledNeeds == null)
				{
					cachedDisabledNeeds = new Dictionary<NeedDef, Hediff>();
				}
				cachedDisabledNeeds[key2] = hediff;
			}
		}
	}

	public void CacheThoughtsNullified()
	{
		cachedMaxPctConditionalThoughtsNullified = 0f;
		cachedMaxPctAllThoughtsNullified = 0f;
		cachedConditionalThoughtNullifyingHediff = null;
		cachedAllThoughtNullifyingHediff = null;
		foreach (Hediff hediff in hediffs)
		{
			HediffStage curStage = hediff.CurStage;
			if (curStage != null)
			{
				if (curStage.pctConditionalThoughtsNullified > cachedMaxPctConditionalThoughtsNullified)
				{
					cachedMaxPctConditionalThoughtsNullified = curStage.pctConditionalThoughtsNullified;
					cachedConditionalThoughtNullifyingHediff = hediff;
				}
				if (curStage.pctAllThoughtNullification > cachedMaxPctAllThoughtsNullified)
				{
					cachedMaxPctAllThoughtsNullified = curStage.pctAllThoughtNullification;
					cachedAllThoughtNullifyingHediff = hediff;
				}
			}
		}
	}

	public Hediff ThoughtNullifyingHediff(ThoughtDef def)
	{
		if (def.nullifyingHediffs != null)
		{
			for (int i = 0; i < def.nullifyingHediffs.Count; i++)
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.nullifyingHediffs[i]);
				if (firstHediffOfDef != null)
				{
					return firstHediffOfDef;
				}
			}
		}
		float num = 0f;
		Hediff result = null;
		if (!def.IsMemory)
		{
			num = cachedMaxPctConditionalThoughtsNullified;
			result = cachedConditionalThoughtNullifyingHediff;
		}
		if (cachedMaxPctAllThoughtsNullified > num)
		{
			num = cachedMaxPctAllThoughtsNullified;
			result = cachedAllThoughtNullifyingHediff;
		}
		if (num == 0f)
		{
			return null;
		}
		Rand.PushState();
		Rand.Seed = pawn.thingIDNumber * 31 + def.index * 139;
		bool num2 = Rand.Value < num;
		Rand.PopState();
		if (!num2)
		{
			return null;
		}
		return result;
	}

	public bool HasDirectlyAddedPartFor(BodyPartRecord part)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].Part == part && hediffs[i] is Hediff_AddedPart)
			{
				return true;
			}
		}
		return false;
	}

	public Hediff GetDirectlyAddedPartFor(BodyPartRecord part)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].Part == part && hediffs[i] is Hediff_AddedPart)
			{
				return hediffs[i];
			}
		}
		return null;
	}

	public bool TryGetDirectlyAddedPartFor(BodyPartRecord part, out Hediff hediff)
	{
		hediff = GetDirectlyAddedPartFor(part);
		return hediff != null;
	}

	public bool PartOrAnyAncestorHasDirectlyAddedParts(BodyPartRecord part)
	{
		if (HasDirectlyAddedPartFor(part))
		{
			return true;
		}
		if (part.parent != null && PartOrAnyAncestorHasDirectlyAddedParts(part.parent))
		{
			return true;
		}
		return false;
	}

	public bool AncestorHasDirectlyAddedParts(BodyPartRecord part)
	{
		if (part.parent != null && PartOrAnyAncestorHasDirectlyAddedParts(part.parent))
		{
			return true;
		}
		return false;
	}

	public IEnumerable<Hediff> GetTendableNonInjuryNonMissingPartHediffs()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (!(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && hediffs[i].TendableNow())
			{
				yield return hediffs[i];
			}
		}
	}

	public bool HasTendableNonInjuryNonMissingPartHediff(bool forAlert = false)
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((!forAlert || hediffs[i].def.makesAlert) && !(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && hediffs[i].TendableNow())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasImmunizableNotImmuneHediff()
	{
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (!(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && hediffs[i].Visible && hediffs[i].def.PossibleToDevelopImmunityNaturally() && !hediffs[i].FullyImmune())
			{
				return true;
			}
		}
		return false;
	}

	public bool InLabor(bool includePostpartumExhaustion = true)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (HasHediff(HediffDefOf.PregnancyLabor) || HasHediff(HediffDefOf.PregnancyLaborPushing))
		{
			return true;
		}
		if (includePostpartumExhaustion && HasHediff(HediffDefOf.PostpartumExhaustion))
		{
			return true;
		}
		return false;
	}

	private float CalculateBleedRate()
	{
		if (pawn.health.Dead)
		{
			return 0f;
		}
		if (pawn.Deathresting)
		{
			return 0f;
		}
		if (!pawn.health.CanBleed)
		{
			return 0f;
		}
		float num = 1f;
		float num2 = 0f;
		for (int i = 0; i < hediffs.Count; i++)
		{
			Hediff hediff = hediffs[i];
			HediffStage curStage = hediff.CurStage;
			if (curStage != null)
			{
				num *= curStage.totalBleedFactor;
			}
			num2 += hediff.BleedRate;
		}
		return num2 * num / pawn.HealthScale;
	}

	private float CalculatePain()
	{
		if (!pawn.RaceProps.IsFlesh || pawn.Dead)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < hediffs.Count; i++)
		{
			num += hediffs[i].PainOffset;
		}
		if (pawn.genes != null)
		{
			num += pawn.genes.PainOffset;
		}
		if (pawn.story?.traits != null)
		{
			List<Trait> allTraits = pawn.story.traits.allTraits;
			for (int j = 0; j < allTraits.Count; j++)
			{
				num += allTraits[j].CurrentData.painOffset;
			}
		}
		for (int k = 0; k < hediffs.Count; k++)
		{
			num *= hediffs[k].PainFactor;
		}
		if (pawn.genes != null)
		{
			num *= pawn.genes.PainFactor;
		}
		if (pawn.story?.traits != null)
		{
			List<Trait> allTraits2 = pawn.story.traits.allTraits;
			for (int l = 0; l < allTraits2.Count; l++)
			{
				num *= allTraits2[l].CurrentData.painFactor;
			}
		}
		return Mathf.Clamp(num, 0f, 1f);
	}

	public Color GetSkinColor(Color baseColor)
	{
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.def.skinColorTint.HasValue)
			{
				baseColor = baseColor * (1f - hediff.def.skinColorTintStrength) + hediff.def.skinColorTint.Value * hediff.def.skinColorTintStrength;
			}
			if (hediff.def.skinColorOverride.HasValue)
			{
				return hediff.def.skinColorOverride.Value;
			}
		}
		return baseColor;
	}

	public void Clear()
	{
		hediffs.Clear();
		DirtyCache();
	}
}
