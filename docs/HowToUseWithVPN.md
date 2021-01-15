# How to use this with VPN
## Initial situation
You ONLY want to access all AWS machines via a VPN tunnel and not directly.

Maybe you have routing problems like described here: https://github.com/AdoptOpenJDK/openjdk-build/issues/1349

## Get a VPN 
You need a OpenVPN compatible VPN provider. 
Example: https://protonvpn.com

## Setup
* Download the .ovpn OpenVPN configuration file from your provider
* Download the [latest relase](https://github.com/litetex/AWSIPRangeRouteCreator/releases)
* Generate the routing information with ``AWSIPRangeRouteCreator.exe -f openvpn``
* Open the .ovpn file
  * (optional) Add ``route-nopull`` - This will use your locally described routes in the config-file instead of rerouting the complete traffic
  * Copy the routing information from ``aws-routes.txt`` into the file
  * Save the .opvpn file, it should look like this:
```
client

...

pull
fast-io

route-nopull
route 3.0.0.0 255.254.0.0
route 3.2.0.0 255.255.255.0
...
route 223.71.71.128 255.255.255.128

<ca>
-----BEGIN CERTIFICATE-----
...
```
* Import the .ovpn file into your VPN client

Done
