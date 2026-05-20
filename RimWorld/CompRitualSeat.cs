using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompRitualSeat : ThingComp
{
	private static StringBuilder tmpStringBuilder = new StringBuilder();

	private static List<Ideo> tmpIdeos = new List<Ideo>();

	public override string CompInspectStringExtra()
	{
		string ideosString = GetIdeosString();
		if (string.IsNullOrEmpty(ideosString))
		{
			return null;
		}
		return "RitualSeatOf".Translate(ideosString.Named("IDEOS")).Resolve();
	}

	private string GetIdeosString(List<Ideo> outIdeos = null)
	{
		tmpStringBuilder.Clear();
		foreach (Ideo item in Find.IdeoManager.IdeosInViewOrder)
		{
			bool flag = false;
			foreach (Precept item2 in item.PreceptsListForReading)
			{
				if (item2 is Precept_RitualSeat precept_RitualSeat && precept_RitualSeat.ThingDef == parent.def)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				outIdeos?.Add(item);
				if (tmpStringBuilder.Length > 0)
				{
					tmpStringBuilder.Append(", ");
				}
				tmpStringBuilder.Append(item.name.ApplyTag(item).Resolve());
			}
		}
		return tmpStringBuilder.ToString();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		tmpIdeos.Clear();
		string ideosString = GetIdeosString(tmpIdeos);
		if (!string.IsNullOrEmpty(ideosString))
		{
			Dialog_InfoCard.Hyperlink[] array = new Dialog_InfoCard.Hyperlink[tmpIdeos.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Dialog_InfoCard.Hyperlink(tmpIdeos[i]);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Thing_RelatedToIdeos_Name".Translate(), ideosString, "Stat_Thing_RelatedToIdeos_Desc".Translate(), 1110, null, array);
		}
	}
}
