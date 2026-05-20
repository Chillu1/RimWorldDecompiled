using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CustomXenotype : IExposable
{
	public string fileName;

	public string name;

	public List<GeneDef> genes = new List<GeneDef>();

	public XenotypeIconDef iconDef;

	public bool inheritable;

	public XenotypeIconDef IconDef => iconDef ?? XenotypeIconDefOf.Basic;

	public void ExposeData()
	{
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref inheritable, "inheritable", defaultValue: false);
		Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
		Scribe_Defs.Look(ref iconDef, "iconDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && genes.RemoveAll((GeneDef x) => x == null) > 0)
		{
			Log.Error("Removed null genes");
		}
	}
}
