using System.Collections.Generic;
using System.Linq;

namespace Verse;

public class HediffCompProperties_VerbGiver : HediffCompProperties
{
	public List<VerbProperties> verbs;

	public List<Tool> tools;

	public ImplementOwnerTypeDef ownerTypeOverride;

	public HediffCompProperties_VerbGiver()
	{
		compClass = typeof(HediffComp_VerbGiver);
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (tools != null)
		{
			for (int i = 0; i < tools.Count; i++)
			{
				tools[i].id = i.ToString();
			}
		}
	}

	public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (tools == null)
		{
			yield break;
		}
		Tool tool = tools.SelectMany((Tool lhs) => tools.Where((Tool rhs) => lhs != rhs && lhs.id == rhs.id)).FirstOrDefault();
		if (tool != null)
		{
			yield return $"duplicate hediff tool id {tool.id}";
		}
		foreach (Tool tool2 in tools)
		{
			foreach (string item2 in tool2.ConfigErrors())
			{
				yield return item2;
			}
		}
	}
}
