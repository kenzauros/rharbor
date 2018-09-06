RHarbor - Remote Desktop via SSH Servers
=====

RHarbor helps you to connect to Windows by Remote Desktop (RDP) via multi-hop SSH tunnel.

## Description

RHarbor can also manage Remote Desktop connections without SSH tunnel.

## Environment

- Windows 7 or later
- .NET Framework 4.6.1

## Install

RHarbor needs no special installation. Please unzip the downloaded file into a suitable folder and just start RHarbor.exe.

## How to use

Please see the detail pages.

- [Remote Desktop with multi-hop SSH](rdp-with-multi-hop-ssh.md)

## Clear all settings

"RHarbor.db" exists in the same folder as "RHarbor.exe". This file contains RHarbor's settings you were set.

To initialize the setting please shutdown RHarbor and then delete "RHarbor.db".

New "RHarbor.db" will be generated when you start RHarbor again.

## Notice

The passwords stored in RHarbor are encrypted but still remains some security risks.

Do not install in a shared computer.

Malicious programs or bad people may read passwords in the database file or in memory.

Please make sure that an appropriate antivirus software are installed on your computer and use on your own risk.

## Licence

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## Special Thanks to

- [Extended.Wpf.Toolkit](https://github.com/xceedsoftware/wpftoolkit)
- [Json.NET](https://www.newtonsoft.com/json)
- [NLog](https://nlog-project.org/)
- [ReactiveProperty](https://github.com/runceel/ReactiveProperty)
- [SSH.NET](https://github.com/sshnet/SSH.NET/)
- [System.Data.SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
- [System.Data.SQLite.EF6.Migrations](https://github.com/bubibubi/db2ef6migrations)

## Author

- [kenzauros](https://github.com/kenzauros)
