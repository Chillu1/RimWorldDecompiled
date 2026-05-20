using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class SignalAction_DormancyWakeUp : SignalAction_Delay
{
	public Lord lord;

	private Alert_ActionDelay cachedAlert;

	public override Alert_ActionDelay Alert
	{
		get
		{
			if (cachedAlert == null && lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer) && lord.ownedPawns.Count > 0)
			{
				cachedAlert = new Alert_DormanyWakeUpDelay(this);
			}
			return cachedAlert;
		}
	}

	public override bool ShouldRemoveNow
	{
		get
		{
			if (lord == null || !base.Map.lordManager.lords.Contains(lord))
			{
				return true;
			}
			List<Pawn> ownedPawns = lord.ownedPawns;
			if (ownedPawns.NullOrEmpty())
			{
				return true;
			}
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (!ownedPawns[i].Awake())
				{
					return false;
				}
			}
			return true;
		}
	}

	protected override void Complete()
	{
		base.Complete();
		if (lord != null)
		{
			lord.Notify_DormancyWakeup();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref lord, "lord");
	}
}
