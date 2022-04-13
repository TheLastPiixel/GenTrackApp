using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace GenTrackApp
{
    internal class Program
    {
        public enum State
        {
            None,
            Start,
            CreateNewCSV,
            AddLine,
            End
        }

        static void Main(string[] args)
        {
            string headerRow, trailerRow, userInput;
            List<string> listOfCSVs, allCSVData;

            while (true)
            {
                userInput = GetUserInput();
                allCSVData = ExtractData("CSVIntervalData", userInput);
                (headerRow, trailerRow, listOfCSVs) = FSMSortRows(allCSVData);
                WriteCSVFiles(headerRow, trailerRow, listOfCSVs);
            }
        }

        #region Read XML Methods
        //Description: Promps the user and reads the file path input
        public static string GetUserInput()
        {
            Console.WriteLine("Enter the .xml file path: ");
            return Console.ReadLine();
        }

        //Description: Extracts data from inside a specified XML tag from a specified .xml file
        public static List<string> ExtractData(string tagName, string filePath)
        {
            bool insideTag = false;
            string URLString = filePath;
            string data = "";

            try
            {
                XmlTextReader reader = new XmlTextReader(URLString);
                while (reader.Read())
                {
                    if (reader.Name == tagName)
                    {
                        insideTag = !insideTag;
                    }

                    if (insideTag)
                    {
                        data = reader.Value;
                    }
                }
                return ExtractCSVLines(data); ;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"File {filePath} not found!");
                throw e;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"File {filePath} includes illegal characters");
                throw e;
            }
        }

        //Description: Seperates each line of CSV data into individual strings in a list
        public static List<string> ExtractCSVLines(string data)
        {
            List<string> csvData = new List<string>();
            string currentRow = "";
            foreach (char c in data)
            {
                if (Char.Equals(c, (char)13))
                {
                    //Remove line feed character
                    currentRow = currentRow.Replace(Char.ToString((char)10), "");
                    //Check if current row is empty before adding it as a line in csvData
                    if (!String.IsNullOrEmpty(currentRow))
                    {
                        csvData.Add(currentRow);
                    }
                    currentRow = "";
                }
                else
                {
                    currentRow = currentRow + c;
                }
            }
            return csvData;
        }
        #endregion

        #region CSV Data Sorter
        //Description: Uses a finite state machine to sort through each row of csv data
        public static (string, string, List<string>) FSMSortRows(List<string> allCSVLines)
        {
            List<string> listOfCSVs = new List<string>();
            int code = 0;
            string headerRow = "";
            string trailerRow = "";
            string currentCsv = "";
            bool first200Element = true;
            State transition = State.Start;
            State state = State.None;

            for (int i = 0; i < allCSVLines.Count; i++)
            {
                code = ReadFirstElement(allCSVLines[i]);
                switch (transition)
                {
                    case State.Start:
                        if (code == 100)
                        {
                            transition = State.CreateNewCSV;
                            state = State.Start;
                        }
                        break;
                    case State.CreateNewCSV:
                        if (code == 200)
                        {
                            transition = State.AddLine;
                            state = State.CreateNewCSV;
                        }
                        break;
                    case State.AddLine:
                        if (code == 200)
                        {
                            transition = State.AddLine;
                            state = State.CreateNewCSV;
                        }
                        else if (code == 300)
                        {
                            transition = State.AddLine;
                            state = State.AddLine;
                        }
                        else if (code == 900)
                        {
                            transition = State.End;
                            state = State.End;
                        }
                        break;
                    case State.End:
                        break;
                }

                switch (state)
                {
                    case State.Start:
                        headerRow = allCSVLines[i];
                        state = State.None;
                        break;
                    case State.CreateNewCSV:
                        if (first200Element)
                        {
                            first200Element = false;
                        }
                        else
                        {
                            listOfCSVs.Add(currentCsv);
                            currentCsv = "";
                        }
                        currentCsv = currentCsv + allCSVLines[i] + (char)13;
                        state = State.None;
                        break;
                    case State.AddLine:
                        currentCsv = currentCsv + allCSVLines[i] + (char)13;
                        state = State.None;
                        break;
                    case State.End:
                        listOfCSVs.Add(currentCsv);
                        trailerRow = allCSVLines[i];
                        state = State.None;
                        return (headerRow, trailerRow, listOfCSVs);
                        break;
                }
            }
            return (null, null, null);
        }
        #endregion

        #region Write To CSV Methods
        //Description: Writes all the csv data into individual csv files
        public static void WriteCSVFiles(string headerRow, string trailerRow, List<string> listOfCSVs)
        {
            int noOfCSVs = 0;

            foreach (string csvData in listOfCSVs)
            {
                if (WriteToFile(headerRow, trailerRow, csvData)) noOfCSVs++;
            }

            Console.WriteLine($"Successfully created {noOfCSVs} csv files!");
        }

        //Description: Writes the header, trailer, and body of a csv document
        //Output: Boolean to represent a successful or unsuccessful creation of file
        public static bool WriteToFile(string headerRow, string trailerRow, string body)
        {
            string fileName = GetFileName(body);
            try
            {
                // Write the string array to a new file named "WriteLines.txt".
                using (StreamWriter outputFile = new StreamWriter(Path.Combine("csv\\", $"{fileName}.csv")))
                {
                    outputFile.WriteLine(RemoveAllUnwantedCharacters(headerRow));
                    outputFile.WriteLine(RemoveAllUnwantedCharacters(body));
                    outputFile.WriteLine(RemoveAllUnwantedCharacters(trailerRow));
                    return true;
                }
            }
            catch (UnauthorizedAccessException e) 
            {
                Console.WriteLine(e.Message + " - Did you use a valid name for the csv?");
                return false;
            }
        }
        #endregion

        #region Helper Methods
        //Description: Finds the first element number code of a row
        public static int ReadFirstElement(string row)
        {
            try
            {
                if (row.Length >= 3)
                {
                    string threeDigitNumber = row.Substring(0, 3);
                    //Console.WriteLine("Value is: " + threeDigitNumber);
                    return Int32.Parse(threeDigitNumber);
                }
                return 0;
            }
            catch (FormatException e)
            {
                Console.WriteLine("The csv data has an invalid format!");
                throw e;
            }
        }

        //Description: Extracts the file name from the second field in a csv row
        public static string GetFileName(string row)
        {
            bool readingSecondField = false;
            string fileName = "";
            foreach (char c in row)
            {
                if (c == ',')
                {
                    if (!readingSecondField)
                    {
                        readingSecondField = true;
                    }
                    else
                    {
                        return fileName;
                    }
                }
                else if (readingSecondField)
                {
                    fileName = fileName + c;
                }
            }
            return "";
        }

        //Descripion: Find the position of the first and last char in a string and removes "unwanted special characters" from them
        public static string RemoveAllUnwantedCharacters(string data)
        {
            string alteredData = data;
            int lastCharPos = data.Length - 1;
            alteredData = RemoveUnwantedCharacters(0, alteredData);
            alteredData = RemoveUnwantedCharacters(lastCharPos, alteredData);
            return alteredData;
        }

        //Description: Looks for white spaces, new line, and tabs in a specific position of string and removes it
        public static string RemoveUnwantedCharacters(int pos, string data)
        {
            string alteredData = "";
            try
            {
                switch (data[pos])
                {
                    case (char)11:
                    case (char)12:
                    case (char)13:
                    case (char)32:
                        alteredData = data.Remove(pos, 1);
                        break;
                    default:
                        alteredData = data;
                        break;
                }
                return alteredData;
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message + " - Error is trimming whitespace from string");
                throw e;
            }
        }
        #endregion
    }
}
