# :wrench: Configuration 
## :computer: CLI Args

| Arg | Description |
| --- | ----------- | 
| ``--downloadurl <url>`` | URL used for downloading the infos from aws |
| ``-r <region/regions seperated by ,>``<br>``--region <region/s>`` | Only generates the routes for speicific regions; e.g: us-east-1,us-west-1 |
| ``--supernetfullipcheck`` | Checks if every IP is unique; Much slower but may fix broken aws data |
| ``-f <format>``<br>``--outputfileformat <format>`` | Output file format; Options:<ul><li>simple (default) - ``1.1.0.0/16``</li><li> openvpn - ``route 1.1.0.0 255.255.0.0``</li></ul> |
| ``-o <path>``<br>``--outputfile <path>`` | Outputfile path/name; Default ``aws-routes.txt`` |
| ``-l <loglevel>``<br>``--loglevel <loglevel>`` | Loglevel; 'verbose', 'debug', 'information', 'warning', 'error', 'fatal' |

## Examples
* Only generate routes for specific Regions<br>``AWSIPRangeRouteCreator.exe -r us-east-1,us-east-2``
* Generate infos in openvpn format<br>``AWSIPRangeRouteCreator.exe -f openvpn``
