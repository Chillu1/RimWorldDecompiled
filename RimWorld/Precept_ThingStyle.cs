using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Precept_ThingStyle : Precept_ThingDef
{
	public override string UIInfoFirstLine => def.LabelCap;

	public override string UIInfoSecondLine => base.LabelCap;

	public override bool UsesGeneratedName => true;

	public override bool CanRegenerate => true;

	public override bool SortByImpact => false;

	protected virtual string NameRootSymbol => "root";

	protected virtual string ThingLabelCap => base.ThingDef.LabelCap;

	public override string GenerateNameRaw()
	{
		GrammarRequest request = new GrammarRequest
		{
			Includes = { def.nameMaker }
		};
		AddIdeoRulesTo(ref request);
		if (base.ThingDef.ideoBuildingNamerBase != null)
		{
			request.Includes.Add(base.ThingDef.ideoBuildingNamerBase);
		}
		string thingLabelCap = ThingLabelCap;
		if (thingLabelCap != null)
		{
			request.Rules.Add(new Rule_String("thingLabel", thingLabelCap));
		}
		string text = null;
		int num = 100;
		while (num > 0)
		{
			text = GrammarResolver.Resolve(NameRootSymbol, request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false);
			num--;
			if (text.Length <= def.nameMaxLength)
			{
				break;
			}
		}
		return GenText.CapitalizeAsTitle(text);
	}

	public virtual void Notify_ThingLost(Thing thing, bool destroyed = false)
	{
	}

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(Thing thing)
	{
		return Enumerable.Empty<StatDrawEntry>();
	}

	public virtual string TransformThingLabel(string label)
	{
		return label;
	}

	public virtual string InspectStringExtra(Thing thing)
	{
		return null;
	}
}
