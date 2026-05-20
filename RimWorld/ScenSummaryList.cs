using System.Text;
using Verse;

namespace RimWorld;

public static class ScenSummaryList
{
	public static string SummaryWithList(Scenario scen, string tag, string intro)
	{
		string text = SummaryList(scen, tag);
		if (!text.NullOrEmpty())
		{
			return "\n" + intro + ":\n" + text;
		}
		return null;
	}

	private static string SummaryList(Scenario scen, string tag)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (ScenPart allPart in scen.AllParts)
		{
			if (allPart.summarized)
			{
				continue;
			}
			foreach (string summaryListEntry in allPart.GetSummaryListEntries(tag))
			{
				if (!flag)
				{
					stringBuilder.Append("\n");
				}
				stringBuilder.Append("   -" + summaryListEntry);
				allPart.summarized = true;
				flag = false;
			}
		}
		return stringBuilder.ToString();
	}
}
