using RimWorld;

namespace Verse;

public class CompLifespan : ThingComp
{
	public int age = -1;

	public CompProperties_Lifespan Props => (CompProperties_Lifespan)props;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref age, "age", 0);
	}

	public override void CompTick()
	{
		age++;
		if (age >= Props.lifespanTicks)
		{
			Expire();
		}
	}

	public override void CompTickRare()
	{
		age += 250;
		if (age >= Props.lifespanTicks)
		{
			Expire();
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		string result = "";
		int num = Props.lifespanTicks - age;
		if (num > 0)
		{
			result = "LifespanExpiry".Translate() + " " + num.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
			if (!text.NullOrEmpty())
			{
				result = "\n" + text;
			}
		}
		return result;
	}

	protected void Expire()
	{
		if (parent.Spawned)
		{
			if (Props.expireEffect != null)
			{
				Props.expireEffect.Spawn(parent.Position, parent.Map).Cleanup();
			}
			if (Props.plantDefToSpawn != null && Props.plantDefToSpawn.CanNowPlantAt(parent.Position, parent.Map))
			{
				GenSpawn.Spawn(Props.plantDefToSpawn, parent.Position, parent.Map);
			}
			parent.Destroy(DestroyMode.KillFinalize);
		}
	}
}
