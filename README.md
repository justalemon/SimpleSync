# SimpleSync [![AppVeyor][appveyor-img]][appveyor-url] [![Discord][discord-img]][discord-url]

## Weather and Time sync for FiveM

SimpleSync is a script for FiveM that allows you to sync the Weather and Time across the clients connected to the server.

With SimpleSync, you can have the Time in multiple configurations:

* Dynamic Time¹ (increases the Time by a specific value during the specified time)
* Static Time (sets a Time that will never change unless manually)
* Real Time (use the DST aware Time of a specific city around the world)

The Weather synchronization also has multiple configurations:

* Dynamic Weather (the weather changes over time)
* Static Weather (the Weather will never change unless manually)
* Real Weather via [OpenWeather](https://openweathermap.org) (uses the weather of a specific city around the world)

¹ Set to 2 IRL seconds = 1 minute in game by default, just like Story Mode and GTA Online

[appveyor-img]: https://img.shields.io/appveyor/ci/justalemon/simplesync.svg?label=appveyor
[appveyor-url]: https://ci.appveyor.com/project/justalemon/simplesync
[discord-img]: https://img.shields.io/badge/discord-join-7289DA.svg
[discord-url]: https://discord.gg/Cf6sspj
