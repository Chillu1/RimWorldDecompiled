namespace RimWorld;

public class CompFacilityInactiveWhenElectricityDisabled : CompFacility
{
	public override bool CanBeActive
	{
		get
		{
			if (!base.CanBeActive)
			{
				return false;
			}
			return !parent.Map.gameConditionManager.ElectricityDisabled(parent.Map);
		}
	}
}
