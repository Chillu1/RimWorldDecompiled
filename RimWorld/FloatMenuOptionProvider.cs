using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class FloatMenuOptionProvider
{
	protected abstract bool Drafted { get; }

	protected abstract bool Undrafted { get; }

	protected abstract bool Multiselect { get; }

	protected virtual bool RequiresManipulation => false;

	protected virtual bool MechanoidCanDo => false;

	protected virtual bool CanSelfTarget => false;

	public virtual bool CanTargetDespawned => false;

	protected virtual bool IgnoreFogged => true;

	public virtual bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
	{
		if (pawn.IsMutant && pawn.mutant.Def.whitelistedFloatMenuProviders != null && !pawn.mutant.Def.whitelistedFloatMenuProviders.Contains(FloatMenuMakerMap.currentProvider.GetType()))
		{
			return false;
		}
		if (!Drafted && pawn.Drafted)
		{
			return false;
		}
		if (!Undrafted && !pawn.Drafted)
		{
			return false;
		}
		if (!MechanoidCanDo && pawn.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (RequiresManipulation && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return false;
		}
		return true;
	}

	public virtual bool TargetThingValid(Thing thing, FloatMenuContext context)
	{
		if (!CanTargetDespawned && !thing.Spawned)
		{
			return false;
		}
		if (thing is Pawn pawn && !TargetPawnValid(pawn, context))
		{
			return false;
		}
		return true;
	}

	public virtual bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
	{
		if (!CanSelfTarget && pawn == context.FirstSelectedPawn)
		{
			return false;
		}
		return true;
	}

	public virtual bool Applies(FloatMenuContext context)
	{
		if (!Multiselect && context.IsMultiselect)
		{
			return false;
		}
		if (IgnoreFogged && context.ClickedCell.Fogged(context.map))
		{
			return false;
		}
		if (!AppliesInt(context))
		{
			return false;
		}
		return true;
	}

	protected virtual bool AppliesInt(FloatMenuContext context)
	{
		return true;
	}

	public virtual IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
	{
		FloatMenuOption singleOption = GetSingleOption(context);
		if (singleOption != null)
		{
			yield return singleOption;
		}
	}

	public virtual IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		FloatMenuOption singleOptionFor = GetSingleOptionFor(clickedThing, context);
		if (singleOptionFor != null)
		{
			yield return singleOptionFor;
		}
	}

	public virtual IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		FloatMenuOption singleOptionFor = GetSingleOptionFor(clickedPawn, context);
		if (singleOptionFor != null)
		{
			yield return singleOptionFor;
		}
	}

	protected virtual FloatMenuOption GetSingleOption(FloatMenuContext context)
	{
		return null;
	}

	protected virtual FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		return null;
	}

	protected virtual FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		return null;
	}
}
