using Antlr4.StringTemplate;
using CommandLine;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Salmon
{
    class Program
    {
        static void Main(string[] args)
        {
            // test

            //コマンドライン引数を解析する
            //ParserResult<Options> result = CommandLine.Parser.Default.ParseArguments<Options>(() => new Options(), args);
            //ParserResult<GenOptions> result = CommandLine.Parser.Default.ParseArguments<GenOptions>(args);
            //if (result.Tag == ParserResultType.Parsed)
            //{
            //    GenOptions opts = ((CommandLine.Parsed<GenOptions>)result).Value;
            //    //解析に成功した時は、解析結果を表示
            //    Console.WriteLine("OutputFile: {0}", opts.OutputFile);
            //    Console.WriteLine("Overwrite: {0}", opts.Overwrite);

            //    foreach(var s in opts.InputFiles)
            //    {
            //        Console.WriteLine("InputFiles: {0}", s);
            //    }

            //    Array.ForEach(opts.InputFiles.ToArray(), s => Console.WriteLine("InputFiles: {0}", s));


            //}
            //else
            //{
            //    //解析に失敗
            //    Console.WriteLine("コマンドライン引数の解析に失敗");
            //}

            CommandLine.Parser.Default.ParseArguments<GenOptions, StGenOptions, ImpOptions, SepOptions>(args)
                .MapResult(
                    (GenOptions opts) => RunGen(opts),
                    (StGenOptions opts) => RunStGen(opts),
                    (ImpOptions opts) => RunImp(opts),
                    (SepOptions opts) => RunSep(opts),
                    errs => 1
                );

            Console.ReadLine();

        }

        static int RunGen(GenOptions opts)
        {
            Console.WriteLine("Generate snippets.");

            Console.WriteLine("Source: {0}", opts.InputFile);
            Console.WriteLine("Template: {0}", opts.Template);
            Console.WriteLine("OutputFile: {0}", opts.OutputFile);

            if (opts.InputFile == null || opts.Template == null || opts.OutputFile == null)
            {
                Console.WriteLine("Error. needs Source, Template and OutputFile.");
                return -1;
            }

            try
            {

                // Read the template file
                string template = "";
                using (StreamReader stReader = new StreamReader(opts.Template, Encoding.GetEncoding("Shift_JIS")))
                {
                    template = stReader.ReadToEnd();
                }


                using (StreamReader stReader = new StreamReader(opts.InputFile, Encoding.GetEncoding("Shift_JIS")))
                {
                    using (CsvReader csvReader = new CsvReader(stReader))
                    {

                        var records = csvReader.GetRecords<dynamic>();
                        foreach (ExpandoObject record in records)
                        {
                            // content to output
                            string outputContent = template;
                            // output filename
                            string outputfile = opts.OutputFile;
                            foreach (var kv in record.ToList())
                            {
                                Console.WriteLine("{0}:{1}", kv.Key, kv.Value);
                                string key = $"@@@{kv.Key}@@@";
                                string value = kv.Value.ToString();
                                outputContent = outputContent.Replace(key, value);
                                outputfile = outputfile.Replace(key, value);
                            }
                            Console.WriteLine("{0}", outputContent);

                            // Write an output

                            using (StreamWriter writer = new StreamWriter(outputfile, false, Encoding.GetEncoding("Shift_JIS")))
                            {
                                writer.Write(outputContent);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return -1;
            }

            return 0;
        }

        static int RunStGen(StGenOptions opts)
        {
            Console.WriteLine("Generate snippets (using StringTemplate)");

            Console.WriteLine("Source: {0}", opts.InputFile);
            Console.WriteLine("Template: {0}", opts.Template);
            Console.WriteLine("OutputFile: {0}", opts.OutputFile);

            if (opts.InputFile == null || opts.Template == null || opts.OutputFile == null)
            {
                Console.WriteLine("Error. needs Source, Template and OutputFile.");
                return -1;
            }

            try
            {

                // Read the template file
                string templateStr = "";
                using (StreamReader stReader = new StreamReader(opts.Template, Encoding.GetEncoding("Shift_JIS")))
                {
                    templateStr = stReader.ReadToEnd();
                }


                using (StreamReader stReader = new StreamReader(opts.InputFile, Encoding.GetEncoding("Shift_JIS")))
                {
                    using (CsvReader csvReader = new CsvReader(stReader))
                    {

                        var records = csvReader.GetRecords<dynamic>();
                        foreach (ExpandoObject record in records)
                        {
                            //// content to output
                            //string outputContent = template;
                            //// output filename
                            //string outputfile = opts.OutputFile;
                            //foreach (var kv in record.ToList())
                            //{
                            //    Console.WriteLine("{0}:{1}", kv.Key, kv.Value);
                            //    string key = $"@@@{kv.Key}@@@";
                            //    string value = kv.Value.ToString();
                            //    outputContent = outputContent.Replace(key, value);
                            //    outputfile = outputfile.Replace(key, value);
                            //}
                            //Console.WriteLine("{0}", outputContent);

                            // content to output
                            //string outputContent = template;
                            Template template = null;
                            TemplateGroup tg = null;

                            if (!string.IsNullOrEmpty(opts.GetInstanceOf))
                            {
                                // template group
                                if (!string.IsNullOrEmpty(opts.DelimiterStartChar)
                                    && !string.IsNullOrEmpty(opts.DelimiterStopChar))
                                {
                                    tg = new TemplateGroupString(
                                        "[string]",
                                        templateStr,
                                        opts.DelimiterStartChar[0],
                                        opts.DelimiterStopChar[0]);
                                }
                                else
                                {
                                    tg = new TemplateGroupString(templateStr);
                                }

                                template = tg.GetInstanceOf(opts.GetInstanceOf);

                            }
                            else
                            {
                                // template
                                template = new Template(templateStr);
                            }


                            Template tempOutputFile = new Template(opts.OutputFile);

                            
                            foreach (var kv in record.ToList())
                            {
                                Console.WriteLine("{0}:{1}", kv.Key, kv.Value);
                                string key = $"{kv.Key}";
                                object value = kv.Value;
                                template.Add(key, value);
                                tempOutputFile.Add(key, value);  
                                //outputfile = outputfile.Replace(key, value.ToString());
                            }
                            Console.WriteLine($"content:{template.Render()}");
                            Console.WriteLine($"outputFile:{tempOutputFile.Render()}");

                            // output filename
                            string outputfile = tempOutputFile.Render();

                            // Write an output

                            using (StreamWriter writer = new StreamWriter(outputfile, false, Encoding.GetEncoding("Shift_JIS")))
                            {
                                writer.Write(template.Render());
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return -1;
            }

            return 0;
        }


        static int RunImp(ImpOptions opts)
        {
            Console.WriteLine("Import snippets into files.");

            if (opts.Template == null)
            {
                Console.WriteLine("Error. needs Template.");
                return -1;
            }

            string snippetRoot = ".";

            if (opts.SnippetRoot != null)
            {
                snippetRoot = opts.SnippetRoot;
            }

            string separator = "###";

            if (opts.Separator != null)
            {
                separator = opts.Separator;
            }

            // check if shippetRoot exists
            if (!Directory.Exists(snippetRoot))
            {
                Console.WriteLine($"Error. Directory {snippetRoot} doesn't exist.");
                return -1;
            }

            try
            {
                // read template file
                List<string> templateLines = new List<string>();
                using (StreamReader sr = new StreamReader(
                       opts.Template, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        templateLines.Add(line);
                    }
                }

                List<Mark> marks = new List<Mark>();

                Regex beginRegex = new Regex(separator + @" +begin +([a-zA-Z0-9]+) +" + separator);
                Regex endRegex = new Regex(separator + @" +end +([a-zA-Z0-9_-]+) +" + separator);
                Regex importRegex = new Regex(separator + @" +import +([a-zA-Z0-9_-]+) +" + separator);

                int lineat = 0;
                int beginat = -1;
                string name = null;

                foreach (string line in templateLines)
                {
                    //extract begin marks("begin PARTNAME", PARTNAME is [a-zA-Z0-9_-]+)
                    MatchCollection mcBegin = beginRegex.Matches(line);
                    MatchCollection mcEnd = endRegex.Matches(line);
                    MatchCollection mcImport = importRegex.Matches(line);

                    if (mcBegin.Count == 1 && mcBegin[0].Groups.Count == 2 && mcBegin[0].Groups[1].Captures.Count == 1)
                    {
                        //begins.Add(mcBegin[0].Groups[1].Captures[0].Value, lineat);
                        if (beginat >= 0)
                        {
                            Console.WriteLine("error. Found another begin mark after one.");
                            return -1;
                        }

                        name = mcBegin[0].Groups[1].Captures[0].Value;
                        beginat = lineat;
                    }

                    if (mcEnd.Count == 1 && mcEnd[0].Groups.Count == 2 && mcEnd[0].Groups[1].Captures.Count == 1)
                    {
                        //ends.Add(mcEnd[0].Groups[1].Captures[0].Value, lineat);
                        if (beginat >= 0)
                        {
                            string endname = mcEnd[0].Groups[1].Captures[0].Value;
                            if (name == endname)
                            {
                                marks.Add(new Mark(name, beginat, lineat));
                                beginat = -1;
                                name = null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("error. Found an end mark without a begin mark.");
                            return -1;
                        }
                    }


                    if (mcImport.Count == 1 && mcImport[0].Groups.Count == 2 && mcImport[0].Groups[1].Captures.Count == 1)
                    {
                        //imports.Add(mcImport[0].Groups[1].Captures[0].Value, lineat);
                        string importname = mcImport[0].Groups[1].Captures[0].Value;
                        marks.Add(new Mark(importname, lineat));
                    }


                    lineat = lineat + 1;
                }

                foreach (var mark in marks)
                {
                    Console.WriteLine(mark);
                }

                Dictionary<string, string> snippetContents = new Dictionary<string, string>();

                foreach (var partname in marks.Select(x => x.name))
                {
                    // find the file that has the same name that partname shows.
                    var txtFiles = Directory.EnumerateFiles(snippetRoot, partname + ".txt");

                    if (txtFiles.Count() == 0)
                    {
                        continue;
                    }

                    string path = txtFiles.ToList()[0];

                    string content = File.ReadAllText(path, Encoding.GetEncoding("Shift_JIS"));
                    if (!snippetContents.ContainsKey(partname))
                    {
                        snippetContents[partname] = content;
                    }
                }

                Console.WriteLine($"Read {snippetContents.Count()} snippets");

                // replace contents on template to ones of snippets
                List<string> resultLines = new List<string>();
                lineat = 0;
                for (lineat = 0; lineat < templateLines.Count(); lineat++)
                {

                    var mark = marks.Where(x => (x.isimport == false && x.beginat == lineat)
                                             || (x.isimport == true &&  x.importat == lineat)).FirstOrDefault();
                    if (mark != null && mark.isimport == false)
                    {
                        resultLines.Add(templateLines[lineat]);
                        // use snippet content and skip template line until end

                        resultLines.Add(snippetContents[mark.name]);

                        resultLines.Add(templateLines[mark.endat]);
                        lineat = mark.endat;
                    }
                    else if (mark != null && mark.isimport == true)
                    {
                        string importLine = templateLines[lineat];
                        string beginLine = importLine.Replace("import", "begin");
                        string endLine = importLine.Replace("import", "end");

                        // start with beginLine
                        resultLines.Add(beginLine);

                        resultLines.Add(snippetContents[mark.name]);

                        // end with endLine
                        resultLines.Add(endLine);

                    }
                    else
                    {
                        resultLines.Add(templateLines[lineat]);
                    }
                }

                // output result into file or console
                if (opts.OutputFile != null)
                {

                    using (StreamWriter writer = new StreamWriter(opts.OutputFile, false, Encoding.GetEncoding("Shift_JIS")))
                    {
                        Array.ForEach(resultLines.ToArray(), x => writer.WriteLine(x));
                    }
                }
                else
                {
                    Array.ForEach(resultLines.ToArray(), x => Console.WriteLine(x));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return -1;
            }


            return 0;
        }

        static int RunSep(SepOptions opts)
        {
            Console.WriteLine("Separate snippets from files.");

            if (opts.InputFile == null)
            {
                Console.WriteLine("Error. needs InputFile.");
                return -1;
            }

            string separator = "###";

            if (opts.Separator != null)
            {
                separator = opts.Separator;
            }

            try
            {
                //ファイルを読み込み
                List<string> lines = new List<string>();
                using (StreamReader sr = new StreamReader(
                       opts.InputFile, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }

                //foreach (string line in lines)
                //{
                //    Console.WriteLine(line);
                //}

                Dictionary<string, int> begins = new Dictionary<string, int>();
                Dictionary<string, int> ends = new Dictionary<string, int>();

                //開始マーク(begin パート名、パート名は[a-zA-Z0-9_-]+)の検出
                Regex beginRegex = new Regex(separator + @" +begin +([a-zA-Z0-9]+) +" + separator);
                Regex endRegex   = new Regex(separator + @" +end +([a-zA-Z0-9_-]+) +" + separator);

                int lineat = 0;
                foreach (string line in lines)
                {
                    MatchCollection mc = beginRegex.Matches(line);

                    if (mc.Count == 1 && mc[0].Groups.Count == 2 && mc[0].Groups[1].Captures.Count == 1)
                    {
                        begins.Add(mc[0].Groups[1].Captures[0].Value, lineat);
                    }

                    lineat = lineat + 1;
                }

                //終了マーク(end パート名)の検出
                lineat = 0;
                foreach (string line in lines)
                {
                    MatchCollection mc = endRegex.Matches(line);

                    if (mc.Count == 1 && mc[0].Groups.Count == 2 && mc[0].Groups[1].Captures.Count == 1)
                    {
                        ends.Add(mc[0].Groups[1].Captures[0].Value, lineat);
                    }

                    lineat = lineat + 1;
                }

                Console.WriteLine("begins.Count:" + begins.Count);
                Console.WriteLine("ends.Count:" + begins.Count);

                //開始マークと終了マークがそろっていることを確認して
                foreach (string key in begins.Keys)
                {
                    if (ends.ContainsKey(key))
                    {
                        int beginat = begins[key];
                        int endat = ends[key];

                        string filename = key + ".txt";

                        Console.WriteLine($"output {filename}");

                        //パート名.txtで出力
                        using (StreamWriter sw = new StreamWriter(
                            filename,
                            false,
                            Encoding.GetEncoding("shift_jis")))
                        {
                            for (int i = 0; i < lines.Count; i++)
                            {
                                if (beginat < i && i < endat)
                                {
                                    sw.WriteLine(lines[i]);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return -1;
            }



            return 0;
        }

    }

    /// <summary>
    /// Mark has a pair of "begin" and "end" or "import"
    /// </summary>
    public class Mark
    {
        public string name { get; private set; }
        public int beginat { get; private set; }
        public int endat { get; private set; }
        public int importat { get; private set; }
        public bool isimport { get; private set; }

        public Mark(string name, int beginat, int endat)
        {
            this.isimport = false;

            this.name = name;
            this.beginat = beginat;
            this.endat = endat;
        }

        public Mark(string name, int importat)
        {
            this.isimport = true;

            this.name = name;
            this.importat = importat;
        }

        public override string ToString()
        {
            return $"name:{name} beginat:{this.beginat} endat:{this.endat} importat:{this.importat}";
        }
    }

    // options classes

    public abstract class Options
    {
        [CommandLine.Option('v')]
        public bool Verbose { get; set; }
    }

    /// <summary>
    /// Options for the generator
    /// </summary>
    [Verb("gen", HelpText = "Generate codes.")]
    public class GenOptions : Options
    {
        [CommandLine.Option('i')]
        public string InputFile { get; set; }

        [CommandLine.Option('t')]
        public string Template { get; set; }

        [CommandLine.Option('o')]
        public string OutputFile { get; set; }
    }

    /// <summary>
    /// Options for the generator using StringTemplate
    /// </summary>
    [Verb("stgen", HelpText = "Generate codes using StringTemplate.")]
    public class StGenOptions : Options
    {
        [CommandLine.Option('i')]
        public string InputFile { get; set; }

        [CommandLine.Option('t')]
        public string Template { get; set; }

        [CommandLine.Option('g', HelpText = "Assume the template as a template group and get an instance of template from the group.")]
        public string GetInstanceOf { get; set; }

        [CommandLine.Option('s')]
        public string DelimiterStartChar { get; set; }

        [CommandLine.Option('e')]
        public string DelimiterStopChar { get; set; }

        [CommandLine.Option('a', HelpText = "put all records into a template at once")]
        public bool AllRecords { get; set; }


        [CommandLine.Option('o')]
        public string OutputFile { get; set; }
    }

    /// <summary>
    /// Options for the importer
    /// </summary>
    [Verb("imp", HelpText = "Import snippet into templates.")]
    public class ImpOptions : Options
    {
        [CommandLine.Option('t', HelpText = "template")]
        public string Template { get; set; }

        [CommandLine.Option('r', HelpText = "snippet's root dir")]
        public string SnippetRoot { get; set; }

        [CommandLine.Option('s', HelpText = "separator")]
        public string Separator { get; set; }

        [CommandLine.Option('o', HelpText = "output file")]
        public string OutputFile { get; set; }

        ////Boolean型のオプション
        //[CommandLine.Option('v')]
        //public bool Overwrite
        //{
        //    get;
        //    set;
        //}

        ////オプション名がない値のリスト
        //[CommandLine.Value(1)]
        //public IEnumerable<string> InputFiles
        //{
        //    get;
        //    set;
        //}
    }

    /// <summary>
    /// Options for the seperater
    /// </summary>
    [Verb("sep", HelpText = "Separete snippets from files.")]
    public class SepOptions : Options
    {
        [CommandLine.Option('i')]
        public string InputFile { get; set; }

        [CommandLine.Option('s')]
        public string Separator { get; set; }
    }
}
