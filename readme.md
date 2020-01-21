# Gwent Tracker [![Translation Progress](https://badges.crowdin.net/gwent-tracker/localized.svg)][1]


Application that reads The Witcher 3 save files and reports gwent card 
collection progress for the collect 'em all achievement.

Download the latest release from [Github].

![Screenshot]

## Translations

You can submit translations via [Crowdin] or direct pull requests.

## Runtime Prerequisites
#### Windows
* [.Net Framework 4.7.2 runtime] (Likely already installed if you have an up-to-date OS)
* For windows 7 users, you'll need the [platform update]

#### Linux
* [.Net Core 3.1 Runtime]

  There are two versions of the application. Both depend on the .net core runtime. 
  One requires you to have the runtime installed on your machine and the other 
  has it pre-bundled with the application. If you have the runtime installed or
  are willing to install it, download the release without the _runtime_ tag in 
  the filename.

* GTK3 has to be installed in the system or be available from LD_LIBRARY_PATH

[1]: https://crowdin.com/project/gwent-tracker
[.Net Framework 4.7.2 runtime]: https://dotnet.microsoft.com/download/dotnet-framework/net472
[platform update]: https://www.microsoft.com/en-us/download/details.aspx?id=36805
[.Net Core 3.1 Runtime]: https://docs.microsoft.com/dotnet/core/install/linux-package-managers
[Github]: https://github.com/Rfvgyhn/gwent-tracker/releases
[Screenshot]: screenshot.png?raw=true
[Crowdin]: https://crwd.in/gwent-tracker