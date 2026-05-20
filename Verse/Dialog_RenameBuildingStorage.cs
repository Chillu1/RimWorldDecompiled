namespace Verse;

public class Dialog_RenameBuildingStorage : Dialog_Rename<IRenameable>
{
	public Dialog_RenameBuildingStorage(IRenameable storage)
		: base(storage)
	{
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (Current.Game.CurrentMap.zoneManager.AllZones.Any((Zone z) => z.label == name) || Current.Game.CurrentMap.storageGroups.HasGroupWithName(name))
		{
			return "NameIsInUse".Translate();
		}
		return true;
	}
}
