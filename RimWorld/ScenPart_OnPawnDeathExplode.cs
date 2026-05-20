using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_OnPawnDeathExplode : ScenPart
{
	private float radius = 5.9f;

	private DamageDef damage;

	private string radiusBuf;

	public override void Randomize()
	{
		radius = (float)Rand.RangeInclusive(3, 8) - 0.1f;
		damage = PossibleDamageDefs().RandomElement();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref radius, "radius", 0f);
		Scribe_Defs.Look(ref damage, "damage");
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_OnPawnDeathExplode".Translate(damage.label, radius.ToString());
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
		Widgets.TextFieldNumericLabeled(scenPartRect.TopHalf(), "radius".Translate(), ref radius, ref radiusBuf);
		if (!Widgets.ButtonText(scenPartRect.BottomHalf(), damage.LabelCap))
		{
			return;
		}
		FloatMenuUtility.MakeMenu(PossibleDamageDefs(), (DamageDef d) => d.LabelCap, (DamageDef d) => delegate
		{
			damage = d;
		});
	}

	public override void Notify_PawnDied(Corpse corpse)
	{
		if (corpse.Spawned)
		{
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, radius, damage, null);
		}
	}

	private IEnumerable<DamageDef> PossibleDamageDefs()
	{
		yield return DamageDefOf.Bomb;
		yield return DamageDefOf.Flame;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((damage != null) ? damage.GetHashCode() : 0) ^ radius.GetHashCode();
	}
}
