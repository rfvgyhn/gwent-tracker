# Changelog

## [Unreleased]

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

[unreleased]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/rfvgyhn/gwent-tracker/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.2.0-alpha...v1.0.0
[0.2.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/v0.1.0-alpha...v0.2.0-alpha
[0.1.0-alpha]: https://github.com/rfvgyhn/gwent-tracker/compare/cd9002c...v0.1.0-alpha
[avalonia]: https://avaloniaui.net/
