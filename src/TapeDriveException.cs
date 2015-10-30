using System;

namespace TapeDriveIO
{
	/// <summary>
	/// Base exception for all tape drive errors
	/// </summary>
	public class TapeDriveException : Exception
	{
		public TapeDriveException()
		{
		}

		public TapeDriveException(string message) : base(message)
		{
		}

		public TapeDriveException(string message, Exception inner) : base(message, inner)
		{
		}
	}

	/// <summary>
	/// Tape drive ID has not been found
	/// </summary>
	public class TapeDriveNotFoundException : TapeDriveException
	{
		public TapeDriveNotFoundException() : base("Tape drive has no media")
		{
		}
	}

	/// <summary>
	/// Lock failed on tape
	/// </summary>
	public class LockFailedException : TapeDriveException
	{
		public LockFailedException() : base("Could not lock media")
		{
		}
	}

	/// <summary>
	/// If the media changes
	/// </summary>
	public class MediaChangedException : TapeDriveException
	{
		public MediaChangedException() : base("Media has changed in drive")
		{
		}
	}

	/// <summary>
	/// Theres no media in the tape drive
	/// </summary>
	public class NoMediaException : TapeDriveException
	{
		public NoMediaException() : base("No media was detected in the tape drive")
		{
		}
	}

	/// <summary>
	/// Media is write-protected
	/// </summary>
	public class WriteProtectedException : TapeDriveException
	{
		public WriteProtectedException() : base("Media is write-protected")
		{
		}
	}
}
