using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AddressProcessing.CSV;
using System.IO;

namespace Csv.Tests
{
	[TestFixture]
	public class CSVReaderWriterTests
	{
		private List<string> _tempFiles = new List<string>();

		[TestFixtureTearDown]
		public void TestCleanup()
		{
			foreach (string tempFile in _tempFiles)
			{
				try
				{
					if (File.Exists(tempFile))
						File.Delete(tempFile);
				}
				catch { }
			}
		}


		#region Open

		[Test]
		[Category("Pass")]
		public void Should_throw_on_invalid_open_filemode()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();
			
			Assert.Throws<ArgumentException>(() => { csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write | CSVReaderWriter.Mode.Read); });
		}

		[Test]
		[Category("Pass")]
		public void Should_create_file_on_open_for_write()
		{
			string fileName = GetTempFilePath();


			Assert.True(!File.Exists(fileName));


			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);


			Assert.True(File.Exists(fileName));
		}

		#endregion

		#region Write

		[Test]
		[Category("Pass")]
		public void Should_write_columns()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);

			csvReaderWriter.Write("COL 1", "column 2", "Test");

			csvReaderWriter.Close();

			string contents = string.Empty;

			using (FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader reader = new StreamReader(inputStream))
				{
					contents = reader.ReadToEnd();
				}
			}

			Assert.IsNotNullOrEmpty(contents);

			//strip newline
			contents = contents.Replace(Environment.NewLine, string.Empty);


			string[] columnsFromFile = contents.Split('\t');

			Assert.AreEqual(3, columnsFromFile.Length);


			Assert.AreEqual("COL 1", columnsFromFile[0]);

			Assert.AreEqual("column 2", columnsFromFile[1]);

			Assert.AreEqual("Test", columnsFromFile[2]);
		}

		[Test]
		[Category("Pass")]
		public void Should_throw_on_write_columns_with_no_items()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);

			Assert.Throws<ArgumentException>(() => { csvReaderWriter.Write(new string[] { }); });
		}

		[Test]
		[Category("Pass")]
		public void Should_throw_on_write_null_columns()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);

			Assert.Throws<ArgumentNullException>(() => { csvReaderWriter.Write(null); });
		}

		[Test]
		[Category("Pass")]
		public void Should_throw_on_write_columns_with_empty()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);

			Assert.Throws<ArgumentException>(() => { csvReaderWriter.Write("COL 1", "", "Test"); });
		}

		#endregion

		#region Read

		[Test]
		[Category("Pass")]
		public void Should_fail_read_columns_from_file_without_out()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();


			using (FileStream outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using (StreamWriter writer = new StreamWriter(outputStream))
				{
					writer.WriteLine("ITEM 1\titeM 2");
				}
			} 


			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Read);

			string column1 = string.Empty;
			string column2 = string.Empty;


			Assert.True(csvReaderWriter.Read(column1, column2));

			Assert.AreEqual(string.Empty, column1);

			Assert.AreEqual(string.Empty, column2);
		}

		[Test]
		[Category("Pass")]
		public void Should_read_columns_from_file()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();


			using (FileStream outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using (StreamWriter writer = new StreamWriter(outputStream))
				{
					writer.WriteLine("ITEM 1\titeM 2");
				}
			}


			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Read);

			string column1 = string.Empty;
			string column2 = string.Empty;


			Assert.True(csvReaderWriter.Read(out column1, out column2));

			Assert.AreEqual("ITEM 1", column1);

			Assert.AreEqual("iteM 2", column2);
		}

		#endregion

		#region Performance

		[Test]
		[Category("Performance")]
		public void Should_write_columns_faster_with_updates()
		{
			CSVReaderWriter csvReaderWriter = new CSVReaderWriter();

			string fileName = GetTempFilePath();

			csvReaderWriter.Open(fileName, CSVReaderWriter.Mode.Write);

			DateTime startTime = DateTime.Now;
			for (int i = 0; i < 10000; i++)
			{
				csvReaderWriter.Write("COL 1", "column 2", "Test");
			}
			DateTime endTime = DateTime.Now;

			csvReaderWriter.Close();


			TimeSpan updatedDuration = endTime.Subtract(startTime);



			CSVReaderWriterForAnnotation oldCsvReaderWriter = new CSVReaderWriterForAnnotation();

			fileName = GetTempFilePath();

			oldCsvReaderWriter.Open(fileName, CSVReaderWriterForAnnotation.Mode.Write);

			startTime = DateTime.Now;
			for (int i = 0; i < 100000; i++)
			{
				oldCsvReaderWriter.Write("COL 1", "column 2", "Test");
			}
			endTime = DateTime.Now;

			oldCsvReaderWriter.Close();


			TimeSpan oldDuration = endTime.Subtract(startTime);


			string newTime = updatedDuration.ToString("c");
			string oldTime = oldDuration.ToString("c");

			System.Diagnostics.Debug.Write(newTime);
			System.Diagnostics.Debug.Write(oldTime);

			Assert.Less(updatedDuration.Ticks, oldDuration.Ticks);
		}

		#endregion

		#region Private Medthods

		private string GetTempFilePath()
		{
			string fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			_tempFiles.Add(fileName);

			return fileName;
		}

		#endregion
	}
}
