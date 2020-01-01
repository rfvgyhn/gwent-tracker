# Gwent Tracker
Application that reads The Witcher 3 save files and reports gwent card 
collection progress for the collect 'em all achievement.

Download the latest release from [Github][4].

![Screenshot][5]

## Runtime Prerequisites
#### Windows
* [.Net Framework 4.7.2 runtime][1] (Likely already installed if you have an up-to-date OS)
* For windows 7 users, you'll need the [platform update][2]

#### Linux
* [.Net Core 3.1 Runtime][3]

  There are two versions of the application. Both depend on the .net core runtime. 
  One requires you to have the runtime installed on your machine and the other 
  has it pre-bundled with the application. If you have the runtime installed or
  are willing to install it, download the release without the _runtime_ tag in 
  the filename.

* GTK3 has to be installed in the system or be available from LD_LIBRARY_PATH

[1]: https://dotnet.microsoft.com/download/dotnet-framework/net472
[2]: https://www.microsoft.com/en-us/download/details.aspx?id=36805
[3]: https://docs.microsoft.com/dotnet/core/install/linux-package-managers
[4]: https://github.com/Rfvgyhn/gwent-tracker/releases
[5]: screenshot.png?raw=true