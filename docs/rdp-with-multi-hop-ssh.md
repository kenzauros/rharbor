Remote Desktop with multi-hop SSH
=====

## Summary

RHarbor helps you to establish multihop SSH connections in one step.

If you have prepared multistage SSH connection, you can connect to Windows in remote network far from local network with Remote Desktop (RDP).

RHarbor automatically establish the required SSH connections before starting RDP.

Although there is no upper limit on the number of hop, the delay may increase due to the overhead of the connections.

## Setup connections

In the following instruction, we will consider the connections below.

```
Bastion1 SSH (192.168.10.114:22)
  -> Bastion2 SSH (192.168.10.112:22)
    -> Windows1 RDP (192.168.10.10:3389)
```

Both SSHs are password authentication and all user names are "hogehoge".

The procedure is as follows.

1. Define connection settings for Bastion 1
1. Define connection settings for Bastion 2
1. Define connection settings for Windows 1

### Define connection settings for Bastion 1

Start RHarbor.

<img src="images/multihop_1.png" width="480">

1. Open SSH tab.
2. Click + (Add new connection info).

<img src="images/multihop_2.png" width="480">

1. Input the connection informations for Bastion1.
2. Click Save button to keep the connection settings for Bastion1.

### Define connection settings for Bastion 2

<img src="images/multihop_3.png" width="480">

1. Make sure that Bastion1 setting is successfully saved.
2. Click + (Add new connection info) button.

<img src="images/multihop_4.png" width="480">

1. Input the connection informations for Bastion2.
2. **Set "Bastion1" as "Required Connection"**.
3. Click Save button to keep the connection settings for Bastion2.

### Define connection settings for Windows 1

<img src="images/multihop_5.png" width="480">

1. Open RDP tab.
2. Click + (Add new connection info).

<img src="images/multihop_6.png" width="480">

1. Input the connection informations for Windows1.
2. **Set "Bastion2" as "Required Connection"**.
3. Click Save button to keep the connection settings for Windows1.

<img src="images/multihop_7.png" width="480">

1. Make sure that Windows1 setting is successfully saved.
2. Click Connect button for Windows1.
3. Click Yes on the confirmation dialog.

<img src="images/multihop_8.png" width="280">

1. Input your account if authentication dialogs show.

<img src="images/multihop_9.png" width="480">

After Bastion1 and Bastion2 connections being established, an authentication window of Remote Desktop (mstsc.exe) will appear.
