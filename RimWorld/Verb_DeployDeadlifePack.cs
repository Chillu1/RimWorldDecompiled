using Verse;

namespace RimWorld;

public class Verb_DeployDeadlifePack : Verb
{
	protected override bool TryCastShot()
	{
		return TryDeploy(base.EquipmentSource.TryGetComp<CompApparelReloadable>(), base.EquipmentSource.TryGetComp<CompReleaseGas>());
	}

	public static bool TryDeploy(CompApparelReloadable reloadable, CompReleaseGas releaseGas)
	{
		if (!ModLister.CheckAnomaly("Deadlife packs"))
		{
			return false;
		}
		if (reloadable == null || !reloadable.CanBeUsed(out var _) || releaseGas == null)
		{
			return false;
		}
		reloadable.UsedOnce();
		releaseGas.StartRelease();
		return true;
	}
}
