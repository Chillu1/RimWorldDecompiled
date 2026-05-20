using RimWorld;

namespace Verse;

public class Dialog_RenameAnimalPen : Dialog_Rename<CompAnimalPenMarker>
{
	private readonly Map map;

	public Dialog_RenameAnimalPen(Map map, CompAnimalPenMarker marker)
		: base(marker)
	{
		this.map = map;
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (name != renaming.label && map.animalPenManager.GetPenNamed(name) != null)
		{
			return "NameIsInUse".Translate();
		}
		return true;
	}
}
