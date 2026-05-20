namespace Verse;

public class Dialog_RenameArea : Dialog_Rename<Area>
{
	public Dialog_RenameArea(Area area)
		: base(area)
	{
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (renaming.Map.areaManager.AllAreas.Any((Area a) => a != renaming && a.Label == name))
		{
			return "NameIsInUse".Translate();
		}
		return true;
	}
}
