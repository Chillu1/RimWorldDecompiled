using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_Xenotype : Precept
{
	public XenotypeDef xenotype;

	public CustomXenotype customXenotype;

	private string XenotypeName
	{
		get
		{
			if (xenotype == null)
			{
				return customXenotype?.name;
			}
			return xenotype.label;
		}
	}

	public override string UIInfoFirstLine => XenotypeName.CapitalizeFirst();

	public override string TipLabel => def.issue.LabelCap + ": " + XenotypeName.CapitalizeFirst();

	public override string GenerateNameRaw()
	{
		return name;
	}

	public override void DrawIcon(Rect rect)
	{
		if (xenotype != null)
		{
			GUI.DrawTexture(rect, xenotype.Icon);
		}
		else if (customXenotype != null)
		{
			GUI.DrawTexture(rect, customXenotype.IconDef.Icon);
		}
	}

	public override void CopyTo(Precept other)
	{
		base.CopyTo(other);
		Precept_Xenotype obj = (Precept_Xenotype)other;
		obj.xenotype = xenotype;
		obj.customXenotype = customXenotype;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref xenotype, "xenotype");
		Scribe_Deep.Look(ref customXenotype, "customXenotype");
	}
}
