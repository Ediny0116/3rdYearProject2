# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.6.0] - 2023-11-08

### Fixed

- Some fixes for Clone heartbeats
- More fixes for persistent window layouts

### Features

- Can now focus player with hotkeys (ctr &  F9-F12)
- Just show the layout view toggles as disabled during playmode
- Automatically focus players after activating

## [0.5.0] - 2023-09-27

### Fixed

- Fix broken links in documentation

### Features

- Moved tags to the project settings window
- Players can now have multiple tags

## [0.4.0] - 2023-08-25

### Fixed

- Fixed an issue where a player could still show up as active after it had exited.
- Fixed an issue where you could have multiple add tag views.
- Fixed an issue where `CurrentPlayer.ReadOnlyTag` would not return the correct value when domain reload on play mode is disabled.
- Fixed an issue where clones were crashing when scrolling mouse over menu top bar

### Features

- Debug workflows now has dynamic layouts that persist between runs.
- Players that are unresponsive will now result in a prompt appearing in the main editor asking if they should be restarted.
- Now support for content selection in your players (Requires dedicated server package to be installed).

## [0.3.0] - 2023-07-26

### Fixed

- Fixed PlayerTags starting and ending with whitespace.
- Fixed handling of relative paths in the package.json of a player.

### Features

- Can now have your players reboot if they fail to import or stop unexpectedly (e.g. crash). A prompt will appear asking what action to take.
- Debugging workflows is now enabled (giving users more access to the editor in players). It can be accessed by using the Layout Dropdown in the top left of a player to enable more views (Scene, Game, etc).
- Can disable MPPM in settings.
- Updated Tag to be ReadOnlyTag to better communicate how tags as a feature works.

## [0.2.0] - 2023-06-26

### Fixed

- Fixed null exception when loading into a new scene with unsaved changes.
- Fixed exception triggered on players when changing scene in play mode.
- Updated text to "Show in Finder" on MacOS for revealing Players location on disk.
- Fixed issues with the Asset Database syncing.

### Features

- Display a message to the console log when opening Clones that don't have the required symlinked folders.
- Trigger play mode for clones before main editor starts entering play mode.
- Clone players will have the same behaviour as the main editor regarding the "Stop playing and compile" option.
 
## [0.1.1] - 2023-01-26

### Fixed

- Reduced GC allocations in the messaging system loop.
- Fixed CurrentPlayer.Tag returning "Untagged" instead of an empty string if a tag is not assigned.
- Fixed bug where pressing Return or Enter would not submit a created tag to the UI.
- Fixed missing XMLDoc for CurrentPlayer API.
- Fixed broken links in the package documentation.

### Features

- Added support for additive scenes.

## [0.1.0] - 2022-11-15

### Initial Release

- This release adds the Multiplayer Play Mode, which is a multiplayer development workflow aiming to provide a more efficient and quick development cycle of multiplayer games. 
The tool enables multiple editor instances to be opened simultaneously on the same development device, using the same source assets on disk.
