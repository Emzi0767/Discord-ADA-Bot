#Advanced/Automatic Discord Administrator by Emzi0767

[![Emzi's Central Dispatch](https://discordapp.com/api/guilds/207879549394878464/widget.png)](https://discord.gg/rGKrJDR)

##ABOUT

A Discord bot built on top of [Discord.NET library](https://github.com/RogueException/Discord.Net). It's designed to simplify and automate certain administrative tasks. Supports 3rd party plugins to allow extending the functionality even further.

##BUILDING

1. In order to build this project, you will need to add the following package sources to your NuGet:
   * `https://www.myget.org/F/discord-net/api/v3/index.json`
   * Subdirectory of this project: `ext_lib/local_nuget`
2. Next, you must restore all NuGet packages.
3. Finally, build the code in Release mode.

You might want to publish the bot for the target platform.

##SETUP

In order for bot to run, you will need to set up your environment. 

1. Create a directory for the bot.
2. In that directory, create the following directories:
   * `plugins`
   * `references`
   * `providers`
3. Copy `config.json` from `sample_configs` to bot's directory.
4. Edit `config.json` and put your bot's access token in the file.
5. Copy bot's Publish results to the bot directory.
6. Put all plugins you want to use in `plugins` directory.
7. If plugins have any plugin-specific references, put them in `references`.
   * There is only one such reference for plugins in this repository. Advanced Commands `MarkovCore.dll` (.NET Core).

##RUNNING

Run `Emzi0767.Ada.exe`. That's it, your bot is now running.

##SUPPORT ME

If you feel like supporting me by providing me with currency that I can exchange for goods and services, you can do so on [my Patreon](https://www.patreon.com/emzi0767).

##ADDITIONAL HELP

Should you still have any questions regarding the bot, feel free to join my server. I'll try to answer an questions:

[![Emzi's Central Dispatch](https://discordapp.com/api/guilds/207879549394878464/embed.png?style=banner1)](https://discord.gg/rGKrJDR)

##REPORTING BUGS

Bugs happen, no software is perfect. If you happen to cause the software to crash or otherwise behave in an unintended manner, make sure to let me know using via [the issue tracker](https://github.com/Emzi0767/Discord-ADA-Bot/issues). If possible, include the list of steps you took that caused the problem.