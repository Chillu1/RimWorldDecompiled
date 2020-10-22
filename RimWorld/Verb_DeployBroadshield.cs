using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Verb_DeployBroadshield : Verb
	{
		protected override bool TryCastShot()
		{
			return Deploy(base.ReloadableCompSource);
		}

		public static bool Deploy(CompReloadable comp)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shields are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 86573384);
				return false;
			}
			if (comp == null || !comp.CanBeUsed)
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
			MoteMaker.MakeStaticMote(projector.TrueCenter(), projector.Map, ThingDefOf.Mote_BroadshieldActivation);
			SoundDefOf.Broadshield_Startup.PlayOneShot(new TargetInfo(projector.Position, projector.Map));
		}
	}
}
