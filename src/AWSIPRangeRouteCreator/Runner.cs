using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace AWSIPRangeRouteCreator
{
   public class Runner
   {
      private CmdOptions Options { get; init; }


      public Runner(CmdOptions cmdOptions)
      {
         Options = cmdOptions;
      }

      public void Run()
      {
         JObject downloadedObject;

         Log.Info($"Downloading active aws networks from '{Options.DownloadUrl}'");

         using (var webclient = new WebClient())
            downloadedObject = JObject.Parse(webclient.DownloadString(Options.DownloadUrl));

         Log.Info("Download successful");

         Log.Info("Parsing and filtering...");
         if(Options.Regions?.Any() ?? false)
         {
            Log.Info($"- Filtering for regions='{string.Join(',', Options.Regions)}'");
         }

         var input = new HashSet<string>(
            (downloadedObject["prefixes"] as JArray)
               .Where(x =>
               {
                  if (!Options.Regions?.Any() ?? false)
                     return true;

                  return Options.Regions.Contains(x["region"].ToObject<string>());
               })
               .Select(x => x["ip_prefix"].ToObject<string>())
            );
         Log.Info("Parsing successful");

         Log.Debug("Will use the following prefixes: ");
         if (Serilog.Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            foreach (var ipPrefix in input)
               Log.Debug(ipPrefix);

         var origNetworks = input
               .Select(x => IPNetwork.Parse(x))
               .OrderBy(x => x)
               .ToHashSet();

         Log.Info("Removing networks that are already contained in other networks");
         RemoveContainedNetworks(origNetworks);
         Log.Info("Done");

         var supers = origNetworks.ToDictionary(x => x);


         var simplified = new HashSet<IPNetwork>();

         for (int i = 0; i < 24 && supers.Count > 0; i++)
         {
            Log.Info($"Run {i + 1}");
            Log.Info("Starting calc supernets");
            foreach (var key in new List<IPNetwork>(supers.Keys))
            {
               // IANA only gives out max /8 blocks
               if (key.Cidr <= 8)
               {
                  simplified.Add(key);
                  supers.Remove(key);
                  continue;
               }

               // Get supernet
               supers[key] = GetSuperNetwork(supers[key]);
            }
            Log.Info("Finished calc supernets");

            Log.Info("Simplifying...");
            foreach (var key in new List<IPNetwork>(supers.Keys))
            {
               // Check if enough other ips are in the supernet
               // if yes -> merge the nets together and build next supernet -> go on
               // if no -> remove and put into simplified

               // Try to get the value
               // false = already processed -> skip
               if (!supers.TryGetValue(key, out IPNetwork superNet))
                  continue;

               var sameNets = supers.Where(x => x.Value == superNet);
               // The next calculated supernet always has 2 subnets
               if (sameNets.Count() > 1)
               {
                  Log.Debug($"-S- SUPERNETWORK: {superNet}; Total IPs = {superNet.Total}");

                  if (Serilog.Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                     foreach (var x in sameNets.Select(x => x.Key))
                        Log.Debug($"- {x}");

                  // Quick check: does the ipcount match?
                  var sumTotalIPsSameNets = sameNets.Select(x => x.Key.Total).Aggregate((currentSum, x) => currentSum + x);

                  Log.Debug($"IPcount in detected subnets {sumTotalIPsSameNets}");
                  if (sumTotalIPsSameNets >= superNet.Total)
                  {
                     bool valid = true;
                     if (Options.SupernetFullIpCheck)
                     {
                        var allIpsInNet = new HashSet<byte[]>();

                        foreach (var net in sameNets.Select(x => x.Key))
                        {
                           foreach (var adr in net.ListIPAddress())
                           {
                              allIpsInNet.Add(adr.GetAddressBytes());
                           }
                        }

                        Log.Debug($"Total Unique Ids = {allIpsInNet.Count}");

                        if (allIpsInNet.Count == superNet.Total)
                           valid = true;
                     }

                     if (valid)
                     {
                        foreach (var net in sameNets.Select(x => x.Key))
                           supers.Remove(net);

                        supers.Add(superNet, superNet);
                        continue;
                     }
                  }
               }

               // No match -> put into simplified and remove
               simplified.Add(key);
               supers.Remove(key);
            }

            Log.Info("Finished simplifying");
         }

         Log.Info("Removing networks that are already contained in other networks");
         RemoveContainedNetworks(simplified);
         Log.Info("Done");

         var final = simplified
            .OrderBy(x => x)
            .ToList();


         Func<IPNetwork, string> rowFormatter = net => net.ToString();
         if (Options.OutputFileFormat == OutputFileFormat.OpenVPN)
            rowFormatter = net => $"route {net.Network} {net.Netmask}";

         File.WriteAllLines(Options.OutputFile, final.Select(rowFormatter));
         Log.Info($"Wrote info to '{Options.OutputFile}'; Format = {Options.OutputFileFormat}");
      }

      static void RemoveContainedNetworks(ISet<IPNetwork> nets)
      {
         foreach (var net in nets.Select(x => x))
         {
            foreach (var containCheckNet in nets.Where(x => x != net))
            {
               if (containCheckNet.Contains(net))
                  nets.Remove(net);
            }
         }
      }

      static IPNetwork GetSuperNetwork(IPNetwork network)
      {
         return IPNetwork.Parse($"{network.Network}/{network.Cidr - 1}");
      }
   }
}
