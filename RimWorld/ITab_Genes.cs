using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_Genes : ITab
{
	protected Vector2 scrollPosition;

	protected const float TopPadding = 20f;

	public const float GeneSize = 90f;

	public const float GeneGap = 6f;

	public const int MaxGenesHorizontal = 7;

	public const float InitialWidth = 736f;

	protected const float InitialHeight = 550f;

	public override bool Hidden => true;

	public override bool IsVisible => CanShowGenesTab();

	protected override bool StillValid
	{
		get
		{
			if (!base.StillValid)
			{
				return false;
			}
			return ThingWithGenes() != null;
		}
	}

	protected Pawn SelPawnForGenes => PawnForGenes(base.SelThing);

	public ITab_Genes()
	{
		size = new Vector2(Mathf.Min(736f, UI.screenWidth), 550f);
		labelKey = "TabGenes";
	}

	public override void OnOpen()
	{
		if (!ModLister.CheckBiotech("genes viewing"))
		{
			CloseTab();
		}
		else
		{
			base.OnOpen();
		}
	}

	protected override void FillTab()
	{
		GeneUIUtility.DrawGenesInfo(new Rect(0f, 20f, size.x, size.y - 20f), ThingWithGenes(), 550f, ref size, ref scrollPosition);
	}

	private static Thing ThingWithGenes()
	{
		Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
		Pawn pawn = PawnForGenes(singleSelectedThing);
		if (pawn != null)
		{
			return pawn;
		}
		if (singleSelectedThing is GeneSetHolderBase)
		{
			return singleSelectedThing;
		}
		if (singleSelectedThing is Building_GrowthVat building_GrowthVat)
		{
			return building_GrowthVat.selectedEmbryo;
		}
		return null;
	}

	private static Pawn PawnForGenes(Thing thing)
	{
		if (thing is Pawn { genes: not null } pawn)
		{
			return pawn;
		}
		if (thing is Corpse corpse && corpse.InnerPawn.genes != null)
		{
			return corpse.InnerPawn;
		}
		return null;
	}

	public static bool CanShowGenesTab()
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		Pawn pawn = PawnForGenes(Find.Selector.SingleSelectedThing);
		if (pawn != null && pawn.genes != null)
		{
			return true;
		}
		if (ThingWithGenes() != null)
		{
			return true;
		}
		return false;
	}
}
