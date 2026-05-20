using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_Genes : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ModsConfig.BiotechActive && req.HasThing && req.Thing is Genepack { GeneSet: not null } genepack)
		{
			val *= NumGenesFactor(genepack.GeneSet);
			val *= ArchiteCostFactor(genepack.GeneSet);
			val *= GenesFactorsDefined(genepack.GeneSet);
		}
	}

	private float NumGenesFactor(GeneSet geneSet)
	{
		return Mathf.Max(3.5f - 0.5f * (float)geneSet.GenesListForReading.Count, 0.5f);
	}

	private float ArchiteCostFactor(GeneSet geneSet)
	{
		return 1f + 3f * (float)geneSet.ArchitesTotal;
	}

	private float GenesFactorsDefined(GeneSet geneSet)
	{
		float num = 1f;
		for (int i = 0; i < geneSet.GenesListForReading.Count; i++)
		{
			num *= geneSet.GenesListForReading[i].marketValueFactor;
		}
		return num;
	}

	public override string ExplanationPart(StatRequest req)
	{
		string text = null;
		if (!ModsConfig.BiotechActive)
		{
			return text;
		}
		if (!req.HasThing || !(req.Thing is Genepack { GeneSet: not null } genepack))
		{
			return text;
		}
		int count = genepack.GeneSet.GenesListForReading.Count;
		if (count > 0)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += string.Format("{0} ({1}): x{2}", "NumberOfGenes".Translate(), count, NumGenesFactor(genepack.GeneSet).ToStringPercent());
		}
		int architesTotal = genepack.GeneSet.ArchitesTotal;
		if (architesTotal != 0)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += string.Format("{0} ({1}): x{2}", "ArchitesTotal".Translate(), architesTotal, ArchiteCostFactor(genepack.GeneSet).ToStringPercent());
		}
		if (GenesFactorsDefined(genepack.GeneSet) != 1f)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "GenePriceFactors".Translate() + ":";
			for (int i = 0; i < genepack.GeneSet.GenesListForReading.Count; i++)
			{
				GeneDef geneDef = genepack.GeneSet.GenesListForReading[i];
				if (geneDef.marketValueFactor != 1f)
				{
					text += $"\n  - {geneDef.LabelCap} x{geneDef.marketValueFactor.ToStringPercent()}";
				}
			}
		}
		return text.TrimEndNewlines();
	}
}
