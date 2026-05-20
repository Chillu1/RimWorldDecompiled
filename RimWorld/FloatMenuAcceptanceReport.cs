namespace RimWorld;

public struct FloatMenuAcceptanceReport
{
	private string failMessageInt;

	private string failReasonInt;

	private bool acceptedInt;

	public bool Accepted => acceptedInt;

	public string FailMessage => failMessageInt;

	public string FailReason => failReasonInt;

	public static FloatMenuAcceptanceReport WasAccepted => new FloatMenuAcceptanceReport
	{
		acceptedInt = true
	};

	public static FloatMenuAcceptanceReport WasRejected => new FloatMenuAcceptanceReport
	{
		acceptedInt = false
	};

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
		return new FloatMenuAcceptanceReport
		{
			acceptedInt = false,
			failReasonInt = failReason
		};
	}

	public static FloatMenuAcceptanceReport WithFailMessage(string failMessage)
	{
		return new FloatMenuAcceptanceReport
		{
			acceptedInt = false,
			failMessageInt = failMessage
		};
	}

	public static FloatMenuAcceptanceReport WithFailReasonAndMessage(string failReason, string failMessage)
	{
		return new FloatMenuAcceptanceReport
		{
			acceptedInt = false,
			failReasonInt = failReason,
			failMessageInt = failMessage
		};
	}
}
