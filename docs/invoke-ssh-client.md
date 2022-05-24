Invocation of external programs
=====

- Go back to [RHarbor](index.md) top page

## Summary

You can call **SSH clients, such as Tera Term Pro or RLogin, with the SSH connection informations stored in RHarbor.**

## Settings of external programs

### Add a program setting

Click <img src="images/buttons/add-button.png" alt="plus" style="height:2ex"> button in [Settings]/[External Programs] to select a template. Select "From scratch" if your expected client not listed.

<img src="images\invoke-ssh-client\settings.png" alt="External Programs" width="480">

The executable path and command-line argument will be automatically set in some templates. Make sure these properties match your environment.

**Parameters such as `{host}` and `{username}` can be used in "Command-line argument".**
Each parameters will be replaced with the values of the SSH connection information. Please refer to the "Command-line argument" section for more details.

Click <img src="images/buttons/save-button.png" alt="Save Settings" style="height:2ex"> button at the bottom-right of the screen to save the settings.

### Prepared program setting templates

You can add a program setting using the following templates.

- OpenSSH (Copy `ssh` command text to clipboard, passing password not supported)
- PuTTY
- Tera Term Pro
- RLogin

Otherwise make your own setting as you wish.

### Command-line argument

**You can embed parameters in a command-line argument with the format like `{KEY}`.**

The following parameters can be used by default. They will be replaced with the values specified in the SSH connection information.

Key | Replacement
-- | --
`{host}` | Host name
`{port}` | Port
`{username}` | Username
`{password}` | Password / Passphrade
`{keyfile}` | File path for private key file

Likewise, the parameters which you define in **"Additional Parameters"** in each connection setting can be used.

In addition, **conditional operator (`condition ? positive : negative`)** can be included so that the value can be varied depending on the condition.

The "condition" part can accept the following expressions. First operand (`KEY` shown in the table below) will be evaluated as the key of the parameter and another parameter (`A`) will be evaluated as string.

Expression | Evaluation
-- | --
`KEY=A` | Positive if the value for the key eqauls to `A`
`KEY!=A` | Positive if the value for the key NOT eqauls to `A`
`KEY` | Positive if the value is set AND is non-empty
`!KEY` | Positive if the value is NOT set OR is empty

#### Examples for command-line argument

Assume the following connection information.

Parameter | Value
-- | --
Host name | `my-host`
Key file path | `C:\my-host.key`
Additional parameter `stage` | `staging`

Then if you set "Example" to "Command-line argument", "Result" value will be passed as the argument for the external program.

Example | Result | Explanation
-- | -- | --
`{stage}` | `staging` | Replaced with additional parameter `stage`'s value
`{stage=staging?blue:red}` | `blue` | Replaced with `blue` as `stage`'s value is `staging`
`{stage?hoge:fuga}` | `hoge` | Replaced with `hoge` as `stage` is defined
`{!stage?{host}:{host}-{stage}}` | `my-host-staging` | Replaced with `my-host-staging` as the result of `{host}-{stage}`
`/auth={keyfile?publickey:password}` | `/auth=publickey` | Replaced with `publickey` as `keyfile` is specified


### Removing a setting

Click <img src="images/buttons/remove-button.png" alt="Remove" style="height:2ex"> button and save settings to remove an existed setting.

## Invoking SSH client

Click <img src="images/buttons/ssh-client-button.png" alt="Invoke SSH client" style="height:2ex"> button in SSH connection information panel to list your programs.

<img src="images\invoke-ssh-client\invoke-ssh-client.png" alt="Invoking SSH client" width="480">

**Select a program then the program will be invoked with the connection information.**

If "Copy to clipboard" is checked in the program setting, the command text will be set to clipboard instead of starting the program.

## Tips

### Keyfile for PuTTY

**SSH key files which used in RHarbor are OpenSSH format. It means the key files are not able to use for [PuTTY](https://www.putty.org/)**

If you use PuTTY as an SSH client, please consider the following procedure.

First, convert from the OpenSSH key file into the PuTTY format using [PuTTYgen](https://www.puttygen.com/) (included in PuTTY).
Then name the PuTTY key file as `<Original OpenSSH key file name>.ppk` and place within the same folder as the OpenSSH key file.

For instance the PuTTY key file would be named `key.pem.ppk` for its original OpenSSH key file named `key.pem`.

Now you can pass the appropriate key file to the PuTTY program with the command-line argument setting like `putty -ssh -i "{keyfile}.ppk"`.
