using Verse;

namespace RimWorld;

public class Landmark : IExposable
{
	public LandmarkDef def;

	public string name;

	public bool isComboLandmark;

	public Landmark()
	{
	}

	public Landmark(LandmarkDef def)
	{
		this.def = def;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref isComboLandmark, "isComboLandmark", defaultValue: false);
	}
}
