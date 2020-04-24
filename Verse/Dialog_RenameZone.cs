using RimWorld;

namespace Verse
{
	public class Dialog_RenameZone : Dialog_Rename
	{
		private Zone zone;

		public Dialog_RenameZone(Zone zone)
		{
			this.zone = zone;
			curName = zone.label;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (zone.Map.zoneManager.AllZones.Any((Zone z) => z != zone && z.label == name))
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}

		protected override void SetName(string name)
		{
			zone.label = curName;
			Messages.Message("ZoneGainsName".Translate(curName), MessageTypeDefOf.TaskCompletion, historical: false);
		}
	}
}
