using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class DropPodIncoming : Skyfaller, IActiveTransporter, IThingHolder
{
	public ActiveTransporterInfo Contents
	{
		get
		{
			return ((ActiveTransporter)innerContainer[0]).Contents;
		}
		set
		{
			((ActiveTransporter)innerContainer[0]).Contents = value;
		}
	}

	protected override void SpawnThings()
	{
		if (!Contents.spawnWipeMode.HasValue)
		{
			base.SpawnThings();
			return;
		}
		for (int num = innerContainer.Count - 1; num >= 0; num--)
		{
			GenSpawn.Spawn(innerContainer[num], base.Position, base.Map, Contents.spawnWipeMode.Value);
		}
	}

	protected override void Impact()
	{
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map, 1.2f);
		}
		FleckMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
		GenClamor.DoClamor(this, 15f, ClamorDefOf.Impact);
		base.Impact();
	}
}
