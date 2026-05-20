using Verse;

namespace RimWorld;

public class CompUseEffect_CallBossgroup : CompUseEffect
{
	private Effecter prepareEffecter;

	public CompProperties_Useable_CallBossgroup Props => (CompProperties_Useable_CallBossgroup)props;

	public bool ShouldSendSpawnLetter
	{
		get
		{
			if (Props.spawnLetterLabelKey.NullOrEmpty() || Props.spawnLetterTextKey.NullOrEmpty())
			{
				return false;
			}
			if (!MechanitorUtility.AnyMechanitorInPlayerFaction())
			{
				return false;
			}
			if (Find.BossgroupManager.lastBossgroupCalled > 0)
			{
				return false;
			}
			return true;
		}
	}

	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		if (Props.effecterDef != null)
		{
			Effecter obj = new Effecter(Props.effecterDef);
			obj.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
			obj.Cleanup();
		}
		prepareEffecter?.Cleanup();
		prepareEffecter = null;
		CallBossgroup();
	}

	private void CallBossgroup()
	{
		GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
		if (component == null)
		{
			Log.Error("Trying to call bossgroup with no GameComponent_Bossgroup.");
		}
		else
		{
			Props.bossgroupDef.Worker.Resolve(parent.Map, component.NumTimesCalledBossgroup(Props.bossgroupDef));
		}
	}

	public override TaggedString ConfirmMessage(Pawn p)
	{
		GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
		return "BossgroupWarningDialog".Translate(NamedArgumentUtility.Named(Props.bossgroupDef.boss.kindDef, "LEADERKIND"), Props.bossgroupDef.GetWaveDescription(component.NumTimesCalledBossgroup(Props.bossgroupDef)).Named("PAWNS"));
	}

	public override void PrepareTick()
	{
		if (Props.prepareEffecterDef != null && prepareEffecter == null)
		{
			prepareEffecter = Props.prepareEffecterDef.Spawn(parent.Position, parent.MapHeld);
		}
		prepareEffecter?.EffectTick(parent, TargetInfo.Invalid);
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (Faction.OfMechanoids == null || Faction.OfMechanoids.deactivated)
		{
			return "MechsDisabled".Translate();
		}
		if (!MechanitorUtility.IsMechanitor(p))
		{
			return "RequiresMechanitor".Translate();
		}
		return Props.bossgroupDef.Worker.CanResolve(p);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotech("Call bossgroup"))
		{
			parent.Destroy();
		}
		else if (!respawningAfterLoad && ShouldSendSpawnLetter)
		{
			Props.SendBossgroupDetailsLetter(Props.spawnLetterLabelKey, Props.spawnLetterTextKey, parent.def);
		}
	}
}
