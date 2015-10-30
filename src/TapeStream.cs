using System;
using System.IO;


namespace TapeDriveIO
{
	/// <summary>
	/// Tape Stream. For reading and writing to a tape
	/// </summary>
	public class TapeStream : System.IO.Stream
	{
		private TapeDrive tapeDrive;


		private enum TapePosition
		{
			Absolute = 0, // Device-specific block address
			Logical = 1		// Logical block address 
		}

		private bool setMarkDetected = false, fileMarkDetected = false, endOfDataDetected = false;
		private bool beginningMediaDetected = false, endMediaDetected = false;

		//////////////////////////////////////////////

		public bool SetMarkDetected
		{
			get { return setMarkDetected; }
		}

		public bool FileMarkDetected
		{
			get { return fileMarkDetected; }
		}

		public bool BeginningOfMediaDetected
		{
			get { return beginningMediaDetected; }
		}

		public bool EndMediaDetected
		{
			get { return endMediaDetected; }
		}

		public bool EndOfDataDetected
		{
			get { return endOfDataDetected; }
		}

		public void ClearErrors()
		{
			setMarkDetected = fileMarkDetected = endOfDataDetected = true;
			beginningMediaDetected = endMediaDetected = true;
		}
		//////////////////////////////////////////////

		/// <summary>
		/// Constructs a TapeStream
		/// </summary>
		/// <param name="drive">TapeDrive to read/write from</param>
		public TapeStream(TapeDrive drive)
		{
			if (!drive.Loaded)
				drive.Load();
			tapeDrive = drive;
		}

		/// <summary>
		/// Gets/Sets the position within the stream
		/// </summary>
		public override long Position
		{
			get
			{
				UInt32 lowBits, highBits, partitionID;

				CheckError(TapeDriveFunctions.GetTapePosition(tapeDrive.Handle, (UInt32)TapePosition.Absolute, out partitionID, out lowBits, out highBits));

				return lowBits | (highBits << 32);
			}
			set
			{
				UInt32 lowBits, highBits;
				lowBits = (UInt32)value;
				highBits = (UInt32)(value >> 32);
				CheckError(TapeDriveFunctions.SetTapePosition(tapeDrive.Handle, 1, 0, lowBits, highBits, false));
			}
		}

		/// <summary>
		/// True if the stream is ok, false if it isnt
		/// </summary>
		public bool IsGood
		{
			get
			{
				return !(setMarkDetected || fileMarkDetected || endOfDataDetected || beginningMediaDetected || endMediaDetected);
			}
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		/// <summary>
		/// Length of tape drive
		/// </summary>
		public override long Length
		{
			get
			{
				TapeDriveFunctions.TapeMediaInformation info = TapeDriveFunctions.GetTapeMediaParameters(tapeDrive);
				return info.Capacity;
			}
		}

		/// <summary>
		/// Flushes buffers to the drive
		/// </summary>
		public override void Flush()
		{
			TapeDriveFunctions.FlushFileBuffers(tapeDrive.Handle);
		}

		/// <summary>
		/// Reads data from the tapedrive
		/// </summary>
		/// <param name="buffer">byte buffer to copy into</param>
		/// <param name="offset">integer offset within buffer to start copying data to</param>
		/// <param name="count">number of bytes to copy</param>
		/// <returns>number of bytes read</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			UInt32 bytesRead;
			byte[] buffer2 = new byte[count];

			TapeDriveFunctions.ReadFile(tapeDrive.Handle, buffer2, (UInt32)count, out bytesRead, null);

			Array.Copy(buffer2, 0, buffer, offset, (int)bytesRead);

			return (int)bytesRead;
		}

		/// <summary>
		/// Writes data to the tapedrive
		/// </summary>
		/// <param name="buffer">byte buffer to copy from</param>
		/// <param name="offset">integer offset within buffer to start copying data from</param>
		/// <param name="count">number of bytes to copy</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			UInt32 bytesWritten;
			byte[] buffer2 = new byte[count];

			Array.Copy(buffer, offset, buffer2, 0, count);

			TapeDriveFunctions.WriteFile(tapeDrive.Handle, buffer2, (UInt32)count, out bytesWritten, null);
		}

		/// <summary>
		/// Seeks to a specified position within the stream
		/// </summary>
		/// <param name="offset">offset from SeekOrigin to seek to</param>
		/// <param name="seekTo">where seeking should begin</param>
		/// <returns>new stream position</returns>
		public override long Seek(long offset, SeekOrigin seekTo)
		{
			long position = 0;

			switch (seekTo)
			{
				case SeekOrigin.Begin:
					position = offset;
					break;
				case SeekOrigin.Current:
					position = Position + offset;
					break;
				case SeekOrigin.End:
					position = Length + offset;
					break;
			}

			UInt32 lowBits, highBits;

			lowBits = (UInt32)position;
			highBits = (UInt32)(position >> 32);

			CheckError(TapeDriveFunctions.SetTapePosition(tapeDrive.Handle, 0, 0, lowBits, highBits, false));

			return position;
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		/// <param name="length"></param>
		public override void SetLength(long length)
		{
		}

		/// <summary>
		/// Erases the tape from the current position to the end of partition.
		/// </summary>
		public void Erase()
		{
			CheckError(TapeDriveFunctions.EraseTape(tapeDrive.Handle, 1, false));
		}

		/// <summary>
		/// Writes the specified number of filemarks to the current position
		/// </summary>
		/// <param name="count">Number of filemarks to write</param>
		public void WriteFileMark(uint count)
		{
			CheckError(TapeDriveFunctions.WriteTapemark(tapeDrive.Handle, 1, count, false));
		}

		/// <summary>
		/// Writes the specified number of long filemarks to the current position
		/// </summary>
		/// <param name="count">Number of long filemarks to write</param>
		public void WriteLongFileMark(uint count)
		{
			CheckError(TapeDriveFunctions.WriteTapemark(tapeDrive.Handle, 3, count, false));
		}

		/// <summary>
		/// Writes the specified number of short filemarks to the current position
		/// </summary>
		/// <param name="count">Number of short filemarks to write</param>
		public void WriteShortFileMark(uint count)
		{
			CheckError(TapeDriveFunctions.WriteTapemark(tapeDrive.Handle, 2, count, false));
		}

		/// <summary>
		/// Writes the specified number of set marks to the current position
		/// </summary>
		/// <param name="count">Number of set marks to write</param>
		public void WriteSetMark(uint count)
		{
			CheckError(TapeDriveFunctions.WriteTapemark(tapeDrive.Handle, 0, count, false));
		}

		/// <summary>
		/// Writes an end of data mark at the current position
		/// </summary>
		public void WriteEndOfDataMark()
		{
			CheckError(TapeDriveFunctions.EraseTape(tapeDrive.Handle, 0, false));
		}


		private void CheckError(UInt32 code)
		{
			switch (code)
			{
				case 1102:	// ERROR_BEGINNING_OF_MEDIA
					beginningMediaDetected = true;
					break;
				case 1100:	// ERROR_END_OF_MEDIA
					endMediaDetected = true;
					break;
				case 1103:	// ERROR_SETMARK_DETECTED
					setMarkDetected = true;
					break;
				case 1101:	// ERROR_FILEMARK_DETECTED
					fileMarkDetected = true;
					break;
				case 1104:	// ERROR_NO_DATA_DETECTED
					endOfDataDetected = true;
					break;
				case 19:		// ERROR_WRITE_PROTECT
					throw(new WriteProtectedException());
				default:
					TapeDriveFunctions.CheckGeneralError(code);
					break;
			}
		}
	}
}
