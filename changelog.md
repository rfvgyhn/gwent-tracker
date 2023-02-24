# Changelog

## [1.5.1] - 2023-02-24

### Enhancements

- Upgrade to dotnet 7 and avalonia 11
- Add support for save files from the remastered version of the game

## [1.5.0] - 2020-04-07

### New Features

- Allow textures to be loaded from a local directory
- Allow remote textures to be cached locally for future use via config option `cacheRemoteTextures`

### Enhancements

- Add scrollbar to missable quests for users with high DPI settings
- Override Francesca Findabair: the Beautiful card count for save files that report multiple copies

### Bug Fixes

- Fix multi-column sort by upgrade Avalonia to 0.9.3
- Fix crash when unable to reach remote texture path by providing a fallback texture
- Fix not all summon cards being counted as obtained if their parent card was obtained

## [1.4.0] - 2020-02-06

### New Features

- Show max amount of copies for each card
- Source column which shows if the card is a part the base game or DLC

### Enhancements 

- Czech localization ([Crowdin] by [Jan Kalný])

### Bug Fixes

- Add missing DLC cards
- Fix improper count of DLC cards in progress section

## [1.3.0] - 2020-01-31

### New Features

- Skellige deck
- Show achievement and total card progress

### Enhancements

- Add bullets to location(s) of selected card for better readability
- Include original quote characters for card flavors
- Use gwent icon instead of default avalonia icon
- Polish missable quests UI
- Provide a likely fallback save location for linux
- Use an icon instead of the character 'X' for filter remove button
- Add tooltip to convey what _Cutoff_ means in missable quest
- Russian localization ([Crowdin] by [W1ns-Smile])
- Czech localization ([Crowdin] by [Jan Kalný])

### Bug Fixes

- Don't show commas in selected card location if card has no location info
- Include string translations for strings found in .cs files
- Add missing _Balista_ cards to northern realms deck
- Fix exe icon not showing in windows explorer

## [1.2.0] - 2020-01-21

### Enhancements

- Russian localization ([#6] by [W1ns-Smile])
- Polish localization ([Crowdin] by [Binori])
- Wrap text in selected card column since translations could be cutoff

### Bug Fixes

 - Fix an issue when a save file has an unknown variable type the looks like a known variable type... again
 - Fix the filter textbox watermark not being translated
 - Add back translation keys that went missing in last release
 - Fix an issue where some non-latin glyphs wouldn't be rendered properly
 
## [1.1.0] - 2020-01-18

### New Features

#### Localization

- Card data is now localized based off of the game's w3strings files
- UI strings have support for localization
- `Culture` setting added to `settings.ini`. Allows you to specify a culture different than your system's default
- `FontFamily` setting added to `settings.ini`. Allows you to specify a font in-case your default font doesn't have the correct glyphs

## [1.0.1] - 2020-01-02

### Bug Fixes

- Fix an issue when a save file has an unknown variable type the looks like a known variable type

## [1.0.0] - 2020-01-01

### Enhancements

- Move to [Avalonia] from WPF

### New Features

- Linux compatibility
- Show missable card information
- Show detailed location for selected card

## [0.2.0-alpha] - 2017-01-27

### Enhancements

- Try to use common save location if the location specified in config file isn't found
- Add logging

## [0.1.0-alpha] - 2017-01-26

[unreleased]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.5.1...HEAD
[1.5.1]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.2.0-alpha...v1.0.0
[0.2.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.1.0-alpha...v0.2.0-alpha
[0.1.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/cd9002c...v0.1.0-alpha
[avalonia]: https://avaloniaui.net/
[#6]: https://github.com/Rfvgyhn/gwent-tracker/pull/6
[Crowdin]: https://crowdin.com/project/gwent-tracker
[W1ns-Smile]: https://github.com/W1ns-Smile
[Binori]: https://crowdin.com/profile/Binori
[Jan Kalný]: https://crowdin.com/profile/honzas4400w
