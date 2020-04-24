namespace Verse
{
	public class Dialog_RenameArea : Dialog_Rename
	{
		private Area area;

		public Dialog_RenameArea(Area area)
		{
			this.area = area;
			curName = area.Label;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (area.Map.areaManager.AllAreas.Any((Area a) => a != area && a.Label == name))
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}

		protected override void SetName(string name)
		{
			area.SetLabel(curName);
		}
	}
}
