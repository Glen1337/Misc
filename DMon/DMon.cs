using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace Misc
{
    namespace Projects
    {
        class _Main
        {
            private static String Get(String inStr)
            {
                Console.WriteLine(String.Concat(Environment.NewLine, inStr, ": ", Environment.NewLine));
                return Console.ReadLine().Trim().ToUpper();
            }

            static void Main(string[] args)
            {
                string path;
                string timeDelay;
                string useAlerts;
                string degreeOfParallelism;

                // Cmd prompt start up and buffer dimensions set up
                System.Diagnostics.Process.GetCurrentProcess().StartInfo.Verb = "runas";
                Console.BufferHeight = Int16.MaxValue / 10;
                Console.BufferWidth = Int16.MaxValue / 80;
                Console.Title = "DMon";

                // Check arg # and get args if they were supplied from command line
                if (args.Length > 0)
                {
                    if (args.Length != 4) { Console.WriteLine("4 arguments not supplied"); return; }

                    // Parse args from command line
                    path = args[0];
                    timeDelay = args[1];
                    useAlerts = args[2];
                    degreeOfParallelism = args[3];
                }
                else
                {
                    // Gets args from user
                    path = Get("Enter folder path");
                    timeDelay = Get("Enter time delay in seconds");
                    useAlerts = Get("Alert changes with message box? Y/N");
                    degreeOfParallelism = Get("Enter degree of parallelism");

                    Console.WriteLine(Environment.NewLine);
                }

                // run DMon
                DMon monitor = new DMon();
                monitor.Run(path, timeDelay, useAlerts, degreeOfParallelism);
            }
        }

        class DMon
        {
            public void Run(string PathIN, string TimeDelayIN, string UseAlertsIN, string DegreeOfParallelism)
            {
                string _Path;
                int _TimeDelay;
                bool _UseAlerts = false;
                int _DegreeOfParallelism;

                // dummy value
                DateTime dt = new DateTime();

                // Raise process priority as high has possiblw
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

                Console.WriteLine("Running process with priority level: " +
                                  Process.GetCurrentProcess().PriorityClass.ToString() +
                                  Environment.NewLine);

                //Data structures for original and updated file times
                ConcurrentDictionary<String, DateTime> FileTimesOriginal = new ConcurrentDictionary<String, DateTime>();
                ConcurrentDictionary<String, DateTime> FileTimesUpdated = new ConcurrentDictionary<String, DateTime>();

                // Data structures for updating original values so messages dont repeat
                ConcurrentBag<String> KeysToRemove = new ConcurrentBag<String>();
                ConcurrentDictionary<String, DateTime> KeyValuesToAdd = new ConcurrentDictionary<String, DateTime>();
                ConcurrentDictionary<String, DateTime> TimesToUpdate = new ConcurrentDictionary<String, DateTime>();

                // Validate/Convert Inputs

                try
                {
                    _DegreeOfParallelism = Convert.ToInt32(DegreeOfParallelism);
                    Console.WriteLine("Degree of Parallelism: " + _DegreeOfParallelism + Environment.NewLine);
                }
                catch
                {
                    Console.WriteLine("non-valid integer entered for parallelism degree: " + DegreeOfParallelism);
                    return;
                }

                try
                {
                    _TimeDelay = Convert.ToInt32(TimeDelayIN);

                    Console.WriteLine("Time Delay: " +
                                        ((_TimeDelay > 60) ?
                                        (Math.Round((((double)_TimeDelay) / 60D), 4, MidpointRounding.AwayFromZero)
                                               .ToString() +
                                               " minutes") :
                                        (_TimeDelay + " seconds")) +
                                        Environment.NewLine);

                }
                catch (System.Exception se)
                {
                    Console.WriteLine("Invalid time delay entered: " + TimeDelayIN + ". Error: " + se.Message);
                    return;
                }

                if (!Directory.Exists(PathIN))
                {
                    Console.WriteLine("Bad directory: " + PathIN + Environment.NewLine);
                    return;
                }
                else
                {
                    _Path = PathIN;
                    Console.WriteLine("Monitoring folder: " + _Path);
                }

                if (String.Equals("Y", UseAlertsIN))
                {
                    _UseAlerts = true;
                    Console.WriteLine(Environment.NewLine + "Message box alerts turned ON" + Environment.NewLine);
                }
                else { Console.WriteLine(Environment.NewLine + "Message box alerts turned OFF" + Environment.NewLine); }

                Console.WriteLine("Initializing..." + Environment.NewLine);

                // Start timer
                Stopwatch timer = Stopwatch.StartNew();

                // Task getFilesTask = Task.Run(() => {
                Directory.EnumerateFiles(_Path + "\\", "*.*", SearchOption.AllDirectories)
                    .AsParallel<String>()
                    .WithDegreeOfParallelism(_DegreeOfParallelism)
                    .ForAll<String>((s) =>
                    {
                        FileInfo fi = new FileInfo(s);
                        FileTimesOriginal.AddOrUpdate(fi.FullName, fi.LastWriteTime, (okey, oval) => { throw new ApplicationException("repeat key in original file enumeration"); });
                    });
                //});
                // ¯\_(ツ)_/¯
                //getFilesTask.Wait();

                timer.Stop();

                Console.WriteLine(FileTimesOriginal.Count().ToString() +
                                  " files " +
                                  " took ~" +
                                  Math.Round(timer.Elapsed.TotalSeconds, 2).ToString() +
                                  " seconds to enumerate files and add to data structure");

                Console.WriteLine(Environment.NewLine + "Monitoring..." + Environment.NewLine);

                try
                {
                    // Each check every 10 seconds        
                    for (int count = 1; ; count++)
                    {
                        System.Threading.Thread.Sleep(_TimeDelay * 1000);

                        // Write Check # to cmd prompt
                        System.Console.Write("Check #: " + count.ToString(String.Format("000")));

                        // Reset and start timer
                        timer.Reset();
                        timer.Start();

                        // Clear updated file dicitonary
                        FileTimesUpdated = new ConcurrentDictionary<String, DateTime>();

                        // Start getting updated file dictionary here asynchronously
                        Task getCurrentFilesTask = Task.Run(() =>
                        {
                            Directory.EnumerateFiles(_Path, "*.*", SearchOption.AllDirectories)
                               .AsParallel<String>()
                               .WithDegreeOfParallelism(_DegreeOfParallelism)
                               .ForAll<String>((s) =>
                               {
                                   FileInfo fi = new FileInfo(s);
                                   FileTimesUpdated.AddOrUpdate(fi.FullName, fi.LastWriteTime, (okey, oval) =>
                                   {
                                       throw new ApplicationException("error creating current list");
                                   });
                               });
                        });

                        // Do independant work 

                        Parallel.Invoke(

                            // Update original list with updated write times, so messages don't repeat
                            () =>
                            {
                                Parallel.ForEach(TimesToUpdate, kvp =>
                                {
                                    FileTimesOriginal[kvp.Key] = kvp.Value;
                                });
                                TimesToUpdate = new ConcurrentDictionary<String, DateTime>();
                            },

                            // Update original list with added files, so messages don't repeat
                            () =>
                            {
                                Parallel.ForEach(KeyValuesToAdd, kvp =>
                                {
                                    FileTimesOriginal.AddOrUpdate(kvp.Key, kvp.Value, (key, oldval) =>
                                    {
                                        throw new ApplicationException("repeat key in dicitonary update");
                                    });
                                });
                                KeyValuesToAdd = new ConcurrentDictionary<String, DateTime>();
                            },

                            // Update original list with removed files, so messages don't repeat      
                            () =>
                            {
                                Parallel.ForEach(KeysToRemove, key =>
                                {
                                    if (!FileTimesOriginal.TryRemove(key, out dt))
                                    {
                                        throw new ApplicationException("attempt to remove non-existant key");
                                    }
                                });
                                KeysToRemove = new ConcurrentBag<String>();
                            }
                        );

                        // would await here if async method was used was used
                        getCurrentFilesTask.Wait();

                        // Print time taken to get files to console
                        timer.Stop();

                        Console.WriteLine("  (" +
                                          FileTimesUpdated.Count() +
                                          " files | " +
                                          Math.Round(timer.Elapsed.TotalSeconds, 2).ToString() +
                                          " seconds)  " +
                                          DateTime.Now.ToString("M/d/yyyy -- h:mm:ss.fff tt"));

                        Parallel.Invoke(

                            () =>
                            {
                                // Iterate through current files
                                Parallel.ForEach(FileTimesUpdated, kvp =>
                                {
                                    if (FileTimesOriginal.ContainsKey(kvp.Key))
                                    {
                                        if (!FileTimesOriginal[kvp.Key].Equals(kvp.Value))
                                        {
                                            // File was written to
                                            Console.WriteLine("  " + kvp.Key + " was updated: " + kvp.Value.ToString(String.Format("M/d/yyyy h:mm:ss.fffffff tt")));/* + " by " + Shell.GetUsernameHandlingFile(kvp.Key)*/

                                            if (_UseAlerts)
                                            {
                                                MessageBox.Show("  " + kvp.Key + " was updated: " + kvp.Value.ToString("M/d/yyyy h:mm:ss.fffffff tt"));
                                            }

                                            TimesToUpdate.AddOrUpdate(kvp.Key, kvp.Value, delegate(String oldkey, DateTime oldval)
                                            {
                                                throw new ApplicationException("error updating keys to add list on check #: " + count.ToString());
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // File was added
                                        Console.WriteLine("  " + kvp.Key + " was added: " + kvp.Value.ToString("M/d/yyyy h:mm:ss.fffffff tt") /*+ " by " + Shell.GetUsernameHandlingFile(kvp.Key)*/);

                                        if (_UseAlerts)
                                        {
                                            MessageBox.Show("  " + kvp.Key + " was added: " + kvp.Value.ToString("M/d/yyyy h:mm:ss.fffffff tt"));
                                        }

                                        KeyValuesToAdd.AddOrUpdate(kvp.Key, kvp.Value, delegate(String oldkey, DateTime oldval)
                                        {
                                            throw new ApplicationException("error updating keys to add list on check #: " + count.ToString());
                                        });
                                    }
                                });
                            },

                            () =>
                            {
                                // Iterate through original files
                                Parallel.ForEach(FileTimesOriginal, kvp =>
                                {
                                    if (!FileTimesUpdated.ContainsKey(kvp.Key))
                                    {
                                        // File was removed
                                        Console.WriteLine("  " + kvp.Key + " was removed after: " + DateTime.Now.Subtract(new TimeSpan(0, 0, _TimeDelay)).ToString("M/d/yyyy h:mm tt"));

                                        if (_UseAlerts)
                                        {
                                            MessageBox.Show("  " + kvp.Key + " was removed after: " + DateTime.Now.Subtract(new TimeSpan(0, 0, _TimeDelay)).ToString("M/d/yyyy h:mm tt"));
                                        }
                                        KeysToRemove.Add(kvp.Key);
                                    }
                                });
                            }
                        );
                    } // For loop
                } // Try
                catch (System.Exception se)
                {
                    Console.WriteLine("Error: " + se.Message);
                }
                finally
                {
                    Console.WriteLine("Exiting...");
                    Console.ReadKey();
                }
                return;
            }
        }


    }
}
