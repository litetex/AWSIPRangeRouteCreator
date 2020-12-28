using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Serilog.Events;

namespace AWSIPRangeRouteCreator
{
   public class CmdOptions
   {
      [Option("downloadurl", Default = "https://ip-ranges.amazonaws.com/ip-ranges.json", HelpText = "URL used for downloading the infos from aws")]
      public string DownloadUrl { get; set; } = "https://ip-ranges.amazonaws.com/ip-ranges.json";

      [Option('r',"region", Separator = ',', HelpText = "Only generates the routes for speicific regions; e.g: us-east-1,us-west-1")]
      public IEnumerable<string> Regions { get; set; } = null;

      [Option("supernetfullipcheck", HelpText = "Checks if every IP is unique; Much slower but may fix broken aws data")]
      public bool SupernetFullIpCheck { get; set; } = false;

      [Option('f', "outputfileformat", Default = OutputFileFormat.Simple, HelpText = "Output file format")]
      public OutputFileFormat OutputFileFormat { get; set; } = OutputFileFormat.Simple;

      [Option('o', "outputfile", Default = "aws-routes.txt", HelpText = "Outputfile path/name")]
      public string OutputFile { get; set; } = "aws-routes.txt";

      [Option('l', "loglevel", HelpText = "Loglevel")]
      public LogEventLevel? LogEventLevel { get; set; } = null;
   }
}
