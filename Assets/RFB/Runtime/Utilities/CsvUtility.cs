using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace RFB.Utilities
{
	public static class CsvUtility
	{
		// Log
		public static void Log(string log, LogType type = LogType.Log)
		{
			LogUtility.Log(log, "Csv Utility", type);
		}

		// Decode
		public static Dictionary<string, string>[] Decode(string csvString, bool firstRowHeader)
		{
			// Get all rows
			List<List<string>> rows = DecodeRows(csvString);
			if (rows == null)
			{
				return null;
			}

			// Dictionaries
			List<Dictionary<string, string>> dictionaries = new List<Dictionary<string, string>>();

			// First row header
			if (firstRowHeader)
			{
				// Get keys
				List<string> keys = rows[0];

				// Iterate rows
				for (int r = 1; r < rows.Count; r++)
				{
					// Get row columns
					List<string> columns = rows[r];

					// Get new dictionary
					Dictionary<string, string> dict = new Dictionary<string, string>();

					// Iterate keys
					for (int k = 0; k < keys.Count; k++)
					{
						// Get key
						string key = keys[k];
						if (string.IsNullOrEmpty(key))
						{
							continue;
						}

						// Get value
						string value = columns == null || k >= columns.Count ? "" : columns[k];

						// Set
						dict[key] = value;
					}

					// Add to list
					dictionaries.Add(dict);
				}
			}
			// First column header
			else
			{
				// Iterate rows
				for (int r = 0; r < rows.Count; r++)
				{
					// Get row columns
					List<string> columns = rows[r];

					// Skip without two or more columns
					if (columns.Count <= 1)
					{
						continue;
					}

					// Skip with null key
					string key = columns[0];
					if (string.IsNullOrEmpty(key))
					{
						continue;
					}

					// Iterate columns
					for (int c = 1; c < columns.Count; c++)
					{
						// Ensure dictionary exists
						int d = c - 1;
						if (dictionaries.Count <= d)
						{
							dictionaries.Add(new Dictionary<string, string>());
						}

						// Get dictionary
						Dictionary<string, string> dict = dictionaries[d];

						// Get value
						string value = columns[c];

						// Set
						dict[key] = value;

						// Apply
						dictionaries[d] = dict;
					}
				}
			}

			// Return dictionaries
			return dictionaries.ToArray();
		}

		// Decodes a list of rows
		public static List<List<string>> DecodeRows(string csvString)
		{
			// Skip invalid
			if (string.IsNullOrEmpty(csvString))
			{
				Log("Decode Failed\nReason: Empty string", LogType.Error);
				return null;
			}

			// List of column lists
			List<List<string>> result = new List<List<string>>();

			// Decode while rows exist
			int index = 0;
			while (index < csvString.Length)
			{
				List<string> columns = DecodeLine(ref index, csvString);
				result.Add(columns);
			}

			// Return list
			return result;
		}

		// Decode a single line
		public static List<string> DecodeLine(string csvString)
		{
			int index = 0;
			return DecodeLine(ref index, csvString);
		}

		// Decode the following line
		public static List<string> DecodeLine(ref int startIndex, string csvString)
		{
			// Whether currently in quote
			bool isQuote = false;
			// Whether line is complete
			bool isComplete = false;
			// The current parameter
			string parameter = "";
			// The resultant parameters of this csv line
			List<string> parameters = new List<string>();

			// Iterate
			while (!isComplete && startIndex < csvString.Length)
			{
				// Get character
				char c = csvString[startIndex];
				startIndex++;

				// Quote
				if (c == '\"')
				{
					// Begin quote
					if (!isQuote)
					{
						isQuote = true;
						continue;
					}
					// End quote
					else if (isQuote && startIndex - 2 > 0 && csvString[startIndex - 2] != '\\')
					{
						isQuote = false;
						continue;
					}
				}
				// Not quote
				else if (!isQuote)
				{
					if (c == '\n')
					{
						isComplete = true;
						continue;
					}
					else if (c == ',')
					{
						parameters.Add(parameter);
						parameter = "";
						continue;
					}
					// Skip
					else if (c == '\r')
					{
						continue;
					}
				}

				// Add character
				parameter += c;
			}

			// Add last parameter
			if (!string.IsNullOrEmpty(parameter))
			{
				parameters.Add(parameter);
			}

			// Return
			return parameters;
		}

		#region DECODE
		// Data field
		private const string KEY_DATA_FIELD = "additionalData";

		// Decode data into array
		public static T[] Decode<T>(Dictionary<string, string>[] dictionaries)
		{
			// Get list
			List<T> list = new List<T>();

			// Empty
			if (dictionaries == null)
			{
				return list.ToArray();
			}

			// Get log
			string log = "";

			// Iterate
			for (int i = 0; i < dictionaries.Length; i++)
			{
				// Get dictionary
				Dictionary<string, string> dictionary = dictionaries[i];
				// Parse Dictionary
				T item = Decode<T>(i, dictionary, ref log);
				// Add
				list.Add(item);
			}

			// Log
			if (!string.IsNullOrEmpty(log))
			{
				Log(typeof(T).ToString() + " Array - Parse Warnings\n" + log, LogType.Error);
			}

			// Return list
			return list.ToArray();
		}
		// Iterate
		private static T Decode<T>(int row, Dictionary<string, string> fileDict, ref string log)
		{
			// Generate
			T d = default(T);
			object o = d;

			// Get type
			Type safeType = typeof(T);

			// Add additional data if found
			FieldInfo dataField = safeType.GetField(KEY_DATA_FIELD);
			if (dataField != null && dataField.FieldType == typeof(Dictionary<string, string>))
			{
				dataField.SetValue(o, fileDict);
			}

			// Check each key
			foreach (string key in fileDict.Keys)
			{
				// Value
				string val = fileDict[key];

				// Remove space, Remove /, Lowercase first letter
				string safeKey = key.Replace(" ", "").Replace("/", "");
				safeKey = safeKey.Substring(0, 1).ToLower() + safeKey.Substring(1);

				// Check for matching var
				FieldInfo f = safeType.GetField(safeKey);

				// Null
				if (f == null)
				{
					log += "\n" + row.ToString("000") + ": Field Missing: " + safeKey;
				}
				// String
				else if (f.FieldType == typeof(string))
				{
					f.SetValue(o, val);
				}
				// String Array (Split by ,)
				else if (f.FieldType == typeof(string[]))
				{
					string[] v = val.Split(',');
					f.SetValue(o, v);
				}
				// Date Time
				else if (f.FieldType == typeof(DateTime))
				{
					DateTime v;
					if (DateTime.TryParse(val, out v))
					{
						f.SetValue(o, v);
					}
					else
					{
						f.SetValue(o, DateTime.MaxValue);
						if (!string.IsNullOrEmpty(val))
						{
							log += "\n" + row.ToString("000") + ": Field Cast DateTime Failed: " + safeKey + " (" + val + ")";
						}
					}
				}
				// Boolean
				else if (f.FieldType == typeof(bool))
				{
					f.SetValue(o, val.Equals("true", StringComparison.CurrentCultureIgnoreCase));
				}
				// Integer
				else if (f.FieldType == typeof(int))
				{
					int v;
					if (int.TryParse(val, out v))
					{
						f.SetValue(o, v);
					}
					else
					{
						log += "\n" + row.ToString("000") + ": Field Cast Int Failed: " + safeKey + " (" + val + ")";
					}
				}
				// Float
				else if (f.FieldType == typeof(float))
				{
					float v;
					if (float.TryParse(val, out v))
					{
						f.SetValue(o, v);
					}
					else
					{
						log += "\n" + row.ToString("000") + ": Field Cast Float Failed: " + safeKey + " (" + val + ")";
					}
				}
				// Enum
				else if (f.FieldType.IsEnum)
				{
					try
					{
						object v = Enum.Parse(f.FieldType, val);
						f.SetValue(o, v);
					}
					catch (Exception e)
					{
						log += "\n" + row.ToString("000") + ": " + f.FieldType.ToString() + " Enum Cast Failed: " + safeKey + " (" + val + ")" + " (Error" + e.Message + ")";
					}
				}
				// Mismatch
				else
				{
					log += "\n" + row.ToString("000") + ": Field Mismatch: " + safeKey + " (" + f.FieldType.ToString() + ")";
				}
			}

			// Return val
			return (T)o;
		}
		#endregion
	}
}