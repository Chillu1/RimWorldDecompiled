using Verse;

namespace RimWorld;

public class WeaponTraitWorker
{
	public WeaponTraitDef def;

	public virtual void Notify_Bonded(Pawn pawn)
	{
		if (!def.bondedHediffs.NullOrEmpty())
		{
			for (int i = 0; i < def.bondedHediffs.Count; i++)
			{
				pawn.health.AddHediff(def.bondedHediffs[i], pawn.health.hediffSet.GetBrain());
			}
		}
	}

	public virtual void Notify_Equipped(Pawn pawn)
	{
		if (!def.equippedHediffs.NullOrEmpty())
		{
			for (int i = 0; i < def.equippedHediffs.Count; i++)
			{
				pawn.health.AddHediff(def.equippedHediffs[i], pawn.health.hediffSet.GetBrain());
			}
		}
	}

	public virtual void Notify_EquipmentLost(Pawn pawn)
	{
		if (def.equippedHediffs.NullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < def.equippedHediffs.Count; i++)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.equippedHediffs[i]);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}

	public virtual void Notify_KilledPawn(Pawn pawn)
	{
		if (def.killThought != null && pawn.needs.mood != null)
		{
			Thought_WeaponTrait thought_WeaponTrait = (Thought_WeaponTrait)ThoughtMaker.MakeThought(def.killThought);
			thought_WeaponTrait.weapon = pawn.equipment.Primary;
			pawn.needs.mood.thoughts.memories.TryGainMemory(thought_WeaponTrait);
		}
	}

	public virtual void Notify_Unbonded(Pawn pawn)
	{
		if (def.bondedHediffs.NullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < def.bondedHediffs.Count; i++)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.bondedHediffs[i]);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}

	public virtual void Notify_OtherWeaponWielded(CompBladelinkWeapon weapon)
	{
	}
}
