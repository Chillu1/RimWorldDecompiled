namespace RimWorld
{
	public struct FloatMenuAcceptanceReport
	{
		private string failMessageInt;

		private string failReasonInt;

		private bool acceptedInt;

		public bool Accepted => acceptedInt;

		public string FailMessage => failMessageInt;

		public string FailReason => failReasonInt;

		public static FloatMenuAcceptanceReport WasAccepted
		{
			get
			{
				FloatMenuAcceptanceReport result = default(FloatMenuAcceptanceReport);
				result.acceptedInt = true;
				return result;
			}
		}

		public static FloatMenuAcceptanceReport WasRejected
		{
			get
			{
				FloatMenuAcceptanceReport result = default(FloatMenuAcceptanceReport);
				result.acceptedInt = false;
				return result;
			}
		}

		public static implicit operator FloatMenuAcceptanceReport(bool value)
		{
			if (value)
			{
				return WasAccepted;
			}
			return WasRejected;
		}

		public static implicit operator bool(FloatMenuAcceptanceReport rep)
		{
			return rep.Accepted;
		}

		public static FloatMenuAcceptanceReport WithFailReason(string failReason)
		{
			FloatMenuAcceptanceReport result = default(FloatMenuAcceptanceReport);
			result.acceptedInt = false;
			result.failReasonInt = failReason;
			return result;
		}

		public static FloatMenuAcceptanceReport WithFailMessage(string failMessage)
		{
			FloatMenuAcceptanceReport result = default(FloatMenuAcceptanceReport);
			result.acceptedInt = false;
			result.failMessageInt = failMessage;
			return result;
		}
	}
}
