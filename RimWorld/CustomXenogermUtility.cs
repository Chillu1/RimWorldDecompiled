using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class CustomXenogermUtility
	{
		private static List<Genepack> tmpGenepacks = new List<Genepack>();

		public static AcceptanceReport SaveXenogermTemplate(string xenoTypeName, XenotypeIconDef iconDef, List<Genepack> selectedGenepacks)
		{
			if (selectedGenepacks.NullOrEmpty())
			{
				return "CannotSaveXenogermTemplate".Translate("GenepackPlural".Translate());
			}
			if (xenoTypeName.NullOrEmpty())
			{
				return "CannotSaveXenogermTemplate".Translate("XenotypeName".Translate());
			}
			CustomXenogerm customXenogerm = new CustomXenogerm(xenoTypeName, iconDef, selectedGenepacks);
			Find.CustomXenogermDatabase.Add(customXenogerm);
			Messages.Message("XenogermTemplateSaved".Translate(xenoTypeName.Named("NAME")), MessageTypeDefOf.PositiveEvent, historical: false);
			return true;
		}

		public static IEnumerable<Genepack> GetMatchingGenepacks(IEnumerable<GeneSet> templateGenesets, IEnumerable<Genepack> genepacks)
		{
			tmpGenepacks.Clear();
			tmpGenepacks.AddRange(genepacks);
			foreach (GeneSet templateGeneset in templateGenesets)
			{
				Genepack genePack = tmpGenepacks.FirstOrDefault((Genepack gp) => gp.GeneSet.Matches(templateGeneset));
				if (genePack != null)
				{
					yield return genePack;
					tmpGenepacks.Remove(genePack);
				}
			}
			tmpGenepacks.Clear();
		}
	}
}
