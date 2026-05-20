namespace Verse;

public class HediffWithTarget : HediffWithComps
{
	public Thing target;

	public override bool ShouldRemove
	{
		get
		{
			if (target != null && !(target is Pawn { Dead: not false }))
			{
				return base.ShouldRemove;
			}
			return true;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref target, "target");
	}
}
