using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class GeneSet : IExposable
{
	private List<GeneDef> genes = new List<GeneDef>();

	private string name;

	[Unsaved(false)]
	private List<GeneDef> cachedNonOverriddenGenes;

	public const string ErrorSlugline = "ERR";

	private List<GeneDefWithType> tmpGeneDefWithTypes = new List<GeneDefWithType>();

	public string Label
	{
		get
		{
			if (name.NullOrEmpty())
			{
				GenerateName();
			}
			return name;
		}
	}

	private string LabelShortAdj
	{
		get
		{
			if (!genes.NullOrEmpty())
			{
				return genes[0].LabelShortAdj;
			}
			return "ERR";
		}
	}

	public List<GeneDef> GenesListForReading => genes;

	public bool Empty => genes.Count == 0;

	public int ComplexityTotal
	{
		get
		{
			RecacheOverridesIfNeeded();
			int num = 0;
			for (int i = 0; i < cachedNonOverriddenGenes.Count; i++)
			{
				num += cachedNonOverriddenGenes[i].biostatCpx;
			}
			return num;
		}
	}

	public int MetabolismTotal
	{
		get
		{
			RecacheOverridesIfNeeded();
			int num = 0;
			for (int i = 0; i < cachedNonOverriddenGenes.Count; i++)
			{
				num += cachedNonOverriddenGenes[i].biostatMet;
			}
			return num;
		}
	}

	public int ArchitesTotal
	{
		get
		{
			RecacheOverridesIfNeeded();
			int num = 0;
			for (int i = 0; i < cachedNonOverriddenGenes.Count; i++)
			{
				num += cachedNonOverriddenGenes[i].biostatArc;
			}
			return num;
		}
	}

	private IEnumerable<DefHyperlink> GeneDefHyperlinks
	{
		get
		{
			for (int i = 0; i < genes.Count; i++)
			{
				yield return new DefHyperlink(genes[i]);
			}
		}
	}

	public void AddGene(GeneDef gene)
	{
		if (!genes.Contains(gene))
		{
			genes.Add(gene);
			DirtyCache();
		}
	}

	public void SetNameDirect(string name)
	{
		this.name = name;
	}

	public bool IsOverridden(GeneDef geneDef)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!genes.Contains(geneDef))
		{
			return false;
		}
		RecacheOverridesIfNeeded();
		return !cachedNonOverriddenGenes.Contains(geneDef);
	}

	public void GenerateName()
	{
		if (ModsConfig.BiotechActive && genes.Any())
		{
			name = GrammarResolver.Resolve("r_name", new GrammarRequest
			{
				Includes = { RulePackDefOf.NamerGenepack },
				Rules = 
				{
					(Rule)new Rule_String("geneWord", LabelShortAdj),
					(Rule)new Rule_String("geneCountMinusOne", (genes.Count - 1).ToString())
				},
				Constants = { 
				{
					"geneCount",
					genes.Count.ToString()
				} }
			}, null, forceLog: false, null, null, null, capitalizeFirstSentence: false);
		}
		else
		{
			name = "ERR";
		}
	}

	public bool CanAddGeneDuringGeneration(GeneDef gene)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (genes.Contains(gene))
		{
			return false;
		}
		if (genes.Count > 0 && !GeneTuning.BiostatRange.Includes(gene.biostatMet + MetabolismTotal))
		{
			return false;
		}
		if (!gene.canGenerateInGeneSet)
		{
			return false;
		}
		if (gene.prerequisite != null && !genes.Contains(gene.prerequisite))
		{
			return false;
		}
		for (int i = 0; i < genes.Count; i++)
		{
			if (gene.ConflictsWith(genes[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void SortGenes()
	{
		genes.SortBy((GeneDef x) => 0f - x.displayCategory.displayPriorityInGenepack);
	}

	public override string ToString()
	{
		return "GeneSet: " + (genes.NullOrEmpty() ? "None" : genes.Select((GeneDef x) => x.LabelCap.ToString()).ToCommaList());
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats(Dialog_InfoCard.Hyperlink? inspectGenesHyperlink = null)
	{
		IEnumerable<Dialog_InfoCard.Hyperlink> enumerable = Dialog_InfoCard.DefsToHyperlinks(GeneDefHyperlinks);
		if (inspectGenesHyperlink.HasValue)
		{
			enumerable = enumerable.Concat(inspectGenesHyperlink.Value);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "Genes".Translate().CapitalizeFirst(), genes.Select((GeneDef x) => x.label).ToCommaList().CapitalizeFirst(), "ContainedGenesDesc".Translate() + ":", 999, null, enumerable);
		int complexityTotal = ComplexityTotal;
		if (complexityTotal != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "ComplexityTotal".Translate(), complexityTotal.ToStringWithSign(), "ComplexityTotalDesc".Translate(), 998);
		}
		int metabolismTotal = MetabolismTotal;
		if (metabolismTotal != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "MetabolismTotal".Translate(), metabolismTotal.ToStringWithSign(), "MetabolismTotalDesc".Translate(), 997);
		}
		int num = genes.Sum((GeneDef x) => x.biostatArc);
		if (num != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "ArchitesRequired".Translate(), num.ToString(), "ArchitesRequiredDesc".Translate(), 995);
		}
	}

	public void Debug_RemoveGene(GeneDef gene)
	{
		if (genes.Remove(gene))
		{
			DirtyCache();
		}
	}

	private void DirtyCache()
	{
		name = null;
		cachedNonOverriddenGenes = null;
	}

	private void RecacheOverridesIfNeeded()
	{
		if (cachedNonOverriddenGenes != null)
		{
			return;
		}
		cachedNonOverriddenGenes = new List<GeneDef>();
		foreach (GeneDef gene in genes)
		{
			tmpGeneDefWithTypes.Add(new GeneDefWithType(gene, xenogene: true));
		}
		cachedNonOverriddenGenes.AddRange(tmpGeneDefWithTypes.NonOverriddenGenes());
		tmpGeneDefWithTypes.Clear();
	}

	public GeneSet Copy()
	{
		GeneSet geneSet = new GeneSet();
		geneSet.name = name;
		geneSet.genes.AddRange(genes);
		geneSet.DirtyCache();
		return geneSet;
	}

	public bool Matches(GeneSet other)
	{
		if (genes.Count != other.genes.Count)
		{
			return false;
		}
		for (int i = 0; i < genes.Count; i++)
		{
			if (!other.genes.Contains(genes[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
		Scribe_Values.Look(ref name, "name");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			genes.RemoveAll((GeneDef x) => x == null);
		}
	}
}
