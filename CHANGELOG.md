# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),


## [1.0.40] - 2019-08-19
### Added
- Added new api for getting provider name based off **DatabaseType**
see 
~~~csharp
public static class DBProviderHelper
~~~

### Changed
- Fix bug in MapTo where mapping T to string was returning null if datareader value was a type other than string
~~~csharp
public static T MapTo<T>(this IDataReader reader) where T : class
~~~



[1.0.40]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.40

