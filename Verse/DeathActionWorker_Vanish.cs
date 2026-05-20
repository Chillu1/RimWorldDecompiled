using RimWorld;
using Verse.AI.Group;

namespace Verse;

public class DeathActionWorker_Vanish : DeathActionWorker
{
	public DeathActionProperties_Vanish Props => (DeathActionProperties_Vanish)props;

	public override void PawnDied(Corpse corpse, Lord prevLord)
	{
		if (Props.fleck != null)
		{
			FleckMaker.Static(corpse.PositionHeld, corpse.MapHeld, Props.fleck);
		}
		if (Props.filth != null)
		{
			int randomInRange = Props.filthCountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				FilthMaker.TryMakeFilth(corpse.PositionHeld, corpse.MapHeld, Props.filth);
			}
		}
		if (Props.meatExplosionSize.HasValue && ModsConfig.AnomalyActive)
		{
			FleshbeastUtility.MeatSplatter(0, corpse.PositionHeld, corpse.MapHeld, Props.meatExplosionSize.Value);
		}
		corpse.Destroy();
	}
}
