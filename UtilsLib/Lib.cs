using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Utils
{
    public static class Lib
    {

        // Custom exception
        public class ExceptionC : System.Exception
        {
            public ExceptionC()
            {
            }

            public ExceptionC(string message)
                : base(message)
            {
            }

            public ExceptionC(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public static String nl = Environment.NewLine;

        public static String n(int num)
        {
            String s = String.Empty;

            if ((num < 1) || (num > 1000)) { throw new ArgumentOutOfRangeException("arg must be between 1 and 1000"); } //{ return s; }

            s = String.Concat(Enumerable.Repeat<String>(Environment.NewLine, num));

            return s;
        }

        // Takes seconds arg, rounds and returns string of minutes if seconds are >60 and seconds if <60
        public static String PrintTimeFromSeconds(this double inSeconds, int places = 2)
        {
            //const int places = 2;

            if (inSeconds <= 0.0) { return "0 seconds"; }

            try
            {
                return (inSeconds < 60) ? (String.Concat((Math.Round(inSeconds, places, MidpointRounding.AwayFromZero)).ToString(), " seconds"))
                                        : (String.Concat((Math.Round((inSeconds / 60.00d), places)).ToString(), " minutes"));
            }
            catch (System.Exception e)
            {
                throw new ArgumentException("Error during calculation", e);
            }
        }

        public static System.Boolean HasNums(this String inString)
        {
            if (String.IsNullOrWhiteSpace(inString)) { return false; }

            String stringNums = new String(inString.ToCharArray().Where(c => char.IsDigit(c)).ToArray());

            return (!String.IsNullOrWhiteSpace(stringNums));
        }

        // Returns true if file is valid, accessible, and has correct file extension
        public static Boolean isFilePathOK(this String inFilePath, String extension)
        {
            try
            {
                return (inFilePath.isFilePathOK() &&
                        String.Equals(System.IO.Path.GetExtension(inFilePath).ToUpper().Trim(),
                                      extension.ToUpper().Trim()));
            }
            catch { return false; }
        }

        // Returns true if file is valid and accessible
        public static Boolean isFilePathOK(this String inFilePath)
        {
            FileInfo fi = null;

            if (String.IsNullOrWhiteSpace(inFilePath)) { return false; }

            try { fi = new FileInfo(inFilePath); }
            catch { return false; }

            if (!File.Exists(inFilePath) || fi == null) { return false; }

            return true;
        }

        // Returns true if directory is a valid and accessible directory
        public static Boolean isDirectoryPathOK(this String inDirectoryPath)
        {
            DirectoryInfo di = null;

            if (String.IsNullOrWhiteSpace(inDirectoryPath)) { return false; }

            try { di = new DirectoryInfo(inDirectoryPath); }
            catch { return false; }

            if (!Directory.Exists(inDirectoryPath) || di == null) { return false; }

            return true;
        }

        // Truncates positive or negative double after given # of decimal places for printing
        public static String truncstring(this double inDouble, int numPlaces = 3)
        {
            if (numPlaces <= 0) { return inDouble.ToString(); }

            try
            {
                checked
                {
                    int numPosDigits = Convert.ToInt32(Math.Floor(Math.Log10(Math.Truncate(Math.Abs(inDouble))) + 1));

                    return inDouble.ToString(String.Concat(String.Concat(Enumerable.Repeat<String>("#", numPosDigits)),
                                                           ".",
                                                           String.Concat(Enumerable.Repeat<String>("0", numPlaces))));
                }
            }
            catch(System.OverflowException o)
            {
                throw new ArgumentOutOfRangeException("overflow exception", o);
            }
            catch (System.Exception) { throw; }

            // https://stackoverflow.com/questions/4483886/how-can-i-get-a-count-of-the-total-number-of-digits-in-a-number
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings
            //Char[] placeArray;
            //placeArray = new Char[numPlaces];
        }

        // Gives percentage value of current page
        public static Int32 GetPercentage(Int32 current, Int32 total)
        {
            try
            {
                //Math.Ceiling(Convert.ToInt32())
                checked
                {
                    if ((current < 1)) { return 0; }
                    if ((current >= total) || (total < 1)) { return 100; }

                    return ((Int32)Math.Round((((double)current / (double)total) * 100), MidpointRounding.AwayFromZero));
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
