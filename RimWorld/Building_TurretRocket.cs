using UnityEngine;

namespace RimWorld;

public class Building_TurretRocket : Building_TurretGun
{
	protected override bool CanSetForcedTarget => true;

	protected override bool HideForceTargetGizmo => true;

	public override Material TurretTopMaterial
	{
		get
		{
			if (refuelableComp.IsFull)
			{
				return def.building.turretGunDef.building.turretTopLoadedMat;
			}
			return def.building.turretTopMat;
		}
	}
}
