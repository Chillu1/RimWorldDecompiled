using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class ThoughtWorker_RelatedChildMoodBase : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (p.Suspended)
		{
			return ThoughtState.Inactive;
		}
		if (!Active(p))
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveDefault;
	}

	public override string PostProcessLabel(Pawn p, string label)
	{
		Pawn pawn = p.relations.Children.FirstOrDefault((Pawn child) => IsChildWithMood(child, MoodRange()));
		if (pawn != null)
		{
			return label.Formatted(p.Named("PAWN"), pawn.Named("CHILD"));
		}
		return SingleChildLabel().Translate().CapitalizeFirst();
	}

	protected abstract bool Active(Pawn p);

	protected abstract string SingleChildLabel();

	protected abstract FloatRange MoodRange();

	protected int ChildrenWithMoodCount(Pawn parent)
	{
		int num = 0;
		if (parent.relations.ChildrenCount > 0)
		{
			foreach (Pawn child in parent.relations.Children)
			{
				if (InSameMapOrCaravan(parent, child) && IsChildWithMood(child, MoodRange()))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool IsChildWithMood(Pawn child, FloatRange moodRange)
	{
		if (child.needs?.mood != null && child.DevelopmentalStage.Juvenile() && moodRange.Includes(child.needs.mood.CurLevelPercentage) && !(child.ParentHolder is Building_GrowthVat))
		{
			return !(child.ParentHolder is Building_CryptosleepCasket);
		}
		return false;
	}

	public static bool InSameMapOrCaravan(Pawn pawn, Pawn other)
	{
		Map mapHeld = pawn.MapHeld;
		if (mapHeld != null && mapHeld == other.MapHeld)
		{
			return true;
		}
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null && caravan == other.GetCaravan())
		{
			return true;
		}
		return false;
	}
}
