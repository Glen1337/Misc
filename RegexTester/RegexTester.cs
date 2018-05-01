using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Utils;

namespace Misc
{
    namespace Projects
    {

        class _Main
        {
            static void Main(string[] args)
            {
                //double a = 32.2343;
                //double b = -435.564;
                //String s1 = a.truncstring(2);
                //String s2 = b.truncstring(2);

                // Test which strings match regular expression in parallel
                //RegexTester rt = new RegexTester(@"^DESIGNATIONS WITH\s\S*$");
                RegexTester rt = new RegexTester(@"(?<=^(\d){5,7}-)\S*$");

                rt.RunTests();
            }
        }

        class RegexTester
        {
            List<String> tests = new List<String>
            {
                // Add text tests to match to regex here
                @"123456-f5",
                @"123456-f",
                @"123456-5",
                @"123456-32A",
                @"123456-55",

            };

            private readonly Regex MainRegex;

            // Ctor
            public RegexTester(String inRegex)
            {
                MainRegex = new Regex(inRegex);
            }

            public void RunTests()
            {
                System.Diagnostics.Debug.WriteLine("--------------");

                Parallel.ForEach(tests,
                                 delegate(String currentText)
                                 {
                                    if (MainRegex.IsMatch(currentText))
                                    {
                                        System.Diagnostics.Debug.WriteLine("Regex Matches: " + MainRegex.Match(currentText).Value + " in: " + currentText);
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("No Match for: " + currentText);
                                    }
                                 });
                
                System.Diagnostics.Debug.WriteLine("--------------");

            }
        }

    }
}
