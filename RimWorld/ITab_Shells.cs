namespace RimWorld;

public class ITab_Shells : ITab_Storage
{
	protected override IStoreSettingsParent SelStoreSettingsParent
	{
		get
		{
			IStoreSettingsParent selStoreSettingsParent = base.SelStoreSettingsParent;
			if (selStoreSettingsParent != null)
			{
				return selStoreSettingsParent;
			}
			if (base.SelObject is Building_TurretGun building_TurretGun)
			{
				return GetThingOrThingCompStoreSettingsParent(building_TurretGun.gun);
			}
			return null;
		}
	}

	protected override bool IsPrioritySettingVisible => false;

	public ITab_Shells()
	{
		labelKey = "TabShells";
		tutorTag = "Shells";
	}
}
