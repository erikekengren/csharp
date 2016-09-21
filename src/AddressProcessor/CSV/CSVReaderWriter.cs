using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace AddressProcessing.CSV
{
	/*
		2) Refactor this class into clean, elegant, rock-solid & well performing code, without over-engineering.
		   Assume this code is in production and backwards compatibility must be maintained.
	*/

	public class CSVReaderWriter : IDisposable
	{
		#region Variables

		private StreamReader _readerStream = null;
		private StreamWriter _writerStream = null;

		private const int FIRST_COLUMN = 0;
		private const int SECOND_COLUMN = 1;

		private const char SEPARATOR = '\t';

		#endregion

		#region Enums

		public enum Mode { Read = 1, Write = 2 };

		#endregion

		#region Destructor

		~CSVReaderWriter()
		{
			Close();
		}

		#endregion

		#region Open and Close

		public void Open(string fileName, Mode mode)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("Parameter fileName cannot be null or empty");

			if (mode == Mode.Read)
			{
				_readerStream = File.OpenText(fileName);
			}
			else if (mode == Mode.Write)
			{
				FileInfo fileInfo = new FileInfo(fileName);
				_writerStream = fileInfo.CreateText();
			}
			else
			{
				throw new ArgumentException("Unknown file mode for " + fileName);
			}
		}

		public void Close()
		{
			if (_writerStream != null)
			{
				_writerStream.Close();
			}

			if (_readerStream != null)
			{
				_readerStream.Close();
			}
		}

		#endregion

		#region Write

		public void Write(params string[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException("Parameter columns cannot be null");

			if (columns.Length == 0)
				throw new ArgumentException("Parameter array columns contains no items");

			StringBuilder outPut = new StringBuilder();

			// No need to calculate this each iteration
			int lastItem = columns.Length - 1;

			for (int i = 0; i < columns.Length; i++)
			{
				string column = columns[i];

				if (string.IsNullOrWhiteSpace(column))
					throw new ArgumentException("Parameter columns contains empty items");

				outPut.Append(column);

				if (lastItem != i)
				{
					outPut.Append(SEPARATOR);
				}
			}

			WriteLine(outPut.ToString());
		}
		
		#endregion

		#region Read

		// Does nothing except advancing the reader to the next line and checking that there are columns
		public bool Read(string column1, string column2)
		{
			string line;
			string[] columns;

			line = ReadLine();
			columns = line.Split(SEPARATOR);

			return (columns.Length != 0);
		}

		public bool Read(out string column1, out string column2)
		{
			if (_readerStream.EndOfStream)
			{
				column1 = null;
				column2 = null;

				return false;
			}

			string line;

			line = ReadLine();

			if (string.IsNullOrWhiteSpace(line))
			{
				column1 = null;
				column2 = null;

				return false;
			}

			// No point creating the array until the line has been checked
			string[] columns;

			columns = line.Split(SEPARATOR);

			if (columns.Length < 2)
			{
				column1 = null;
				column2 = null;

				return false;
			}
			else
			{
				column1 = columns[FIRST_COLUMN];
				column2 = columns[SECOND_COLUMN];

				return true;
			}
		}
		
		#endregion

		#region Private Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteLine(string line)
		{
			_writerStream.WriteLine(line);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private string ReadLine()
		{
			return _readerStream.ReadLine();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			Close();
		}
		
		#endregion
	}
}
