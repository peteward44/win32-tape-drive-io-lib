using System;
using System.IO;
using System.Runtime.InteropServices;


namespace TapeDriveIO
{
	/// <summary>
	/// Publically holds all Win32 tape drive functions and associated structures
	/// </summary>
	public class TapeDriveFunctions
	{
		[StructLayout(LayoutKind.Sequential)]
			public class SecurityAttributes
		{
			public UInt32 size;
			public IntPtr securityDescripter;
			public UInt32 inheritHandle;
		}

		[StructLayout(LayoutKind.Sequential)]
			public struct FileTime
		{
			public UInt32 dwLowDateTime; 
			public UInt32 dwHighDateTime; 
		}

		[StructLayout(LayoutKind.Sequential)]
			public class Overlapped
		{
			public IntPtr  Internal; 
			public IntPtr  InternalHigh; 
			public UInt32  Offset; 
			public UInt32  OffsetHigh; 
			public UInt32	 hEvent; 
		}


		[DllImport("kernel32.dll")]
		public static extern UInt32 CreateFile(
			string lpFileName,                         // file name
			UInt32 dwDesiredAccess,                      // access mode
			UInt32 dwShareMode,                          // share mode
			SecurityAttributes lpSecurityAttributes, // SD
			UInt32 dwCreationDisposition,                // how to create
			UInt32 dwFlagsAndAttributes,                 // file attributes
			UInt32 hTemplateFile                        // handle to template file
			);


		[DllImport("kernel32.dll")]
		public static extern bool BackupRead(
			UInt32 hFile,                // handle to file or directory
			byte[] lpBuffer,             // read buffer
			UInt32 nNumberOfBytesToRead,  // number of bytes to read
			out UInt32 lpNumberOfBytesRead, // number of bytes read
			bool bAbort,                 // termination type
			bool bProcessSecurity,       // process security options
			IntPtr lpContext            // context information
			);

		[DllImport("kernel32.dll")]
		public static extern bool WriteFile(
			UInt32 hFile,                    // handle to file
			byte[] lpBuffer,                // data buffer
			UInt32 nNumberOfBytesToWrite,     // number of bytes to write
			out UInt32 lpNumberOfBytesWritten,  // number of bytes written
			Overlapped lpOverlapped        // overlapped buffer
			);

		[DllImport("kernel32.dll")]
		public static extern bool BackupWrite(
			UInt32 hFile,                   // handle to file or directory
			byte[] lpBuffer,                // write buffer
			UInt32 nNumberOfBytesToWrite,    // number of bytes to write
			out UInt32 lpNumberOfBytesWritten, // number of bytes written
			bool bAbort,                    // termination type
			bool bProcessSecurity,          // process security
			IntPtr lpContext               // context information
			);

		[DllImport("kernel32.dll")]
		public static extern bool ReadFile(
			UInt32 hFile,                // handle to file
			byte[] lpBuffer,             // data buffer
			UInt32 nNumberOfBytesToRead,  // number of bytes to read
			out UInt32 lpNumberOfBytesRead, // number of bytes read
			Overlapped lpOverlapped    // overlapped buffer
			);

		[DllImport("kernel32.dll")]
		public static extern bool CloseHandle(
			UInt32 hObject   // handle to object
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 PrepareTape(
			UInt32 hDevice,     // handle to device
			UInt32 dwOperation,  // preparation method
			bool bImmediate     // return after operation begins
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 CreateTapePartition(
			UInt32 hDevice,           // handle to device
			UInt32 dwPartitionMethod,  // new partition type
			UInt32 dwCount,            // number of new partitions
			UInt32 dwSize              // size of new partition
			);

		[StructLayout(LayoutKind.Sequential)]
			public class TapeMediaInformation
		{
			public long Capacity; 
			public long Remaining; 
			public UInt32 BlockSize; 
			public UInt32 PartitionCount; 
			public UInt32 WriteProtected; 
		}

		[StructLayout(LayoutKind.Sequential)]
			public class SetTapeMediaInformation
		{
			public UInt32 BlockSize; 
		}


		[StructLayout(LayoutKind.Sequential)]
			public class TapeDriveInformation
		{
			public UInt32 ECC; 
			public UInt32 Compression; 
			public UInt32 DataPadding; 
			public UInt32 ReportSetmarks; 
			public UInt32 DefaultBlockSize; 
			public UInt32 MaximumBlockSize; 
			public UInt32 MinimumBlockSize; 
			public UInt32 MaximumPartitionCount; 
			public UInt32 FeaturesLow; 
			public UInt32 FeaturesHigh; 
			public UInt32 EOTWarningZoneSize; 
		}

		[StructLayout(LayoutKind.Sequential)]
			public class SetTapeDriveInformation
		{
			public UInt32 ECC; 
			public UInt32 Compression; 
			public UInt32 DataPadding; 
			public UInt32 ReportSetmarks; 
			public UInt32 EOTWarningZoneSize;
		}

		[DllImport("kernel32.dll")]
		public static extern UInt32 GetTapePosition(
			UInt32 hDevice,          // handle to device
			UInt32 dwPositionType,    // address type
			out UInt32 lpdwPartition,   // current tape partition
			out UInt32 lpdwOffsetLow,   // low-order bits of position
			out UInt32 lpdwOffsetHigh   // high-order bits of position
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 SetTapePosition(
			UInt32 hDevice,          // handle to device
			UInt32 dwPositionMethod,  // positioning type
			UInt32 dwPartition,       // new tape partition
			UInt32 dwOffsetLow,       // low-order bits of position
			UInt32 dwOffsetHigh,      // high-order bits of position
			bool bImmediate          // return after operation begins
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 GetTapeStatus(
			UInt32 hDevice   // handle to device
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 GetTapeParameters(
			UInt32 hDevice,           // handle to device
			UInt32 dwOperation,        // information type
			out UInt32 lpdwSize,         // information
			TapeMediaInformation lpTapeInformation  // tape media or drive information
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 GetTapeParameters(
			UInt32 hDevice,           // handle to device
			UInt32 dwOperation,        // information type
			out UInt32 lpdwSize,         // information
			TapeDriveInformation lpTapeInformation  // tape media or drive information
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 SetTapeParameters(
			UInt32 hDevice,           // handle to device
			UInt32 dwOperation,        // information type
			SetTapeDriveInformation lpTapeInformation  // information buffer
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 SetTapeParameters(
			UInt32 hDevice,           // handle to device
			UInt32 dwOperation,        // information type
			SetTapeMediaInformation lpTapeInformation  // information buffer
			);

		[DllImport("kernel32.dll")]
		public static extern bool FlushFileBuffers(
			UInt32 hFile  // handle to file
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 EraseTape(
			UInt32 hDevice,     // handle to device
			UInt32 dwEraseType,  // type of erasure to perform
			bool bImmediate     // return after erase operation begins
			);

		[DllImport("kernel32.dll")]
		public static extern UInt32 WriteTapemark(
			UInt32 hDevice,        // handle to device
			UInt32 dwTapemarkType,  // tapemark type
			UInt32 dwTapemarkCount, // number of tapemarks to write
			bool bImmediate        // return after write begins
			);


		public static void CheckGeneralError(UInt32 error)
		{
			switch (error)
			{
				case 1106:	// ERROR_INVALID_BLOCK_LENGTH
					throw(new TapeDriveException("Invalid block length"));
				case 1107:	// ERROR_DEVICE_NOT_PARTITIONED
					throw(new TapeDriveException("Device not partitioned"));
				case 1109:	// ERROR_MEDIA_CHANGED
					throw(new MediaChangedException());
				case 1111:	// ERROR_BUS_RESET
					throw(new TapeDriveException("I/O has been reset"));
				case 1112:	// ERROR_NO_MEDIA_IN_DRIVE
					throw(new NoMediaException());
				case 50:		// ERROR_NOT_SUPPORTED
					throw(new NotSupportedException());
			}
		}

		/// <summary>
		/// Gets the tape drive parameters
		/// </summary>
		/// <returns>Class containing drive parameters</returns>
		public static TapeDriveInformation GetTapeDriveParameters(TapeDrive tapeDrive)
		{
			UInt32 size;
			TapeDriveInformation tapeMediaInfo = new TapeDriveInformation();

			GetTapeParameters(tapeDrive.Handle, 1, out size, tapeMediaInfo);

			return tapeMediaInfo;
		}

		/// <summary>
		/// Gets the tape media parameters
		/// </summary>
		/// <returns>Class containing media parameters</returns>
		public static TapeMediaInformation GetTapeMediaParameters(TapeDrive tapeDrive)
		{
			UInt32 size;
			TapeMediaInformation tapeDriveInfo = new TapeMediaInformation();

			GetTapeParameters(tapeDrive.Handle, 0, out size, tapeDriveInfo);

			return tapeDriveInfo;
		}

		public static void SetTapeDriveParameters(TapeDrive tapeDrive, SetTapeDriveInformation info)
		{
			SetTapeParameters(tapeDrive.Handle, 1, info);
		}

		public static void SetTapeMediaParameters(TapeDrive tapeDrive, SetTapeMediaInformation info)
		{
			SetTapeParameters(tapeDrive.Handle, 0, info);
		}
	}
}
