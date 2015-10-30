using System;

namespace TapeDriveIO
{
	/// <summary>
	/// Represents a Tape Drive
	/// </summary>
	public class TapeDrive : System.IDisposable
	{
		private enum PrepareOption
		{
			Load = 0,
			UnLoad = 1,
			Tension = 2,
			Lock = 3,
			UnLock = 4,
			Format = 5
		}

		private enum FormatOption
		{
			FixedPartitions = 0,			// Partitions the tape to its defaults
			InitiatorPartitions = 1,	// Partitions to a certain number of partitions and size specified
			SelectPartitions = 2			// Partitions to the given number of partitions using default size
		}

		private UInt32 tapeHandle = 0;
		private bool isLocked = false, isLoaded = false;
		private TapeStream tapeStream;
		private TapeDriveFunctions.TapeDriveInformation info;
		private TapeDriveFunctions.SetTapeDriveInformation setInfo = new TapeDriveFunctions.SetTapeDriveInformation();

		/// <summary>
		/// Returns raw tape handle
		/// </summary>
		internal UInt32 Handle
		{
			get { return tapeHandle; }
		}

		/// <summary>
		/// Locks / Unlocks the drive
		/// </summary>
		public bool Locked
		{
			get { return isLocked; }
			set
			{
				if (value)
					Prepare(PrepareOption.Lock, false);
				else
					Prepare(PrepareOption.UnLock, false);

				isLocked = value;
			}
		}

		/// <summary>
		/// If a tape is loaded. Use the Load() and UnLoad() methods to change this.
		/// </summary>
		public bool Loaded
		{
			get { return isLoaded; }
		}

		/// <summary>
		/// Maximum block size.
		/// </summary>
		public uint MaximumBlockSize
		{
			get { return info.MaximumBlockSize; }
		}

		/// <summary>
		/// Minimum block size.
		/// </summary>
		public uint MinimumBlockSize
		{
			get { return info.MinimumBlockSize; }
		}

		/// <summary>
		/// Default block size.
		/// </summary>
		public uint DefaultBlockSize
		{
			get { return info.DefaultBlockSize; }
		}

		/// <summary>
		/// Maximum number of partitions supported
		/// </summary>
		public uint MaximumSupportedPartitions
		{
			get { return info.MaximumPartitionCount; }
		}

		/// <summary>
		/// Gets/Sets hardware compression
		/// </summary>
		public bool HardwareCompressionEnabled
		{
			get { return setInfo.Compression > 0; }
			set
			{
				setInfo.Compression = (uint)(value ? 1 : 0);
				TapeDriveFunctions.SetTapeDriveParameters(this, setInfo);
			}
		}

		/// <summary>
		/// Gets/Sets if ECC is enabled
		/// </summary>
		public bool HardwareErrorCorrectionEnabled
		{
			get { return (setInfo.ECC > 0); }
			set
			{
				setInfo.ECC = (uint)(value ? 1 : 0);
				TapeDriveFunctions.SetTapeDriveParameters(this, setInfo);
			}
		}

		/// <summary>
		/// Data padding enabled
		/// </summary>
		public bool DataPaddingEnabled
		{
			get { return (setInfo.DataPadding > 0); }
			set
			{
				setInfo.DataPadding = (uint)(value ? 1 : 0);
				TapeDriveFunctions.SetTapeDriveParameters(this, setInfo);
			}
		}

		/// <summary>
		/// Whether to report when a set mark is read
		/// </summary>
		public bool ReportSetMarks
		{
			get { return (info.ReportSetmarks > 0); }
			set
			{
				setInfo.ReportSetmarks = (uint)(value ? 1 : 0);
				TapeDriveFunctions.SetTapeDriveParameters(this, setInfo);
			}
		}

		/// <summary>
		/// Number of bytes between the end of the tape and the EOT warning marker
		/// </summary>
		public uint EndOfTapeWarningZoneSize
		{
			get { return setInfo.EOTWarningZoneSize; }
			set
			{
				setInfo.EOTWarningZoneSize = value;
				TapeDriveFunctions.SetTapeDriveParameters(this, setInfo);
			}
		}

		//////////////////////////////////////////////

		public void Dispose()
		{
			Close();
		}

		//////////////////////////////////////////////

		/// <summary>
		/// Constructs a tape drive with the given integer ID.
		/// All tape drives under windows have an ID, starting at 0.
		/// </summary>
		/// <param name="tapeID">ID of tape</param>
		public TapeDrive(int tapeID)
		{
			// check OS version first
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw(new PlatformNotSupportedException("Tape drive access requires WindowsNT 4 or higher"));

			tapeHandle = TapeDriveFunctions.CreateFile(@"\\.\TAPE" + tapeID,
				0x80000000 | 0x40000000,			// (GENERIC_READ | GENERIC_WRITE) read/write access 
				0,                            // not used 
				null,                         // not used 
				3,														// (OPEN_EXISTING) required for tape devices 
				0,                            // not used 
				0);														// not used

			if (tapeHandle == 0)
				throw(new TapeDriveNotFoundException());

			info = TapeDriveFunctions.GetTapeDriveParameters(this);

			setInfo.Compression = info.Compression;
			setInfo.DataPadding = info.DataPadding;
			setInfo.ECC = info.ECC;
			setInfo.EOTWarningZoneSize = info.EOTWarningZoneSize;
			setInfo.ReportSetmarks = info.ReportSetmarks;

			tapeStream = new TapeStream(this);
		}

		/// <summary>
		/// Closes connection to the tape drive
		/// </summary>
		public void Close()
		{
			if (tapeHandle != 0)
			{
				if (Loaded)
					UnLoad();
				TapeDriveFunctions.CloseHandle(tapeHandle);
				tapeHandle = 0;
			}
		}

		/// <summary>
		/// Loads the tape
		/// </summary>
		public void Load()
		{
			Prepare(PrepareOption.Load, false);
			isLoaded = true;
		}

		/// <summary>
		/// Unloads the tape
		/// </summary>
		public void UnLoad()
		{
			Prepare(PrepareOption.UnLoad, false);
			isLoaded = false;
		}

		/// <summary>
		/// Called by all functions that require PrepareTape()
		/// </summary>
		/// <param name="option">PrepareType option</param>
		/// <param name="immediate">If true, return immediately.
		/// If false, return after operation has finished</param>
		private void Prepare(PrepareOption option, bool immediate)
		{
			CheckError(TapeDriveFunctions.PrepareTape(tapeHandle, (UInt32)option, immediate));
		}

		/// <summary>
		/// Partitions the tape based on the device's default definition of partitions.
		/// </summary>
		public void Format()
		{
			FormatHelper(FormatOption.FixedPartitions, 0, 0);
		}

		/// <summary>
		/// Partitions the tape into the number of partitions specified by numPartitions.
		/// The size of the partitions is determined by the device's default partition size.
		/// For more specific information, refer to the documentation for your tape device.
		/// </summary>
		/// <param name="numPartitions">Number of partitions to create</param>
		public void Format(int numPartitions)
		{
			FormatHelper(FormatOption.SelectPartitions, (UInt32)numPartitions, 0);
		}

		/// <summary>
		/// Partitions the tape into the number and size of partitions
		/// specified by numPartitions and size,
		/// respectively, except for the last partition.
		/// The size of the last partition is the remainder of the tape.
		/// </summary>
		/// <param name="numPartitions">Number of partitions to create</param>
		/// <param name="size">Size, in megabytes, of each partition</param>
		public void Format(int numPartitions, int size)
		{
			FormatHelper(FormatOption.InitiatorPartitions, (UInt32)numPartitions, (UInt32)size);
		}

		/// <summary>
		/// Used by all Format() functions to call CreateTapePartition()
		/// </summary>
		/// <param name="option">Win32 option to send</param>
		/// <param name="count"></param>
		/// <param name="size"></param>
		private void FormatHelper(FormatOption option, UInt32 count, UInt32 size)
		{
			CheckError(TapeDriveFunctions.CreateTapePartition(tapeHandle, (UInt32)option, count, size));
		}

		/// <summary>
		/// Performs a low-level format of the tape.
		/// Currently, only the QIC117 device supports this feature.
		/// </summary>
		/// <param name="immediate">If true, return immediately.
		/// If false, return after operation has finished</param>
		public void LowLevelFormat()
		{
			Prepare(PrepareOption.Format, false);
		}

		/// <summary>
		/// Adjusts the tension by moving the tape to the end of the tape and back to the beginning.
		/// This option is not supported by all devices, shall just return if not supported.
		/// </summary>
		/// <param name="immediate">If true, return immediately.
		/// If false, return after operation has finished</param>
		public void ResetTension()
		{
			Prepare(PrepareOption.Tension, false);
		}


		private void CheckError(UInt32 code)
		{
			switch (code)
			{
				case 1105:	// ERROR_PARTITION_FAILURE
					throw(new TapeDriveException("Format failed: Tape could not be partitioned"));
				case 1108:	// ERROR_UNABLE_TO_LOCK_MEDIA
					throw(new LockFailedException());
				case 1109:	// ERROR_UNABLE_TO_UNLOAD_MEDIA
					break;
				default:
					TapeDriveFunctions.CheckGeneralError(code);
					break;
			}
		}
	}
}
