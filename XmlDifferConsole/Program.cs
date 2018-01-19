using clipr;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using XmlDiff;
using XmlDiff.Visitors;

namespace XmlDifferConsole
{
    //[ApplicationInfo(Description = "This is a set of options.")]
    public class Options
    {
        [NamedArgument('v', "verbose",
            Action = ParseAction.StoreTrue,
            //Constraint = NumArgsConstraint.Optional,
            Description = "Increase the verbosity of the output.")]
        public bool Verbose { get; set; }

        [PositionalArgument(0,
            Description = "left input XML file.")]
        public string LeftFile { get; set; }

        [PositionalArgument(1,
                    Description = "right input XML file.")]
        public string RightFile { get; set; }

        [NamedArgument("html", Action = ParseAction.Store,
            //Constraint = NumArgsConstraint.Optional,
            Description = "Output differences to HTML file.")]
        public string OutputHtmlFile { get; set; }

        [NamedArgument("xdt", Action = ParseAction.Store,
            //Constraint = NumArgsConstraint.Optional,
            Description = "Output XDocument Transformation file.")]
        public string OutputXdtFile { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var opt = CliParser.StrictParse<Options>(args);
            var stopwatch = new Stopwatch();

            XDocument leftDoc = null;
            XDocument rightDoc = null;
            if (opt.Verbose)
            {
                Console.WriteLine("Loading \"{0}\"...", opt.LeftFile);
            }
            leftDoc = XDocument.Load(opt.LeftFile);
            if (opt.Verbose)
            {
                Console.WriteLine("Loading \"{0}\"...", opt.RightFile);
            }
            rightDoc = XDocument.Load(opt.RightFile);
            if (opt.Verbose)
            {
                Console.WriteLine("Comparing differences...");
            }
            stopwatch.Start();

            var comparer = new XmlComparer();
            var diff = comparer.Compare(leftDoc.Root, rightDoc.Root);
            var isChanged = diff.IsChanged;

            if (opt.Verbose)
                Console.WriteLine("Compared in {0} ms.", stopwatch.ElapsedMilliseconds);

            if (!string.IsNullOrEmpty(opt.OutputHtmlFile))
            {
                if (opt.Verbose)
                {
                    Console.WriteLine("Creating HTML output...");
                    stopwatch.Restart();
                }

                var visitor = new HtmlVisitor();
                visitor.VisitWithDefaultSettings(diff);

                if (opt.Verbose)
                    Console.WriteLine("Writing HTML output to \"{0}\"...", opt.OutputHtmlFile);
                File.WriteAllText(opt.OutputHtmlFile, visitor.Result);

                if (opt.Verbose)
                    Console.WriteLine("HTML output file created in {0} ms.", stopwatch.ElapsedMilliseconds);
            }

            if (!string.IsNullOrEmpty(opt.OutputXdtFile))
            {
                if (opt.Verbose)
                {
                    Console.WriteLine("Creating XDT output...");
                    stopwatch.Restart();
                }

                var visitor = new XdtVisitor();
                visitor.VisitWithDefaultSettings(diff);

                if (opt.Verbose)
                    Console.WriteLine("Writing XDT output to \"{0}\"...", opt.OutputXdtFile);
                File.WriteAllText(opt.OutputXdtFile, visitor.Result);

                if (opt.Verbose)
                    Console.WriteLine("XDT output file created in {0} ms.", stopwatch.ElapsedMilliseconds);
            }
            stopwatch.Stop();

            if (opt.Verbose)
                Console.WriteLine("\nShowing text diff:");
            if (opt.Verbose || (string.IsNullOrEmpty(opt.OutputHtmlFile) && string.IsNullOrEmpty(opt.OutputXdtFile)))
            {
                var vistor = new ToStringVisitor();
                vistor.VisitWithDefaultSettings(diff);
                Console.WriteLine(vistor.Result);
            }
        }
    }
}
