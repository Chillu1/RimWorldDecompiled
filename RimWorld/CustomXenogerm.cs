using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CustomXenogerm : IExposable
	{
		public string name;

		public List<GeneSet> genesets = new List<GeneSet>();

		public XenotypeIconDef iconDef;

		public CustomXenogerm()
		{
		}

		public CustomXenogerm(string name, XenotypeIconDef iconDef, IEnumerable<Genepack> genepacks)
		{
			genesets.AddRange(genepacks.Select((Genepack gp) => gp.GeneSet.Copy()));
			this.iconDef = iconDef;
			this.name = name;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref name, "name");
			Scribe_Collections.Look(ref genesets, "genesets", LookMode.Deep);
			Scribe_Defs.Look(ref iconDef, "iconDef");
		}
	}
}
