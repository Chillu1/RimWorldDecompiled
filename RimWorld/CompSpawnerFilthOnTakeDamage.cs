using Verse;

namespace RimWorld;

public class CompSpawnerFilthOnTakeDamage : ThingComp
{
	public CompProperties_SpawnFilthOnTakeDamage Props => (CompProperties_SpawnFilthOnTakeDamage)props;

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (parent.Spawned && Rand.Chance(Props.chance))
		{
			int randomInRange = Props.filthCountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				FilthMaker.TryMakeFilth(parent.OccupiedRect().ExpandedBy(1).EdgeCells.RandomElement(), parent.Map, Props.filthDef, out var _, parent.LabelNoParenthesis, FilthSourceFlags.None, shouldPropagate: false);
			}
		}
	}
}
