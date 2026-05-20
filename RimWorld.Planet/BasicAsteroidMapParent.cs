using Verse;

namespace RimWorld.Planet;

public class BasicAsteroidMapParent : SpaceMapParent
{
	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (preciousResource != null)
		{
			text += "\n" + "TracesOfPreciousResource".Translate(NamedArgumentUtility.Named(preciousResource, "RESOURCE"));
		}
		return text.Trim();
	}
}
