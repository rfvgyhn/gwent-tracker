# Gwent Tracker [![Translation Progress](https://badges.crowdin.net/gwent-tracker/localized.svg)][1]


Application that reads The Witcher 3 save files and reports gwent card 
collection progress for the collect 'em all achievement.

Download the latest release from [Github].

![Screenshot]

## Installation

This application doesn't require any installation. It's a standalone executable.

1. Download archive for your operating system
2. Extract - On windows, right click file and select extract. On linux, use your systems archive manager or `﻿unzip`
3. Run the executable named _GwentTracker_


## Usage

The application will load the latest save in your save game directory. It will also watch that directory for changes so you can leave the app open as you play and it will automatically update.

All column headers are sortable (including the checkbox column). You can `Shift+Click` column headers to sort by more one than one column at a time.

You may use the filter text box to narrow down the cards that are shown. For example, typing _velen_ into the textbox and clicking the add button will only show cards with the word velen in their name, deck, type, location or region.

## Settings

Settings are in the file named `settings.ini`. You likely won't need to change any of these if you have a default installation of The Witcher 3.

| Name                | Description
| ------------------- | ------------
| Autoload            | Whether or not to automatically read new saves
| DefaultSavePath     | Location of your save game files
| TexturePath         | Where to retrieve card images
| CacheRemoteTextures | Save a copy of the card texture if it doesn't already exist in the textures folder
| LogToFile           | Log to a file named log.txt. This can be useful for debugging problems
| Culture             | If left unspecified, culture will default to system's default culture. Culture codes are: ar, cs, de, en, es, es-mx, fr, hu, it, ja, kr, pl, pt-br, ru, tr, zh, zh-cn
| FontFamily          | In-case your default font doesn't have the glyphs needed by your culture setting, specify a font that does have them

## Translations

You can submit translations via [Crowdin] or direct pull requests.

[1]: https://crowdin.com/project/gwent-tracker
[Github]: https://github.com/Rfvgyhn/gwent-tracker/releases
[Screenshot]: screenshot.png?raw=true
[Crowdin]: https://crwd.in/gwent-tracker