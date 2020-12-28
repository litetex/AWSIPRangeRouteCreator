using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Serilog;
using CommandLine;

namespace AWSIPRangeRouteCreator
{
   static class Program
   {
      static void Main(string[] args)
      {
         Run(args);
      }

      public static void Run(string[] args)
      {
         Serilog.Log.Logger = 
            GetDefaultLoggerConfiguration()
               .CreateLogger();

         AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
         {
            Log.Debug("Shutting down logger; Flushing...");
            Serilog.Log.CloseAndFlush();
         };

#if !DEBUG
         try
         {
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
               try
               {
                  if (ev?.ExceptionObject is Exception ex)
                  {
                     Log.Fatal("An unhandled error occured", ex);
                     return;
                  }

                  Log.Fatal($"An unhandled error occured {ev}");
                  
               }
               catch (Exception ex)
               {
                  Console.Error.WriteLine($"Failed to catch unhandled error '{ev?.ExceptionObject ?? ev}': {ex}");
               }
            };
#endif
         var parser = new Parser(settings =>
         {
            settings.IgnoreUnknownArguments = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
         });
         parser.ParseArguments<CmdOptions>(args)
            .WithParsed((opt) =>
            {
               if (opt.LogEventLevel != null)
                  Serilog.Log.Logger = 
                     GetDefaultLoggerConfiguration()
                        .MinimumLevel
                           .Is(opt.LogEventLevel.Value)
                        .CreateLogger();

               var runner = new Runner(opt);
               runner.Run();
            })
            .WithNotParsed((ex) =>
            {
               if (ex.All(err =>
                        new ErrorType[]
                        {
                           ErrorType.HelpRequestedError,
                           ErrorType.HelpVerbRequestedError,
                           ErrorType.UnknownOptionError
                        }.Contains(err.Tag))
                  )
                  return;

               foreach (var error in ex)
                  Log.Error($"Failed to parse: {error.Tag}");

               Log.Fatal("Failed to process args");
            });
#if !DEBUG
         }
         catch (Exception ex)
         {
            Log.Fatal(ex);
         }
#endif
      }

      private static LoggerConfiguration GetDefaultLoggerConfiguration()
      {
         return new LoggerConfiguration()
            .Enrich.WithThreadId()
            .MinimumLevel
#if DEBUG
               .Debug()
#else
               .Information()
#endif
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss,fff} {Level:u3} {ThreadId,-2} {Message:lj}{NewLine}{Exception}");
      }
   }
}
