using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Frontiers;
using ExtensionMethods;

public sealed class CSVFile
{
	private int m_ColumnCount;
	private List<CSVRow> m_Rows;

	public int ColumnCount { get { return m_ColumnCount; } }
	public System.Collections.ObjectModel.ReadOnlyCollection<CSVRow> Rows { get { return m_Rows.AsReadOnly(); } }


	public CSVFile(string path)
	{
		m_Rows = new List<CSVRow>();

		int curCharVal = 0;
		char curChar;
		bool inQuotes = false;
		var curField = new StringBuilder();
		var curRow = new CSVRow();

		try
		{
			using (var sr = new System.IO.StreamReader(path))
			{
				curCharVal = sr.Read();

				while (curCharVal >= 0)
				{
					//We can't be sure if the file we've received uses Line Feed (char 10) by itself, or Carriage Return / Line Feed (chars 13 / char 10) to indicate a new line
					//what we can be sure of, however, is that we really don't care if there's a 13!
					while (curCharVal == 13)
					{
						curCharVal = sr.Read();

						if (curCharVal == -1)
							break;
					}

					//Sanity check, if we ended up with a -1 due to curCharVal == 13 loop...
					//Should never happen, but god knows what some people's CSV files look like
					if (curCharVal == -1)
					{
						curRow.Fields.Add(curField.ToString());
						curField.Clear();
						this.m_Rows.Add(curRow);

						break;
					}

					curChar = (char)curCharVal;

					if (inQuotes)
					{
						//If we're in a quote enclosed field, we need to identify
						//  if these new quotes are escaped (doubled)
						//  and if they are, only add a single set of quotes to our
						//  current field.  If they are not escaped, then we are
						//  no longer in a quote enclosed field
						if (curChar == '"')
						{
							curCharVal = sr.Read();

							if (curCharVal >= 0)
							{
								curChar = (char)curCharVal;

								if (curChar != '"')
								{
									inQuotes = false;

									//The new character we just imported (presumably a comma)
									//  will be handled once we fall through into the next if block below
								}
								else
								{
									curField.Append(curChar);
								}
							}
						}
						else
						{
							curField.Append(curChar);
						}
					}

					//This is a separate if statement, rather than an else clause
					//  because within the if block above, the inQuotes value could be
					//  set to false, in which case we want to evaluate the logic
					//  within this code block
					if(!inQuotes)
					{
						if (curField.Length == 0 && curChar == '"')
						{
							inQuotes = true;
						}
						else if (curChar == ',')
						{
							curRow.Fields.Add(curField.ToString());
							curField.Clear();
						}
						else if (curCharVal == 10)
						{
							curRow.Fields.Add(curField.ToString());
							curField.Clear();

							//We're done with this row, add it to the list and set
							//  ourselves up for a fresh row.
							this.m_Rows.Add(curRow);
							curRow = new CSVRow();
						}
						else
						{
							curField.Append(curChar);
						}
					}


					curCharVal = sr.Read();

					//We just reached the end of the file.
					//  Add the current row to the list of rows before the loop ends
					if (curCharVal == -1)
					{
						curRow.Fields.Add(curField.ToString());
						curField.Clear();
					}
				}
			}
		}
		catch
		{
			m_Rows.Clear();
			m_ColumnCount = 0;
		}
	}


	public sealed class CSVRow
	{
		private List<string> m_Fields;

		public List<string> Fields { get { return m_Fields; } }

		public CSVRow()
		{
			m_Fields = new List<string>();
		}
	}
}