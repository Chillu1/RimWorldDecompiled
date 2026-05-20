using System;

namespace Verse
{
	public static class AutoBreastfeedModeExtension
	{
		public static TaggedString Translate(this AutofeedMode mode)
		{
			return mode switch
			{
				AutofeedMode.Never => "AutofeedModeNever".Translate(), 
				AutofeedMode.Childcare => "AutofeedModeChildcare".Translate(), 
				AutofeedMode.Urgent => "AutofeedModeUrgent".Translate(), 
				_ => throw new NotImplementedException(), 
			};
		}

		public static TaggedString GetTooltip(this AutofeedMode mode, Pawn baby, Pawn feeder)
		{
			return (mode switch
			{
				AutofeedMode.Never => "AutofeedModeTooltipNever", 
				AutofeedMode.Childcare => "AutofeedModeTooltipChildcare", 
				AutofeedMode.Urgent => "AutofeedModeTooltipUrgent", 
				_ => throw new NotImplementedException(), 
			}).Translate(baby.Named("BABY"), feeder.Named("FEEDER"));
		}
	}
}
