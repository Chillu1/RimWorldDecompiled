using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Grammar;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class GeneUtility
{
	private struct GeneCount
	{
		public int nonArchiteCount;

		public int architeCount;

		public float chance;
	}

	private static List<GeneDef> cachedGeneDefsInOrder = null;

	private const int MaxTriesToGenerateSingleXenotypeName = 150;

	private const string BiostatsTexPath = "UI/Icons/Biostats/";

	private const string PrefixKeyword = "genePrefix";

	private const string SuffixKeyword = "geneSuffix";

	private const string WholeNameKeyword = "wholeName";

	private const string RootKeyword = "r_name";

	public static readonly CachedTexture UniqueXenotypeTex = new CachedTexture("UI/Icons/Xenotypes/Other");

	public static readonly CachedTexture GCXTex = new CachedTexture("UI/Icons/Biostats/Complexity");

	public static readonly CachedTexture METTex = new CachedTexture("UI/Icons/Biostats/Metabolism");

	public static readonly CachedTexture ARCTex = new CachedTexture("UI/Icons/Biostats/ArchiteCapsuleRequired");

	private static readonly GeneCount[] GeneCountProbabilities = new GeneCount[6]
	{
		new GeneCount
		{
			nonArchiteCount = 1,
			chance = 0.66f
		},
		new GeneCount
		{
			nonArchiteCount = 2,
			chance = 0.2f
		},
		new GeneCount
		{
			nonArchiteCount = 3,
			chance = 0.05f
		},
		new GeneCount
		{
			nonArchiteCount = 4,
			chance = 0.02f
		},
		new GeneCount
		{
			nonArchiteCount = 1,
			architeCount = 1,
			chance = 0.05f
		},
		new GeneCount
		{
			nonArchiteCount = 2,
			architeCount = 1,
			chance = 0.02f
		}
	};

	public static readonly Color GCXColor = new ColorInt(105, 213, 240).ToColor;

	public static readonly Color METColor = new ColorInt(247, 72, 160).ToColor;

	public static readonly Color ARCColor = new ColorInt(214, 219, 61).ToColor;

	private static List<GeneSymbolPack> tmpSymbolPacks = new List<GeneSymbolPack>();

	private static List<GeneDef> tmpGenes = new List<GeneDef>();

	private static HashSet<GeneDefWithType> tmpOverriddenGenes = new HashSet<GeneDefWithType>();

	private static List<GeneDefWithType> tmpGeneDefsWithType = new List<GeneDefWithType>();

	private static List<GeneDefWithType> tmpTypedGenes = new List<GeneDefWithType>();

	public static List<GeneDef> GenesInOrder
	{
		get
		{
			if (cachedGeneDefsInOrder == null)
			{
				cachedGeneDefsInOrder = new List<GeneDef>();
				foreach (GeneDef allDef in DefDatabase<GeneDef>.AllDefs)
				{
					if (allDef.endogeneCategory != EndogeneCategory.Melanin)
					{
						cachedGeneDefsInOrder.Add(allDef);
					}
				}
				cachedGeneDefsInOrder.SortBy((GeneDef x) => 0f - x.displayCategory.displayPriorityInXenotype, (GeneDef x) => x.displayCategory.label, (GeneDef x) => x.displayOrderInCategory);
			}
			return cachedGeneDefsInOrder;
		}
	}

	public static GeneSet GenerateGeneSet(int? seed = null)
	{
		if (!ModLister.CheckBiotech("geneset generation"))
		{
			return null;
		}
		GeneSet geneSet = new GeneSet();
		if (seed.HasValue)
		{
			Rand.PushState(seed.Value);
		}
		GeneCount geneCount = GeneCountProbabilities.RandomElementByWeight((GeneCount x) => x.chance);
		for (int num = 0; num < geneCount.architeCount; num++)
		{
			if (DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.biostatArc > 0 && geneSet.CanAddGeneDuringGeneration(x)).TryRandomElementByWeight((GeneDef x) => x.selectionWeight, out var result))
			{
				geneSet.AddGene(result);
			}
		}
		for (int num2 = 0; num2 < geneCount.nonArchiteCount; num2++)
		{
			if (DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.biostatArc == 0 && geneSet.CanAddGeneDuringGeneration(x)).TryRandomElementByWeight((GeneDef x) => x.selectionWeight, out var result2))
			{
				geneSet.AddGene(result2);
			}
		}
		geneSet.GenerateName();
		if (seed.HasValue)
		{
			Rand.PopState();
		}
		if (geneSet.Empty)
		{
			Log.Error("Generated gene pack with no genes.");
		}
		geneSet.SortGenes();
		return geneSet;
	}

	public static bool PawnWouldDieFromReimplanting(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		return pawn.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating);
	}

	public static void ReimplantXenogerm(Pawn caster, Pawn recipient)
	{
		if (!ModLister.CheckBiotech("xenogerm reimplantation"))
		{
			return;
		}
		QuestUtility.SendQuestTargetSignals(caster.questTags, "XenogermReimplanted", caster.Named("SUBJECT"));
		recipient.genes.SetXenotype(caster.genes.Xenotype);
		recipient.genes.xenotypeName = caster.genes.xenotypeName;
		recipient.genes.xenotypeName = caster.genes.xenotypeName;
		recipient.genes.iconDef = caster.genes.iconDef;
		recipient.genes.ClearXenogenes();
		foreach (Gene xenogene in caster.genes.Xenogenes)
		{
			recipient.genes.AddGene(xenogene.def, xenogene: true);
		}
		if (!caster.genes.Xenotype.soundDefOnImplant.NullOrUndefined())
		{
			caster.genes.Xenotype.soundDefOnImplant.PlayOneShot(SoundInfo.InMap(recipient));
		}
		recipient.health.AddHediff(HediffDefOf.XenogerminationComa);
		ExtractXenogerm(caster);
		UpdateXenogermReplication(recipient);
	}

	public static void ExtractXenogerm(Pawn pawn, int overrideDurationTicks = -1)
	{
		if (ModLister.CheckBiotech("xenogerm extraction"))
		{
			pawn.health.AddHediff(HediffDefOf.XenogermLossShock);
			if (PawnWouldDieFromReimplanting(pawn))
			{
				pawn.genes.SetXenotype(XenotypeDefOf.Baseliner);
			}
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.XenogermReplicating, pawn);
			if (overrideDurationTicks > 0)
			{
				hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = overrideDurationTicks;
			}
			pawn.health.AddHediff(hediff);
		}
	}

	public static void ImplantXenogermItem(Pawn pawn, Xenogerm xenogerm)
	{
		if (!ModLister.CheckBiotech("xenogerm implantation"))
		{
			return;
		}
		UpdateXenogermReplication(pawn);
		if (xenogerm.GeneSet == null || pawn.genes == null)
		{
			return;
		}
		pawn.genes.SetXenotype(XenotypeDefOf.Baseliner);
		pawn.genes.xenotypeName = xenogerm.xenotypeName;
		pawn.genes.iconDef = xenogerm.iconDef;
		foreach (GeneDef item in xenogerm.GeneSet.GenesListForReading)
		{
			pawn.genes.AddGene(item, xenogene: true);
		}
	}

	public static void UpdateXenogermReplication(Pawn pawn)
	{
		if (ModsConfig.BiotechActive)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
			pawn.health.AddHediff(HediffDefOf.XenogermReplicating);
		}
	}

	public static bool IsBloodfeeder(this Pawn pawn)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return false;
		}
		return pawn.genes.HasActiveGene(GeneDefOf.Bloodfeeder);
	}

	public static bool CanDeathrest(this Pawn pawn)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return false;
		}
		return pawn.genes.GetFirstGeneOfType<Gene_Deathrest>() != null;
	}

	public static bool SterileGenes(this Pawn pawn)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return false;
		}
		List<Gene> genesListForReading = pawn.genes.GenesListForReading;
		for (int i = 0; i < genesListForReading.Count; i++)
		{
			if (genesListForReading[i].Active && genesListForReading[i].def.sterilize)
			{
				return true;
			}
		}
		return false;
	}

	public static BodyTypeDef ToBodyType(this GeneticBodyType bodyType, Pawn pawn)
	{
		switch (bodyType)
		{
		case GeneticBodyType.Standard:
			if (pawn.gender != Gender.Female)
			{
				return BodyTypeDefOf.Male;
			}
			return BodyTypeDefOf.Female;
		case GeneticBodyType.Fat:
			return BodyTypeDefOf.Fat;
		case GeneticBodyType.Hulk:
			return BodyTypeDefOf.Hulk;
		case GeneticBodyType.Thin:
			return BodyTypeDefOf.Thin;
		default:
			Log.Error("Undefined bodyType");
			return null;
		}
	}

	public static void OffsetHemogen(Pawn pawn, float offset, bool applyStatFactor = true)
	{
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		if (offset > 0f && applyStatFactor)
		{
			offset *= pawn.GetStatValue(StatDefOf.HemogenGainFactor);
		}
		Gene_HemogenDrain gene_HemogenDrain = pawn.genes?.GetFirstGeneOfType<Gene_HemogenDrain>();
		if (gene_HemogenDrain != null)
		{
			GeneResourceDrainUtility.OffsetResource(gene_HemogenDrain, offset);
			return;
		}
		Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
		if (gene_Hemogen != null)
		{
			gene_Hemogen.Value += offset;
		}
	}

	public static string GenerateXenotypeNameFromGenes(List<GeneDef> genes)
	{
		tmpSymbolPacks.Clear();
		string text = string.Empty;
		if (genes.Count == 0)
		{
			return text;
		}
		GrammarRequest request = new GrammarRequest
		{
			Includes = { RulePackDefOf.NamerXenotype }
		};
		for (int i = 0; i < genes.Count; i++)
		{
			if (genes[i].symbolPack != null)
			{
				tmpSymbolPacks.Add(genes[i].symbolPack);
			}
		}
		for (int j = 0; j < 150; j++)
		{
			tmpSymbolPacks.Where((GeneSymbolPack x) => !x.wholeNameSymbols.NullOrEmpty()).TryRandomElementByWeight((GeneSymbolPack x) => x.wholeNameSymbols.Sum((GeneSymbolPack.WeightedSymbol y) => y.weight), out var result);
			tmpSymbolPacks.Where((GeneSymbolPack x) => !x.prefixSymbols.NullOrEmpty()).TryRandomElementByWeight((GeneSymbolPack x) => x.prefixSymbols.Sum((GeneSymbolPack.WeightedSymbol y) => y.weight), out var result2);
			tmpSymbolPacks.Where((GeneSymbolPack x) => !x.suffixSymbols.NullOrEmpty()).Except(result2).TryRandomElementByWeight((GeneSymbolPack x) => x.suffixSymbols.Sum((GeneSymbolPack.WeightedSymbol y) => y.weight), out var result3);
			GeneSymbolPack.WeightedSymbol prefix = null;
			GeneSymbolPack.WeightedSymbol suffix = null;
			GeneSymbolPack.WeightedSymbol result4 = null;
			if (result != null && result.wholeNameSymbols.TryRandomElementByWeight((GeneSymbolPack.WeightedSymbol x) => SymbolWeight(x), out result4))
			{
				request.Rules.Add(new Rule_String("wholeName", result4.symbol));
			}
			if (result2 != null && result2.prefixSymbols.TryRandomElementByWeight((GeneSymbolPack.WeightedSymbol x) => SymbolWeight(x), out prefix))
			{
				request.Rules.Add(new Rule_String("genePrefix", prefix.symbol));
			}
			if (result3 != null && result3.suffixSymbols.TryRandomElementByWeight((GeneSymbolPack.WeightedSymbol x) => SymbolWeight(x), out suffix))
			{
				request.Rules.Add(new Rule_String("geneSuffix", suffix.symbol));
			}
			string result5 = GrammarResolver.Resolve("r_name", request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false);
			if (!NameUseChecker.XenotypeNameIsUsed(text))
			{
				return result5;
			}
			float SymbolWeight(GeneSymbolPack.WeightedSymbol sym)
			{
				if (prefix != null && sym.symbol == prefix.symbol)
				{
					return 0f;
				}
				if (suffix != null && sym.symbol == suffix.symbol)
				{
					return 0f;
				}
				return sym.weight;
			}
		}
		Log.Error("Failed to generate a unique xenotype name.");
		if (text.NullOrEmpty())
		{
			text = "ERR";
		}
		return text;
	}

	public static void SatisfyChemicalGenes(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return;
		}
		foreach (Gene item in pawn.genes.GenesListForReading)
		{
			if (item is Gene_ChemicalDependency gene_ChemicalDependency)
			{
				gene_ChemicalDependency.Reset();
			}
		}
	}

	public static void SortGeneDefs(this List<GeneDef> geneDefs)
	{
		geneDefs.SortBy((GeneDef x) => 0f - x.displayCategory.displayPriorityInXenotype, (GeneDef x) => x.displayOrderInCategory, (GeneDef y) => y.label);
	}

	public static void SortGenes(this List<Gene> genes)
	{
		genes.SortBy((Gene x) => !x.Active, (Gene x) => 0f - x.def.displayCategory.displayPriorityInXenotype, (Gene y) => y.def.displayOrderInCategory);
	}

	public static void SortGenepacks(this List<Genepack> genepacks)
	{
		genepacks.SortBy((Genepack x) => 0f - x.GeneSet.GenesListForReading[0].displayCategory.displayPriorityInXenotype, (Genepack x) => x.GeneSet.GenesListForReading[0].displayCategory.label, (Genepack x) => x.GeneSet.GenesListForReading[0].displayOrderInCategory);
	}

	public static List<GeneDef> NonOverriddenGenes(this List<GeneDefWithType> geneDefWithTypes)
	{
		tmpGenes.Clear();
		tmpOverriddenGenes.Clear();
		if (!ModsConfig.BiotechActive)
		{
			return tmpGenes;
		}
		foreach (GeneDefWithType geneDefWithType in geneDefWithTypes)
		{
			tmpGenes.Add(geneDefWithType.geneDef);
		}
		for (int i = 0; i < geneDefWithTypes.Count; i++)
		{
			if (geneDefWithTypes[i].RandomChosen)
			{
				continue;
			}
			for (int j = i + 1; j < geneDefWithTypes.Count; j++)
			{
				if (!geneDefWithTypes[j].RandomChosen && geneDefWithTypes[i].ConflictsWith(geneDefWithTypes[j]))
				{
					if (geneDefWithTypes[i].Overrides(geneDefWithTypes[j]))
					{
						tmpOverriddenGenes.Add(geneDefWithTypes[j]);
					}
					else
					{
						tmpOverriddenGenes.Add(geneDefWithTypes[i]);
					}
				}
			}
		}
		foreach (GeneDefWithType tmpOverriddenGene in tmpOverriddenGenes)
		{
			tmpGenes.Remove(tmpOverriddenGene.geneDef);
		}
		tmpOverriddenGenes.Clear();
		return tmpGenes;
	}

	public static List<GeneDef> NonOverriddenGenes(this List<GeneDef> geneDefs, bool xenogene)
	{
		tmpGeneDefsWithType.Clear();
		foreach (GeneDef geneDef in geneDefs)
		{
			tmpGeneDefsWithType.Add(new GeneDefWithType(geneDef, xenogene));
		}
		return tmpGeneDefsWithType.NonOverriddenGenes();
	}

	public static bool Overrides(this GeneDef gene, GeneDef other, bool isXenogene, bool otherIsXenogene)
	{
		if (gene.RandomChosen || other.RandomChosen || !gene.ConflictsWith(other))
		{
			return false;
		}
		if (isXenogene == otherIsXenogene)
		{
			return GenesInOrder.IndexOf(gene) <= GenesInOrder.IndexOf(other);
		}
		if (isXenogene)
		{
			return !otherIsXenogene;
		}
		return false;
	}

	public static bool IsBaseliner(this Pawn pawn)
	{
		if (ModsConfig.BiotechActive && pawn.genes?.Xenotype != null)
		{
			if (pawn.genes.Xenotype == XenotypeDefOf.Baseliner)
			{
				return pawn.genes.CustomXenotype == null;
			}
			return false;
		}
		return true;
	}

	public static bool SameHeritableXenotype(Pawn pawn1, Pawn pawn2)
	{
		if (pawn1?.genes == null || pawn2?.genes == null)
		{
			return false;
		}
		if (pawn1.genes.UniqueXenotype || pawn2.genes.UniqueXenotype)
		{
			for (int i = 0; i < pawn1.genes.Endogenes.Count; i++)
			{
				Gene gene = pawn1.genes.Endogenes[i];
				if (gene.def != GeneDefOf.Inbred && gene.def.endogeneCategory != EndogeneCategory.Melanin && gene.def.endogeneCategory != EndogeneCategory.HairColor && !pawn2.genes.Endogenes.Any((Gene x) => x.def == gene.def))
				{
					return false;
				}
			}
			for (int num = 0; num < pawn2.genes.Endogenes.Count; num++)
			{
				Gene gene2 = pawn2.genes.Endogenes[num];
				if (gene2.def != GeneDefOf.Inbred && gene2.def.endogeneCategory != EndogeneCategory.Melanin && gene2.def.endogeneCategory != EndogeneCategory.HairColor && !pawn1.genes.Endogenes.Any((Gene x) => x.def == gene2.def))
				{
					return false;
				}
			}
			return true;
		}
		if (pawn1.genes.Xenotype == pawn2.genes.Xenotype)
		{
			return pawn1.genes.Xenotype.inheritable;
		}
		return false;
	}

	public static bool SameXenotype(Pawn pawn1, Pawn pawn2)
	{
		if (pawn1?.genes == null || pawn2?.genes == null)
		{
			return false;
		}
		if (pawn1.genes.UniqueXenotype || pawn2.genes.UniqueXenotype)
		{
			int i;
			for (i = 0; i < pawn1.genes.Xenogenes.Count; i++)
			{
				if (!pawn2.genes.Xenogenes.Any((Gene x) => x.def == pawn1.genes.Xenogenes[i].def))
				{
					return false;
				}
			}
			int i2;
			for (i2 = 0; i2 < pawn2.genes.Xenogenes.Count; i2++)
			{
				if (!pawn1.genes.Xenogenes.Any((Gene x) => x.def == pawn2.genes.Xenogenes[i2].def))
				{
					return false;
				}
			}
			CustomXenotype customXenotype = pawn1.genes.CustomXenotype;
			CustomXenotype customXenotype2 = pawn2.genes.CustomXenotype;
			if (customXenotype != customXenotype2)
			{
				return false;
			}
			return true;
		}
		return pawn1.genes.Xenotype == pawn2.genes.Xenotype;
	}

	public static bool PawnIsCustomXenotype(Pawn pawn, CustomXenotype custom)
	{
		if (!ModsConfig.BiotechActive || pawn.genes == null)
		{
			return false;
		}
		List<Gene> list = (custom.inheritable ? pawn.genes.Endogenes : pawn.genes.Xenogenes);
		int i;
		for (i = 0; i < custom.genes.Count; i++)
		{
			if (custom.genes[i].passOnDirectly && !list.Any((Gene x) => x.def == custom.genes[i]))
			{
				return false;
			}
		}
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num].def.passOnDirectly && !custom.genes.Contains(list[num].def))
			{
				return false;
			}
		}
		return true;
	}

	public static int MetabolismAfterImplanting(Pawn pawn, GeneSet geneSet)
	{
		tmpTypedGenes.Clear();
		foreach (Gene endogene in pawn.genes.Endogenes)
		{
			tmpTypedGenes.Add(new GeneDefWithType(endogene.def, xenogene: false));
		}
		foreach (GeneDef item in geneSet.GenesListForReading)
		{
			tmpTypedGenes.Add(new GeneDefWithType(item, xenogene: true));
		}
		int result = tmpTypedGenes.NonOverriddenGenes().Sum((GeneDef x) => x.biostatMet);
		tmpTypedGenes.Clear();
		return result;
	}

	public static void GiveReimplantJob(Pawn pawn, Pawn targPawn)
	{
		pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.AbsorbXenogerm, targPawn), JobTag.Misc);
		if (targPawn.HomeFaction != null && !targPawn.HomeFaction.Hidden && targPawn.HomeFaction != pawn.Faction && !targPawn.HomeFaction.HostileTo(Faction.OfPlayer))
		{
			Messages.Message("MessageAbsorbingXenogermWillAngerFaction".Translate(targPawn.HomeFaction, targPawn.Named("PAWN")), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
	}

	public static bool CanAbsorbXenogerm(Pawn pawn)
	{
		if (pawn?.genes == null)
		{
			return false;
		}
		if (!pawn.genes.HasActiveGene(GeneDefOf.XenogermReimplanter))
		{
			return false;
		}
		if (pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure)
		{
			return true;
		}
		if (!pawn.Downed)
		{
			return false;
		}
		if (!pawn.genes.Xenogenes.Any())
		{
			return false;
		}
		return true;
	}

	public static int AddedAndImplantedPartsWithXenogenesCount(Pawn pawn)
	{
		int num = pawn.health.hediffSet.CountAddedAndImplantedParts();
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.Xenogenes.Any())
		{
			num++;
		}
		return num;
	}
}
