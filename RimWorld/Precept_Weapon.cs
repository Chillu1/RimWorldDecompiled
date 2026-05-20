using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_Weapon : Precept
{
	public WeaponClassDef noble;

	public WeaponClassDef despised;

	[Unsaved(false)]
	private ThingDef iconWeapon;

	[Unsaved(false)]
	private HashSet<ThingDef> nobleWeaponsCached;

	[Unsaved(false)]
	private HashSet<ThingDef> despisedWeaponsCached;

	public override string TipLabel => def.issue.LabelCap + ": " + def.LabelCap;

	public override string UIInfoFirstLine => "Noble".Translate().CapitalizeFirst() + ": " + noble.LabelCap;

	public override string UIInfoSecondLine => "Despised".Translate().CapitalizeFirst() + ": " + despised.LabelCap;

	public override Color LabelColor => Color.white;

	protected HashSet<ThingDef> NobleWeapons
	{
		get
		{
			if (nobleWeaponsCached.EnumerableNullOrEmpty())
			{
				nobleWeaponsCached = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsWeapon && x.PlayerAcquirable && !x.weaponClasses.NullOrEmpty() && x.weaponClasses.Contains(noble)));
			}
			return nobleWeaponsCached;
		}
	}

	protected HashSet<ThingDef> DespisedWeapons
	{
		get
		{
			if (despisedWeaponsCached.EnumerableNullOrEmpty())
			{
				despisedWeaponsCached = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsWeapon && !x.weaponClasses.NullOrEmpty() && x.weaponClasses.Contains(despised)));
			}
			return despisedWeaponsCached;
		}
	}

	private void RecacheData()
	{
		nobleWeaponsCached = null;
		despisedWeaponsCached = null;
		iconWeapon = null;
		ClearTipCache();
	}

	public override void DrawIcon(Rect rect)
	{
		if (iconWeapon == null)
		{
			Rand.PushState(randomSeed);
			iconWeapon = NobleWeapons.Where((ThingDef w) => !w.IsStuff).RandomElementWithFallback();
			Rand.PopState();
		}
		if (iconWeapon == null)
		{
			Rand.PushState(randomSeed);
			iconWeapon = DespisedWeapons.Where((ThingDef w) => !w.IsStuff).RandomElementWithFallback();
			Rand.PopState();
		}
		Widgets.DefIcon(rect, iconWeapon, null, 1f, ideo.GetStyleFor(iconWeapon));
	}

	public override string GetTip()
	{
		if (tipCached.NullOrEmpty())
		{
			tipCached = base.GetTip() + "\n\n" + ColorizeDescTitle("Noble".Translate().CapitalizeFirst()) + ":\n" + (from x in NobleWeapons
				where x.canGenerateDefaultDesignator
				select x.LabelCap.Resolve()).ToCommaList() + "\n\n" + ColorizeDescTitle("Despised".Translate().CapitalizeFirst()) + ":\n" + (from x in DespisedWeapons
				where x.canGenerateDefaultDesignator
				select x.LabelCap.Resolve()).ToCommaList();
		}
		return tipCached;
	}

	public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
	{
		yield return new FloatMenuOption("SwapNobleAndDespised".Translate(), delegate
		{
			Gen.Swap(ref noble, ref despised);
			RecacheData();
		});
	}

	public IdeoWeaponDisposition GetDispositionForWeapon(ThingDef td)
	{
		if (NobleWeapons.Contains(td))
		{
			return IdeoWeaponDisposition.Noble;
		}
		if (DespisedWeapons.Contains(td))
		{
			return IdeoWeaponDisposition.Despised;
		}
		return IdeoWeaponDisposition.None;
	}

	public override bool CompatibleWith(Precept other)
	{
		Precept_Weapon wep = other as Precept_Weapon;
		if (wep != null)
		{
			if (!NobleWeapons.Any((ThingDef x) => wep.DespisedWeapons.Contains(x)))
			{
				return !DespisedWeapons.Any((ThingDef x) => wep.NobleWeapons.Contains(x));
			}
			return false;
		}
		return base.CompatibleWith(other);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref noble, "noble");
		Scribe_Defs.Look(ref despised, "despised");
	}

	public override void CopyTo(Precept other)
	{
		base.CopyTo(other);
		Precept_Weapon obj = (Precept_Weapon)other;
		obj.noble = noble;
		obj.despised = despised;
	}
}
