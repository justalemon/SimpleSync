# SimpleSync<br>[![AppVeyor][appveyor-img]][appveyor-url] [![Patreon][patreon-img]][patreon-url] [![PayPal][paypal-img]][paypal-url] [![Discord][discord-img]][discord-url]

SimpleSync is a script for FiveM that allows you to synchronize the Weather, Time and Lights (also called Blackout) between clients. There are multiple modes that you can use:

* Dynamic (Time is increased over time, Weather is changed over time based on the current value)
* Static (the Weather and Time never changes, unless is done manually)
* Real (uses the information of real-life cities via Timezones and OpenWeatherMap for Time and Weather respectively)

Unlike other Synchronization scripts, the resource is highly configurable. You can change the Dynamic Weather switches, the scale of the Time and more options. No more scripts fighting for controlling the Time and Weather! Developers have exports available that they can use.

## Download

* [GitHub](https://github.com/justalemon/SimpleSync/releases)
* [AppVeyor](https://ci.appveyor.com/project/justalemon/simplesync) (experimental)

## Installation

In your **resources** directory, create a folder called **simplesync** and extracts the contents from the compressed file in there.

## Usage

Before using SimpleSync, disable other Time and Weather scripts like vMenu (`vmenu_enable_weather_sync` and `vmenu_enable_time_sync` convars to `false`) and vSync (stop the resource).

By default, SimpleSync will start in Time and Weather set to Dynamic. Check the [Wiki](https://github.com/justalemon/SimpleSync/wiki) for the list of convars, commands and exports.

[appveyor-img]: https://img.shields.io/appveyor/ci/justalemon/simplesync.svg?label=appveyor
[appveyor-url]: https://ci.appveyor.com/project/justalemon/simplesync
[patreon-img]: https://img.shields.io/badge/support-patreon-FF424D.svg
[patreon-url]: https://www.patreon.com/lemonchan
[paypal-img]: https://img.shields.io/badge/support-paypal-0079C1.svg
[paypal-url]: https://paypal.me/justalemon
[discord-img]: https://img.shields.io/badge/discord-join-7289DA.svg
[discord-url]: https://discord.gg/Cf6sspj
