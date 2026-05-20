using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_DraftedAttack : FloatMenuOptionProvider
{
	private static readonly List<Pawn> tmpPawns = new List<Pawn>();

	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => true;

	protected override bool MechanoidCanDo => true;

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!CanTarget(clickedThing))
		{
			yield break;
		}
		if (context.IsMultiselect)
		{
			yield return GetMultiselectAttackOption(clickedThing, context);
			yield break;
		}
		foreach (FloatMenuOption singleSelectAttackOption in GetSingleSelectAttackOptions(clickedThing, context))
		{
			yield return singleSelectAttackOption;
		}
	}

	private static bool CanTarget(Thing clickedThing)
	{
		if (clickedThing.def.noRightClickDraftAttack && clickedThing.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (clickedThing.def.IsNonDeconstructibleAttackableBuilding)
		{
			return true;
		}
		BuildingProperties building = clickedThing.def.building;
		if (building != null && building.quickTargetable)
		{
			return true;
		}
		if (!clickedThing.def.destroyable)
		{
			return false;
		}
		if (clickedThing.HostileTo(Faction.OfPlayer))
		{
			return true;
		}
		if (clickedThing is Pawn p && p.NonHumanlikeOrWildMan())
		{
			return true;
		}
		return false;
	}

	private static IEnumerable<FloatMenuOption> GetSingleSelectAttackOptions(Thing clickedThing, FloatMenuContext context)
	{
		string failStr = null;
		Pawn pawn = context.FirstSelectedPawn;
		bool hostile = clickedThing.HostileTo(Faction.OfPlayer);
		if (ModsConfig.BiotechActive && pawn.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn, clickedThing))
		{
			failStr = "OutOfCommandRange".Translate();
			yield return new FloatMenuOption("Attack".Translate(clickedThing.Label, clickedThing) + ": " + failStr, null);
			yield break;
		}
		string label;
		Action rangedAction = GetRangedAttackAction(pawn, clickedThing, out label, out failStr);
		if (rangedAction != null)
		{
			FloatMenuOption floatMenuOption = new FloatMenuOption(label, delegate
			{
				FleckMaker.Static(clickedThing.DrawPos, clickedThing.Map, FleckDefOf.FeedbackShoot);
				rangedAction();
			}, MenuOptionPriority.AttackEnemy);
			floatMenuOption.Priority = (hostile ? MenuOptionPriority.AttackEnemy : MenuOptionPriority.VeryLow);
			floatMenuOption.autoTakeable = hostile || (clickedThing.def.building?.quickTargetable ?? false);
			floatMenuOption.autoTakeablePriority = 40f;
			yield return floatMenuOption;
		}
		else if (!failStr.NullOrEmpty())
		{
			yield return new FloatMenuOption((label ?? ((string)"Attack".Translate(clickedThing.Label, clickedThing))) + ": " + failStr, null);
		}
		string label2;
		Action meleeAction = GetMeleeAttackAction(pawn, clickedThing, out label2, out failStr);
		if (meleeAction != null)
		{
			FloatMenuOption floatMenuOption2 = new FloatMenuOption(label2, delegate
			{
				FleckMaker.Static(clickedThing.DrawPos, clickedThing.Map, FleckDefOf.FeedbackMelee);
				meleeAction();
			}, MenuOptionPriority.AttackEnemy);
			floatMenuOption2.Priority = (hostile ? MenuOptionPriority.AttackEnemy : MenuOptionPriority.VeryLow);
			floatMenuOption2.autoTakeable = hostile || (clickedThing.def.building?.quickTargetable ?? false);
			floatMenuOption2.autoTakeablePriority = 30f;
			yield return floatMenuOption2;
		}
		else if (!failStr.NullOrEmpty())
		{
			yield return new FloatMenuOption((label2 ?? ((string)"Attack".Translate(clickedThing.Label, clickedThing))) + ": " + failStr, null);
		}
	}

	private FloatMenuOption GetMultiselectAttackOption(Thing clickedThing, FloatMenuContext context)
	{
		tmpPawns.Clear();
		string label = null;
		bool flag = clickedThing.HostileTo(Faction.OfPlayer);
		foreach (Pawn validSelectedPawn in context.ValidSelectedPawns)
		{
			if (GetAttackAction(validSelectedPawn, clickedThing, out label, out var _) != null)
			{
				tmpPawns.Add(validSelectedPawn);
			}
		}
		if (tmpPawns.Count == 0)
		{
			return null;
		}
		FleckDef fleck = (FloatMenuUtility.UseRangedAttack(tmpPawns[0]) ? FleckDefOf.FeedbackShoot : FleckDefOf.FeedbackMelee);
		return new FloatMenuOption(label ?? ((string)"Attack".Translate(clickedThing.Label, clickedThing)), delegate
		{
			FleckMaker.Static(clickedThing.DrawPos, clickedThing.Map, fleck);
			foreach (Pawn tmpPawn in tmpPawns)
			{
				GetAttackAction(tmpPawn, clickedThing, out var _, out var _)?.Invoke();
			}
		}, MenuOptionPriority.AttackEnemy)
		{
			Priority = (flag ? MenuOptionPriority.AttackEnemy : MenuOptionPriority.VeryLow),
			autoTakeable = (flag || (clickedThing.def.building?.quickTargetable ?? false)),
			autoTakeablePriority = 40f
		};
	}

	private static Action GetAttackAction(Pawn pawn, Thing target, out string label, out string failStr)
	{
		failStr = null;
		label = "Attack".Translate(target.Label, target);
		if (ModsConfig.BiotechActive && pawn.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn, target))
		{
			failStr = "OutOfCommandRange".Translate();
			return null;
		}
		Action rangedAttackAction = GetRangedAttackAction(pawn, target, out label, out failStr);
		if (rangedAttackAction != null)
		{
			return rangedAttackAction;
		}
		return GetMeleeAttackAction(pawn, target, out label, out failStr);
	}

	private static Action GetRangedAttackAction(Pawn pawn, Thing target, out string label, out string failStr)
	{
		failStr = null;
		label = "FireAt".Translate(target.Label, target);
		if (!FloatMenuUtility.UseRangedAttack(pawn))
		{
			return null;
		}
		label = "FireAt".Translate(target.Label, target);
		return FloatMenuUtility.GetRangedAttackAction(pawn, target, out failStr);
	}

	private static Action GetMeleeAttackAction(Pawn pawn, Thing target, out string label, out string failStr)
	{
		failStr = null;
		label = "Attack".Translate(target.Label, target);
		if (target is Pawn { Downed: not false })
		{
			label = "MeleeAttackToDeath".Translate(target.Label, target);
		}
		else
		{
			label = "MeleeAttack".Translate(target.Label, target);
		}
		return FloatMenuUtility.GetMeleeAttackAction(pawn, target, out failStr);
	}

	[Obsolete]
	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		return base.GetSingleOptionFor(clickedThing, context);
	}

	[Obsolete]
	private static FloatMenuOption GetSingleSelectAttackOption(Thing clickedThing, FloatMenuContext context)
	{
		string label;
		string failStr;
		Action action = GetAttackAction(context.FirstSelectedPawn, clickedThing, out label, out failStr);
		FleckDef fleck = (FloatMenuUtility.UseRangedAttack(context.FirstSelectedPawn) ? FleckDefOf.FeedbackShoot : FleckDefOf.FeedbackMelee);
		if (action == null)
		{
			if (!failStr.NullOrEmpty())
			{
				return new FloatMenuOption((label ?? ((string)"Attack".Translate(clickedThing.Label, clickedThing))) + ": " + failStr, null);
			}
			return null;
		}
		return new FloatMenuOption(label ?? ((string)"Attack".Translate(clickedThing.Label, clickedThing)), delegate
		{
			FleckMaker.Static(clickedThing.DrawPos, clickedThing.Map, fleck);
			action();
		}, MenuOptionPriority.AttackEnemy);
	}
}
