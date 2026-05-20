using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class XenotypeSet
{
	private List<XenotypeChance> xenotypeChances = new List<XenotypeChance>();

	public XenotypeChance this[int index] => xenotypeChances[index];

	public int Count => xenotypeChances.Count;

	public float BaselinerChance
	{
		get
		{
			if (!ModsConfig.BiotechActive || xenotypeChances.NullOrEmpty())
			{
				return 1f;
			}
			float num = 0f;
			for (int i = 0; i < xenotypeChances.Count; i++)
			{
				if (xenotypeChances[i].xenotype != XenotypeDefOf.Baseliner)
				{
					num += xenotypeChances[i].chance;
				}
			}
			return 1f - num;
		}
	}

	public bool Contains(XenotypeDef xenotype)
	{
		if (xenotype == XenotypeDefOf.Baseliner && BaselinerChance > 0f)
		{
			return true;
		}
		for (int i = 0; i < xenotypeChances.Count; i++)
		{
			if (xenotypeChances[i].xenotype == xenotype)
			{
				return true;
			}
		}
		return false;
	}
}
