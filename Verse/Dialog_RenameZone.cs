using RimWorld;

namespace Verse;

public class Dialog_RenameZone : Dialog_Rename<Zone>
{
	public Dialog_RenameZone(Zone zone)
		: base(zone)
	{
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (renaming.Map.zoneManager.AllZones.Any((Zone z) => z != renaming && z.label == name) || Current.Game.CurrentMap.storageGroups.HasGroupWithName(name))
		{
			return "NameIsInUse".Translate();
		}
		return true;
	}

	protected override void OnRenamed(string name)
	{
		Messages.Message("ZoneGainsName".Translate(name), MessageTypeDefOf.TaskCompletion, historical: false);
	}
}
