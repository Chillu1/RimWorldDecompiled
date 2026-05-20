using Verse;
using Verse.Sound;

namespace RimWorld;

public class Verb_DeployBroadshield : Verb
{
	protected override bool TryCastShot()
	{
		return Deploy(base.ReloadableCompSource);
	}

	public static bool Deploy(CompApparelReloadable comp)
	{
		if (!ModLister.CheckRoyalty("Projectile interceptors"))
		{
			return false;
		}
		if (comp == null || !comp.CanBeUsed(out var _))
		{
			return false;
		}
		Pawn wearer = comp.Wearer;
		Map map = wearer.Map;
		int num = GenRadial.NumCellsInRadius(4f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = wearer.Position + GenRadial.RadialPattern[i];
			if (intVec.IsValid && intVec.InBounds(map))
			{
				SpawnEffect(GenSpawn.Spawn(ThingDefOf.BroadshieldProjector, intVec, map));
				comp.UsedOnce();
				return true;
			}
		}
		Messages.Message("AbilityNotEnoughFreeSpace".Translate(), wearer, MessageTypeDefOf.RejectInput, historical: false);
		return false;
	}

	private static void SpawnEffect(Thing projector)
	{
		FleckMaker.Static(projector.TrueCenter(), projector.Map, FleckDefOf.BroadshieldActivation);
		SoundDefOf.Broadshield_Startup.PlayOneShot(new TargetInfo(projector.Position, projector.Map));
	}
}
