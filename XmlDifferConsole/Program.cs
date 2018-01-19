using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using XmlDiff;
using XmlDiff.Visitors;

namespace XmlDifferConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: {0} leftFile rightFile [outputHtmlFile]", AppDomain.CurrentDomain.FriendlyName);
                    return;
                }

                var leftFile = args[0];
                var rightFile = args[1];
                var outputHTMLFile = string.Empty;
                if (args.Length > 2)
                    outputHTMLFile = args[2];

                var verbose = true;
                XDocument leftDoc = null;
                XDocument rightDoc = null;
                if (verbose)
                    Console.WriteLine("Loading \"{0}\"...", leftFile);
                leftDoc = XDocument.Load(leftFile);
                if (verbose)
                    Console.WriteLine("Loading \"{0}\"...", rightFile);
                rightDoc = XDocument.Load(rightFile);

                var stopwatch = new Stopwatch();
                if (verbose)
                {
                    Console.WriteLine("Comparing differences...");
                    stopwatch.Start();
                }

                var comparer = new XmlComparer();
                var diff = comparer.Compare(leftDoc.Root, rightDoc.Root);
                var isChanged = diff.IsChanged;

                if (verbose)
                {
                    stopwatch.Stop();
                    Console.WriteLine("Compared in {0} ms.", stopwatch.ElapsedMilliseconds);
                }

                if (!string.IsNullOrEmpty(outputHTMLFile))
                {
                    if (verbose)
                        Console.WriteLine("Creating HTML output...");

                    var visitor = new HtmlVisitor();
                    visitor.VisitWithDefaultSettings(diff);

                    if (verbose)
                        Console.WriteLine("Writing HTML output to \"{0}\"...", outputHTMLFile);
                    File.WriteAllText(outputHTMLFile, visitor.Result);
                    if (verbose)
                        Console.WriteLine("HTML output file created.");
                }

                {
                    var vistor = new ToStringVisitor();
                    vistor.VisitWithDefaultSettings(diff);
                    Console.WriteLine(vistor.Result);
                }
            }
            finally
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }
    }
}
