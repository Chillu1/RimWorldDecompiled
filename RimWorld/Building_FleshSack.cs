using Verse;

namespace RimWorld;

public class Building_FleshSack : Building_Casket
{
	public override bool CanOpen => false;

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (base.Spawned && innerContainer.Count > 0 && mode != DestroyMode.Vanish)
		{
			Messages.Message("FleshmassContainedMessage".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst(), new LookTargets(base.Position, base.Map), MessageTypeDefOf.NegativeEvent);
		}
		base.Destroy(mode);
	}
}
