using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_GeneTracker : IExposable
{
	public Pawn pawn;

	public string xenotypeName;

	public XenotypeIconDef iconDef;

	public bool hybrid;

	private XenotypeDef xenotype;

	private List<Gene> xenogenes = new List<Gene>();

	private List<Gene> endogenes = new List<Gene>();

	[Unsaved(false)]
	private Dictionary<DamageDef, float> cachedDamageFactors = new Dictionary<DamageDef, float>();

	[Unsaved(false)]
	private Dictionary<ChemicalDef, float> cachedAddictionChanceFactors = new Dictionary<ChemicalDef, float>();

	[Unsaved(false)]
	private Dictionary<NeedDef, Gene> cachedEnabledNeeds;

	[Unsaved(false)]
	private Dictionary<NeedDef, Gene> cachedDisabledNeeds;

	[Unsaved(false)]
	private List<Gene> cachedGenes;

	[Unsaved(false)]
	private bool? cachedGenesAffectAge;

	[Unsaved(false)]
	private CustomXenotype cachedCustomXenotype;

	[Unsaved(false)]
	private bool? cachedHasCustomXenotype;

	[Unsaved(false)]
	private bool? cachedTattoosVisible;

	[Unsaved(false)]
	private int? cachedWaterCellCost;

	[Unsaved(false)]
	private bool hasCachedWaterCost;

	private const int LearningOpportunityCheckInterval = 300;

	private List<GeneDefWithType> tmpGeneDefWithTypes = new List<GeneDefWithType>();

	private readonly List<Gene> tmpGenes = new List<Gene>();

	public XenotypeDef Xenotype => xenotype ?? XenotypeDefOf.Baseliner;

	public bool UniqueXenotype => !xenotypeName.NullOrEmpty();

	public string XenotypeLabel
	{
		get
		{
			if (!UniqueXenotype)
			{
				return Xenotype.label;
			}
			return xenotypeName ?? ((string)"Unique".Translate());
		}
	}

	public string XenotypeLabelCap => XenotypeLabel.CapitalizeFirst();

	public CustomXenotype CustomXenotype
	{
		get
		{
			if (Xenotype != XenotypeDefOf.Baseliner || hybrid || Current.ProgramState != ProgramState.Playing)
			{
				return null;
			}
			if (!cachedHasCustomXenotype.HasValue)
			{
				cachedHasCustomXenotype = false;
				foreach (CustomXenotype customXenotype in Current.Game.customXenotypeDatabase.customXenotypes)
				{
					if (GeneUtility.PawnIsCustomXenotype(pawn, customXenotype))
					{
						cachedHasCustomXenotype = true;
						cachedCustomXenotype = customXenotype;
						break;
					}
				}
			}
			return cachedCustomXenotype;
		}
	}

	public string XenotypeDescShort
	{
		get
		{
			if (UniqueXenotype)
			{
				return "UniqueXenotypeDesc".Translate();
			}
			if (!Xenotype.descriptionShort.NullOrEmpty())
			{
				return Xenotype.descriptionShort + "\n\n" + "MoreInfoInInfoScreen".Translate().Colorize(ColoredText.SubtleGrayColor);
			}
			return Xenotype.description;
		}
	}

	public Texture2D XenotypeIcon
	{
		get
		{
			if (!ModsConfig.BiotechActive)
			{
				return null;
			}
			if (iconDef != null)
			{
				return iconDef.Icon;
			}
			if (UniqueXenotype)
			{
				return XenotypeIconDefOf.Basic.Icon;
			}
			return Xenotype.Icon;
		}
	}

	public List<Gene> GenesListForReading
	{
		get
		{
			if (cachedGenes == null)
			{
				cachedGenes = new List<Gene>();
				cachedGenes.AddRange(xenogenes);
				cachedGenes.AddRange(endogenes);
			}
			return cachedGenes;
		}
	}

	public List<Gene> Xenogenes => xenogenes;

	public List<Gene> Endogenes => endogenes;

	public float PainOffset
	{
		get
		{
			float num = 0f;
			if (!ModLister.BiotechInstalled)
			{
				return num;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					num += genesListForReading[i].def.painOffset;
				}
			}
			return num;
		}
	}

	public float PainFactor
	{
		get
		{
			float num = 1f;
			if (!ModLister.BiotechInstalled)
			{
				return num;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					num *= genesListForReading[i].def.painFactor;
				}
			}
			return num;
		}
	}

	public float SocialFightChanceFactor
	{
		get
		{
			float num = 1f;
			if (!ModLister.BiotechInstalled)
			{
				return num;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					num *= genesListForReading[i].def.socialFightChanceFactor;
				}
			}
			return num;
		}
	}

	public float AggroMentalBreakSelectionChanceFactor
	{
		get
		{
			float num = 1f;
			if (!ModLister.BiotechInstalled)
			{
				return num;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					num *= genesListForReading[i].def.aggroMentalBreakSelectionChanceFactor;
				}
			}
			return num;
		}
	}

	public bool AffectedByDarkness
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return true;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && genesListForReading[i].def.ignoreDarkness)
				{
					return false;
				}
			}
			return true;
		}
	}

	public float BiologicalAgeTickFactor
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return 1f;
			}
			if (cachedGenesAffectAge.HasValue && !cachedGenesAffectAge.Value)
			{
				return 1f;
			}
			bool valueOrDefault = cachedGenesAffectAge == true;
			if (!cachedGenesAffectAge.HasValue)
			{
				valueOrDefault = false;
				cachedGenesAffectAge = valueOrDefault;
			}
			float num = 1f;
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && genesListForReading[i].def.biologicalAgeTickFactorFromAgeCurve != null)
				{
					num *= genesListForReading[i].def.biologicalAgeTickFactorFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
					cachedGenesAffectAge = true;
				}
			}
			return num;
		}
	}

	public bool EnjoysSunlight
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return true;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].def.dislikesSunlight && genesListForReading[i].Active)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool ShouldHaveCallTracker
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return false;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && genesListForReading[i].def.soundCall != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool DontMindRawFood
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return false;
			}
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && genesListForReading[i].def.dontMindRawFood)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool CanHaveBeard
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return pawn.gender != Gender.Female;
			}
			if (pawn.gender == Gender.Female)
			{
				List<Gene> genesListForReading = GenesListForReading;
				for (int i = 0; i < genesListForReading.Count; i++)
				{
					if (genesListForReading[i].Active && genesListForReading[i].def.womenCanHaveBeards)
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
	}

	public float PrisonBreakIntervalFactor
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return 1f;
			}
			float num = 1f;
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					num *= genesListForReading[i].def.prisonBreakMTBFactor;
				}
			}
			return num;
		}
	}

	public WorkTags DisabledWorkTags
	{
		get
		{
			if (!ModLister.BiotechInstalled)
			{
				return WorkTags.None;
			}
			List<Gene> genesListForReading = GenesListForReading;
			WorkTags workTags = WorkTags.None;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active)
				{
					workTags |= genesListForReading[i].def.disabledWorkTags;
				}
			}
			return workTags;
		}
	}

	public bool TattoosVisible
	{
		get
		{
			if (cachedTattoosVisible.HasValue)
			{
				return cachedTattoosVisible.Value;
			}
			cachedTattoosVisible = true;
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && !genesListForReading[i].def.tattoosVisible)
				{
					cachedTattoosVisible = false;
					break;
				}
			}
			return cachedTattoosVisible.Value;
		}
	}

	public int? WaterCellCost
	{
		get
		{
			if (hasCachedWaterCost)
			{
				return cachedWaterCellCost;
			}
			hasCachedWaterCost = true;
			List<Gene> genesListForReading = GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].Active && genesListForReading[i].def.waterCellCost.HasValue)
				{
					int? waterCellCost = genesListForReading[i].def.waterCellCost;
					if (!cachedWaterCellCost.HasValue || waterCellCost.Value < cachedWaterCellCost.Value)
					{
						cachedWaterCellCost = waterCellCost;
					}
				}
			}
			return cachedWaterCellCost;
		}
	}

	public Pawn_GeneTracker()
	{
	}

	public Pawn_GeneTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void SetXenotype(XenotypeDef xenotype)
	{
		if (ModLister.CheckBiotech("Xenotypes"))
		{
			this.xenotype = xenotype;
			xenotypeName = null;
			iconDef = null;
			cachedHasCustomXenotype = null;
			cachedCustomXenotype = null;
			ClearXenogenes();
			for (int i = 0; i < xenotype.genes.Count; i++)
			{
				AddGene(xenotype.genes[i], !xenotype.inheritable);
			}
		}
	}

	public void SetXenotypeDirect(XenotypeDef xenotype)
	{
		if (ModLister.CheckBiotech("Xenotypes"))
		{
			this.xenotype = xenotype;
			xenotypeName = null;
			cachedHasCustomXenotype = null;
			cachedCustomXenotype = null;
		}
	}

	[Obsolete("Use HasActiveGene() instead")]
	public bool HasGene(GeneDef geneDef)
	{
		if (geneDef == null)
		{
			return false;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].def == geneDef)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveGene(GeneDef geneDef)
	{
		if (geneDef == null)
		{
			return false;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].def == geneDef && genesListForReading[i].Active)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetNeedEnablingGene(NeedDef def, out Gene gene)
	{
		if (cachedEnabledNeeds == null)
		{
			gene = null;
			return false;
		}
		return cachedEnabledNeeds.TryGetValue(def, out gene);
	}

	public bool TryGetNeedDisablingGene(NeedDef def, out Gene gene)
	{
		if (cachedDisabledNeeds == null)
		{
			gene = null;
			return false;
		}
		return cachedDisabledNeeds.TryGetValue(def, out gene);
	}

	public bool EnablesNeed(NeedDef def)
	{
		return cachedEnabledNeeds?.ContainsKey(def) ?? false;
	}

	public bool DisablesNeed(NeedDef def)
	{
		return cachedDisabledNeeds?.ContainsKey(def) ?? false;
	}

	public bool IsXenogene(Gene gene)
	{
		return xenogenes.Contains(gene);
	}

	public bool HasXenogene(GeneDef geneDef)
	{
		for (int i = 0; i < xenogenes.Count; i++)
		{
			if (xenogenes[i].def == geneDef)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEndogene(GeneDef geneDef)
	{
		for (int i = 0; i < endogenes.Count; i++)
		{
			if (endogenes[i].def == geneDef)
			{
				return true;
			}
		}
		return false;
	}

	public Gene GetGene(GeneDef geneDef)
	{
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].def == geneDef)
			{
				return genesListForReading[i];
			}
		}
		return null;
	}

	public T GetFirstGeneOfType<T>() where T : Gene
	{
		if (!ModLister.BiotechInstalled)
		{
			return null;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public GeneDef GetMelaninGene()
	{
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i].def.endogeneCategory == EndogeneCategory.Melanin)
			{
				return genesListForReading[i].def;
			}
		}
		return null;
	}

	public GeneDef GetHairColorGene()
	{
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i].def.endogeneCategory == EndogeneCategory.HairColor)
			{
				return genesListForReading[i].def;
			}
		}
		return null;
	}

	public GeneDef GetFirstEndogeneByCategory(EndogeneCategory cat)
	{
		List<Gene> list = Endogenes;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Active && list[i].def.endogeneCategory == cat)
			{
				return list[i].def;
			}
		}
		return null;
	}

	public Gene AddGene(GeneDef geneDef, bool xenogene)
	{
		if (xenogene && !ModLister.BiotechInstalled)
		{
			return null;
		}
		if (!xenogene && HasEndogene(geneDef))
		{
			return null;
		}
		return AddGene(GeneMaker.MakeGene(geneDef, pawn), xenogene);
	}

	private Gene AddGene(Gene gene, bool addAsXenogene)
	{
		if (addAsXenogene && !ModLister.BiotechInstalled)
		{
			return null;
		}
		if (addAsXenogene)
		{
			xenogenes.Add(gene);
		}
		else
		{
			endogenes.Add(gene);
		}
		cachedGenes = null;
		if (ModLister.BiotechInstalled)
		{
			CheckForOverrides();
			if (!gene.def.abilities.NullOrEmpty() && pawn.abilities != null)
			{
				for (int i = 0; i < gene.def.abilities.Count; i++)
				{
					pawn.abilities.GainAbility(gene.def.abilities[i]);
				}
			}
			if (!gene.def.forcedTraits.NullOrEmpty() && pawn.story != null)
			{
				for (int j = 0; j < gene.def.forcedTraits.Count; j++)
				{
					Trait trait = new Trait(gene.def.forcedTraits[j].def, gene.def.forcedTraits[j].degree);
					trait.sourceGene = gene;
					pawn.story.traits.GainTrait(trait, suppressConflicts: true);
				}
			}
			if (gene.def.passionMod != null)
			{
				SkillRecord skill = pawn.skills.GetSkill(gene.def.passionMod.skill);
				gene.passionPreAdd = skill.passion;
				skill.passion = gene.def.passionMod.NewPassionFor(skill);
			}
			pawn.story?.traits?.RecalculateSuppression();
			if (gene.def.disabledWorkTags.HasFlag(WorkTags.Violent) && pawn.equipment?.Primary != null)
			{
				if (pawn.PositionHeld.IsValid && pawn.MapHeld != null)
				{
					pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var _, pawn.PositionHeld, forbid: false);
				}
				else
				{
					pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
				}
			}
		}
		Notify_GenesChanged(gene.def);
		if (ModLister.BiotechInstalled)
		{
			gene.PostAdd();
		}
		return gene;
	}

	public void RemoveGene(Gene gene)
	{
		if (!xenogenes.Remove(gene) && !endogenes.Remove(gene))
		{
			return;
		}
		cachedGenes = null;
		if (ModLister.BiotechInstalled)
		{
			CheckForOverrides();
			if (pawn.abilities != null && !gene.def.abilities.NullOrEmpty())
			{
				for (int i = 0; i < gene.def.abilities.Count; i++)
				{
					pawn.abilities.RemoveAbility(gene.def.abilities[i]);
				}
			}
			if (!gene.def.forcedTraits.NullOrEmpty() || !gene.def.suppressedTraits.NullOrEmpty())
			{
				pawn.story?.traits.Notify_GeneRemoved(gene);
			}
			if (gene.def.passionMod != null)
			{
				SkillRecord skill = pawn.skills.GetSkill(gene.def.passionMod.skill);
				skill.passion = gene.NewPassionForOnRemoval(skill);
			}
		}
		Notify_GenesChanged(gene.def);
		if (ModLister.BiotechInstalled)
		{
			gene.PostRemove();
		}
	}

	private void CheckForOverrides()
	{
		List<Gene> genesListForReading = GenesListForReading;
		foreach (Gene item in genesListForReading)
		{
			if (!item.def.RandomChosen)
			{
				item.OverrideBy(null);
			}
		}
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].def.RandomChosen)
			{
				continue;
			}
			for (int j = i + 1; j < genesListForReading.Count; j++)
			{
				if (!genesListForReading[j].def.RandomChosen && genesListForReading[i].def.ConflictsWith(genesListForReading[j].def))
				{
					if (genesListForReading[i].def.Overrides(genesListForReading[j].def, IsXenogene(genesListForReading[i]), IsXenogene(genesListForReading[j])))
					{
						genesListForReading[j].OverrideBy(genesListForReading[i]);
					}
					else
					{
						genesListForReading[i].OverrideBy(genesListForReading[j]);
					}
				}
			}
		}
		pawn.skills?.DirtyAptitudes();
		pawn.Notify_DisabledWorkTypesChanged();
	}

	public void ClearXenogenes()
	{
		for (int num = xenogenes.Count - 1; num >= 0; num--)
		{
			RemoveGene(xenogenes[num]);
		}
	}

	public void GeneTrackerTick()
	{
		if (!ModLister.BiotechInstalled)
		{
			return;
		}
		for (int num = xenogenes.Count - 1; num >= 0; num--)
		{
			if (xenogenes[num].Active)
			{
				xenogenes[num].Tick();
			}
		}
		for (int num2 = endogenes.Count - 1; num2 >= 0; num2--)
		{
			if (endogenes[num2].Active)
			{
				endogenes[num2].Tick();
			}
		}
	}

	public void GeneTrackerTickInterval(int delta)
	{
		if (!ModLister.BiotechInstalled)
		{
			return;
		}
		for (int num = xenogenes.Count - 1; num >= 0; num--)
		{
			if (xenogenes[num].Active)
			{
				xenogenes[num].TickInterval(delta);
			}
		}
		for (int num2 = endogenes.Count - 1; num2 >= 0; num2--)
		{
			if (endogenes[num2].Active)
			{
				endogenes[num2].TickInterval(delta);
			}
		}
		if (pawn.Spawned && Xenotype != XenotypeDefOf.Baseliner && pawn.IsHashIntervalTick(300, delta))
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.GenesAndXenotypes, OpportunityType.Important);
		}
	}

	public void Notify_IngestedThing(Thing thing, int numTaken)
	{
		if (!ModLister.BiotechInstalled)
		{
			return;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active)
			{
				genesListForReading[i].Notify_IngestedThing(thing, numTaken);
			}
		}
	}

	public void Reset()
	{
		for (int i = 0; i < GenesListForReading.Count; i++)
		{
			GenesListForReading[i].Reset();
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (!ModLister.BiotechInstalled)
		{
			yield break;
		}
		for (int i = 0; i < GenesListForReading.Count; i++)
		{
			IEnumerable<Gizmo> gizmos = GenesListForReading[i].GetGizmos();
			if (gizmos == null)
			{
				continue;
			}
			foreach (Gizmo item in gizmos)
			{
				yield return item;
			}
		}
	}

	private void Notify_GenesChanged(GeneDef addedOrRemovedGene)
	{
		bool flag = false;
		cachedGenes = null;
		List<Gene> genes = GenesListForReading;
		if (ModLister.BiotechInstalled && addedOrRemovedGene.skinIsHairColor)
		{
			if (SelectGene((Gene x) => x.def.skinIsHairColor, out var chosen))
			{
				pawn.story.skinColorOverride = pawn.story.HairColor;
				OverrideAllConflicting(chosen);
			}
			else
			{
				pawn.story.skinColorOverride = null;
			}
			EnsureCorrectSkinColorOverride();
			flag = true;
		}
		if (addedOrRemovedGene.hairColorOverride.HasValue && SelectGene((Gene g) => g.def.hairColorOverride.HasValue, out var chosen2))
		{
			Color value = chosen2.def.hairColorOverride.Value;
			if (chosen2.def.randomBrightnessFactor != 0f)
			{
				value *= 1f + Rand.Range(0f - chosen2.def.randomBrightnessFactor, chosen2.def.randomBrightnessFactor);
			}
			pawn.story.HairColor = value.ClampToValueRange(GeneTuning.HairColorValueRange);
			OverrideAllConflicting(chosen2);
			EnsureCorrectSkinColorOverride();
			flag = true;
		}
		if (addedOrRemovedGene.skinColorBase.HasValue && SelectGene((Gene g) => g.def.skinColorBase.HasValue, out var chosen3))
		{
			OverrideAllConflicting(chosen3);
			pawn.story.SkinColorBase = chosen3.def.skinColorBase.Value;
			flag = true;
		}
		if (ModLister.BiotechInstalled)
		{
			if (addedOrRemovedGene.skinColorOverride.HasValue)
			{
				if (SelectGene((Gene g) => g.def.skinColorOverride.HasValue, out var chosen4))
				{
					Color value2 = chosen4.def.skinColorOverride.Value;
					if (chosen4.def.randomBrightnessFactor != 0f)
					{
						value2 *= 1f + Rand.Range(0f - chosen4.def.randomBrightnessFactor, chosen4.def.randomBrightnessFactor);
					}
					pawn.story.skinColorOverride = value2.ClampToValueRange(GeneTuning.SkinColorValueRange);
					OverrideAllConflicting(chosen4);
				}
				else
				{
					pawn.story.skinColorOverride = null;
				}
				EnsureCorrectSkinColorOverride();
				flag = true;
			}
			if (addedOrRemovedGene.bodyType.HasValue && !pawn.DevelopmentalStage.Juvenile())
			{
				if (SelectGene((Gene g) => g.def.bodyType.HasValue, out var chosen5))
				{
					OverrideAllConflicting(chosen5);
					pawn.story.bodyType = chosen5.def.bodyType.Value.ToBodyType(pawn);
				}
				else
				{
					pawn.story.bodyType = PawnGenerator.GetBodyTypeFor(pawn);
				}
				flag = true;
			}
			if (!addedOrRemovedGene.forcedHeadTypes.NullOrEmpty())
			{
				if (SelectGene((Gene g) => !g.def.forcedHeadTypes.NullOrEmpty(), out var chosen6))
				{
					OverrideAllConflicting(chosen6);
					if (!pawn.story.TryGetRandomHeadFromSet(chosen6.def.forcedHeadTypes))
					{
						pawn.story.TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where((HeadTypeDef x) => x.randomChosen));
					}
				}
				else
				{
					pawn.story.TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where((HeadTypeDef x) => x.randomChosen));
				}
				flag = true;
			}
			if ((addedOrRemovedGene.forcedHair != null || addedOrRemovedGene.hairTagFilter != null) && !PawnStyleItemChooser.WantsToUseStyle(pawn, pawn.story.hairDef))
			{
				pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
				flag = true;
			}
			if (addedOrRemovedGene.beardTagFilter != null && pawn.style != null && !PawnStyleItemChooser.WantsToUseStyle(pawn, pawn.style.beardDef))
			{
				pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);
				flag = true;
			}
			if (addedOrRemovedGene.fur != null)
			{
				if (SelectGene((Gene g) => g.def.fur != null, out var chosen7))
				{
					pawn.story.furDef = chosen7.def.fur;
					OverrideAllConflicting(chosen7);
				}
				else
				{
					pawn.story.furDef = null;
				}
				flag = true;
			}
			if (addedOrRemovedGene.RandomChosen && SelectGene((Gene g) => g.Active && g.def.ConflictsWith(addedOrRemovedGene), out var chosen8))
			{
				OverrideAllConflicting(chosen8);
				flag = true;
			}
			RecacheNeeds();
			if (addedOrRemovedGene.soundCall != null)
			{
				PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
			}
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
			pawn.health.hediffSet.DirtyCache();
			cachedDamageFactors.Clear();
			cachedAddictionChanceFactors.Clear();
			cachedGenesAffectAge = null;
			cachedHasCustomXenotype = null;
			cachedCustomXenotype = null;
			cachedTattoosVisible = null;
			cachedWaterCellCost = null;
			hasCachedWaterCost = false;
			pawn.skills?.DirtyAptitudes();
			pawn.Notify_DisabledWorkTypesChanged();
		}
		if (flag)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		bool SelectGene(Predicate<Gene> validator, out Gene reference)
		{
			for (int i = 0; i < genes.Count; i++)
			{
				if ((genes[i].Active || genes[i].Overridden) && validator(genes[i]))
				{
					genes[i].OverrideBy(null);
					tmpGenes.Add(genes[i]);
				}
			}
			if (tmpGenes.Where(IsXenogene).TryRandomElement(out reference))
			{
				tmpGenes.Clear();
				return true;
			}
			if (tmpGenes.TryRandomElement(out reference))
			{
				tmpGenes.Clear();
				return true;
			}
			tmpGenes.Clear();
			reference = null;
			return false;
		}
	}

	private void EnsureCorrectSkinColorOverride()
	{
		if (!ModLister.BiotechInstalled)
		{
			return;
		}
		bool flag = false;
		Color? skinColorOverride = null;
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			Gene gene = genesListForReading[i];
			if (gene.Active)
			{
				if (gene.def.skinIsHairColor)
				{
					flag = true;
				}
				else if (gene.def.skinColorOverride.HasValue)
				{
					skinColorOverride = gene.def.skinColorOverride;
				}
			}
		}
		if (flag)
		{
			pawn.story.skinColorOverride = pawn.story.HairColor;
		}
		else
		{
			pawn.story.skinColorOverride = skinColorOverride;
		}
	}

	private void OverrideAllConflicting(Gene gene)
	{
		if (!ModLister.BiotechInstalled || !gene.def.RandomChosen)
		{
			return;
		}
		gene.OverrideBy(null);
		foreach (Gene item in GenesListForReading)
		{
			if (item != gene && item.def.ConflictsWith(gene.def))
			{
				item.OverrideBy(gene);
			}
		}
	}

	public void Notify_NewColony()
	{
		foreach (Gene item in GenesListForReading)
		{
			item.Notify_NewColony();
		}
	}

	public void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		List<Gene> genesListForReading = GenesListForReading;
		for (int num = genesListForReading.Count - 1; num >= 0; num--)
		{
			genesListForReading[num].Notify_PawnDied(dinfo, culprit);
		}
	}

	public bool StyleItemAllowed(StyleItemDef styleItem)
	{
		if (!ModLister.BiotechInstalled)
		{
			return true;
		}
		bool flag = styleItem is HairDef;
		bool flag2 = styleItem is BeardDef;
		if (!flag && !flag2)
		{
			return true;
		}
		if (flag2 && !pawn.style.CanWantBeard)
		{
			return styleItem == BeardDefOf.NoBeard;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (!genesListForReading[i].Active)
			{
				continue;
			}
			if (flag)
			{
				if (genesListForReading[i].def.forcedHair != null && genesListForReading[i].def.forcedHair != styleItem)
				{
					return false;
				}
				if (genesListForReading[i].def.hairTagFilter != null && !genesListForReading[i].def.hairTagFilter.Allows(styleItem.styleTags))
				{
					return false;
				}
			}
			else if (flag2 && genesListForReading[i].def.beardTagFilter != null && !genesListForReading[i].def.beardTagFilter.Allows(styleItem.styleTags))
			{
				return false;
			}
		}
		return true;
	}

	public float FactorForDamage(DamageInfo dinfo)
	{
		if (!ModLister.BiotechInstalled || dinfo.Def == null || GenesListForReading.NullOrEmpty())
		{
			return 1f;
		}
		if (cachedDamageFactors.TryGetValue(dinfo.Def, out var value))
		{
			return value;
		}
		float num = 1f;
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			Gene gene = genesListForReading[i];
			if (!gene.Active || gene.def.damageFactors.NullOrEmpty())
			{
				continue;
			}
			for (int j = 0; j < gene.def.damageFactors.Count; j++)
			{
				if (gene.def.damageFactors[j].damageDef == dinfo.Def)
				{
					num *= gene.def.damageFactors[j].factor;
				}
			}
		}
		cachedDamageFactors.Add(dinfo.Def, num);
		return num;
	}

	public float AddictionChanceFactor(ChemicalDef chemical)
	{
		if (!ModLister.BiotechInstalled || GenesListForReading.NullOrEmpty())
		{
			return 1f;
		}
		if (cachedAddictionChanceFactors.TryGetValue(chemical, out var value))
		{
			return value;
		}
		float num = 1f;
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i].def.chemical == chemical)
			{
				num *= genesListForReading[i].def.addictionChanceFactor;
			}
		}
		cachedAddictionChanceFactors.Add(chemical, num);
		return num;
	}

	public bool HediffGiversCanGive(HediffDef hediff)
	{
		if (!ModLister.BiotechInstalled)
		{
			return true;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i].def.hediffGiversCannotGive != null && genesListForReading[i].def.hediffGiversCannotGive.Contains(hediff))
			{
				return false;
			}
		}
		return true;
	}

	public SoundDef GetSoundOverrideFromGenes(Func<GeneDef, SoundDef> getter, SoundDef oldSound)
	{
		if (!ModLister.BiotechInstalled)
		{
			return oldSound;
		}
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active)
			{
				SoundDef soundDef = getter(genesListForReading[i].def);
				if (soundDef != null)
				{
					return soundDef;
				}
			}
		}
		return oldSound;
	}

	public void InitializeGenesFromOldSave(float melanin)
	{
		if (GetMelaninGene() == null)
		{
			AddGene(PawnSkinColors.GetSkinColorGene(melanin), xenogene: false);
		}
		if (GetHairColorGene() == null)
		{
			GeneDef geneDef = PawnHairColors.ClosestHairColorGene(pawn.story.HairColor, pawn.story.SkinColorBase);
			if (geneDef != null)
			{
				AddGene(geneDef, xenogene: false);
			}
		}
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (!ModLister.BiotechInstalled)
		{
			yield break;
		}
		foreach (Gene item in GenesListForReading)
		{
			if (!item.Active)
			{
				continue;
			}
			IEnumerable<StatDrawEntry> enumerable = item.SpecialDisplayStats();
			if (enumerable == null)
			{
				continue;
			}
			foreach (StatDrawEntry item2 in enumerable)
			{
				yield return item2;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref xenogenes, "xenogenes", LookMode.Deep);
		Scribe_Collections.Look(ref endogenes, "endogenes", LookMode.Deep);
		Scribe_Defs.Look(ref xenotype, "xenotype");
		Scribe_Values.Look(ref xenotypeName, "xenotypeName");
		Scribe_Values.Look(ref hybrid, "hybrid", defaultValue: false);
		Scribe_Defs.Look(ref iconDef, "iconDef");
		if (Scribe.mode == LoadSaveMode.LoadingVars && ((xenogenes != null && xenogenes.RemoveAll((Gene x) => x == null || x.def == null) > 0) || (endogenes != null && endogenes.RemoveAll((Gene x) => x == null || x.def == null) > 0)))
		{
			Log.Error("Removed null gene(s)");
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (xenotype == null && ModsConfig.BiotechActive)
			{
				xenotype = XenotypeDefOf.Baseliner;
			}
			if (xenogenes == null)
			{
				xenogenes = new List<Gene>();
			}
			if (endogenes == null)
			{
				endogenes = new List<Gene>();
			}
			RecacheNeeds();
		}
	}

	private void RecacheNeeds()
	{
		cachedDisabledNeeds?.Clear();
		cachedEnabledNeeds?.Clear();
		List<Gene> genesListForReading = GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			Gene gene = genesListForReading[i];
			if (gene.Active && !gene.def.enablesNeeds.NullOrEmpty())
			{
				for (int j = 0; j < gene.def.enablesNeeds.Count; j++)
				{
					NeedDef key = gene.def.enablesNeeds[j];
					if (cachedEnabledNeeds == null)
					{
						cachedEnabledNeeds = new Dictionary<NeedDef, Gene>();
					}
					cachedEnabledNeeds[key] = gene;
				}
			}
			if (!gene.Active || gene.def.disablesNeeds.NullOrEmpty())
			{
				continue;
			}
			for (int k = 0; k < gene.def.disablesNeeds.Count; k++)
			{
				NeedDef key2 = gene.def.disablesNeeds[k];
				if (cachedDisabledNeeds == null)
				{
					cachedDisabledNeeds = new Dictionary<NeedDef, Gene>();
				}
				cachedDisabledNeeds[key2] = gene;
			}
		}
	}

	public void Debug_AddAllGenes(bool xenogene)
	{
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		ClearXenogenes();
		cachedGenes = null;
		foreach (GeneDef allDef in DefDatabase<GeneDef>.AllDefs)
		{
			if (allDef.endogeneCategory != EndogeneCategory.Melanin && allDef.endogeneCategory != EndogeneCategory.HairColor)
			{
				Gene gene = GeneMaker.MakeGene(allDef, pawn);
				if (xenogene)
				{
					xenogenes.Add(gene);
				}
				else
				{
					endogenes.Add(gene);
				}
				Notify_GenesChanged(allDef);
				gene.PostAdd();
			}
		}
		CheckForOverrides();
	}
}
