# Changelog

## [Unreleased]

### New Features

- Skellige deck
- Show achievement and total card progress

### Enhancements

- Add bullets to location(s) of selected card for better readability
- Include original quote characters for card flavors
- Use gwent icon instead of default avalonia icon
- Polish missable quests UI
- Provide a likely fallback save location for linux

### Bug Fixes

- Don't show commas in selected card location if card has no location info
- Include string translations for strings found in .cs files
- Add missing _Balista_ cards to northern realms deck
- Fix exe icon not showing in windows explorer

## [1.2.0] - 2020-01-21

### Enhancements

- [[PR 6]] - Russian localization
- Polish localization from [Crowdin][1]
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

[unreleased]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.2.0-alpha...v1.0.0
[0.2.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.1.0-alpha...v0.2.0-alpha
[0.1.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/cd9002c...v0.1.0-alpha
[avalonia]: https://avaloniaui.net/
[PR 6]: https://github.com/Rfvgyhn/gwent-tracker/pull/6
[1]: https://crowdin.com/project/gwent-tracker/activity_stream#phrase_suggested-14084595-1579625838
