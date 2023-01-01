# Unity Version Changeset

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/CheeryLee/UnityVersionChangeset)

A small .NET library to work with Unity game engine releases archive

## Features
* Obtains the list of all available engine versions
* Gets changeset number of the required version
* Shows various useful info about release
* Fully async, but contains sync method analogues
* Can be used in your CI/CD environment based on .NET
* Pretty simple to use

## How it works
Basically the algorithm refers to Unity archive website and parses the resulting HTML that it has received, because there is no "true development" technique to get an information about any engine release.

Unfortunately not everything can be solved with this approach, the problems are described in [**"Known issues"**](#known-issues) section.

## Usage
### Get data
#### All versions
The straightforward way:
```csharp
await UnityVersionManager.GetAllVersionsAsync();
```
It returns the whole list of available releases.

#### Single version
To get only one version, run this:
```csharp
// with custom version object:
var data = await UnityVersionManager.GetVersionAsync(new UnityVersion(2020, 3, 34));
// or with string signature:
data = await UnityVersionManager.GetVersionAsync("2020.3.34");

// for alpha and beta version:
data = await UnityVersionManager.GetVersionAsync(new UnityVersion(2023, 1, 0, 14, UnityVersion.VersionType.Alpha));
data = await UnityVersionManager.GetVersionAsync("2022.2.0b9");
```

#### Changeset
Changeset is a control sum that is required to download the release from Unity CDN. You need to use it along with version string. To get it, run this:
```csharp
// with custom version object:
var changeSet = await UnityVersionManager.GetChangeSetAsync(new UnityVersion(2020, 3, 34));
// or with string signature:
changeSet = await UnityVersionManager.GetChangeSetAsync("2020.3.34");

Console.WriteLine(changeSet); // 9a4c9c70452b
```

#### Installable modules
Also you can find the platform targets Unity is building for. To get them, run this:
```csharp
// with custom version object:
var version = new UnityVersion(2020, 3, 34);
var platform = Platform.Windows; // the platform Unity editor is running
var modules = await UnityVersionManager.GetModulesAsync(version, platform);
// or with string signature:
modules = await UnityVersionManager.GetModulesAsync("2020.3.34", platform);

Console.WriteLine(modules[0].Id); // mac-mono
Console.WriteLine(modules[0].Name); // Mac Mono
```

### Run synchronously
At the same time with asynchronous methods there are their synchronous alternatives out of the box. To use them run signatures without _Async_ postfix in name.

### Memory notice
Despite the fact that you run single methods the library will still execute `GetAllVersionsAsync` method under the hood to produce cache. It takes up some memory but dramatically increase performance with slow Internet connection or under a heavy load.

### Update cache
By default all data downloaded in previous stages are cached and won't be updated if there will be anything new on Unity website. It means that running any other method of library would operate with the information that statically stored in `UnityVersionManager` class. The solution assumes speeding up the workflow if you want to use multiple data gathered by `GetAllVersions` method. To update it run `Flush` method:
```csharp
var data = await UnityVersionManager.GetAllVersionsAsync();

// ... something more happened and we need to clear cache
await UnityVersionManager.Flush();
// now we will have completely new data
data = await UnityVersionManager.GetAllVersionsAsync();
```

### Null checks
All basic operations in the project returns `RequestResult` structure to prevent unhandled exceptions on endpoints related to null references. It looks like null object pattern, but works a bit differently. At first you have to make sure that data is correct by referring to `Status` field:
```csharp
var response = await UnityVersionManager.GetChangeSetAsync("2020.3.34");

// if response is correct, do something with it
if (response.Status == ResultStatus.Ok)
    Console.WriteLine(response.Result);
```

## Console app
The project provides the realization of all features in one small console app. You can find it's source inside `UnityVersionChangeset.App` folder.

Typing `-h` or `--help` argument will give you a small description how it works.

<a name="known-issues"></a>
## Known issues
* It's not possible to get changesets while gathering data about all releases. Changeset is located on a release page and to get multiple values we need to send a lot of requests.
* A version number of **release** build doesn't have proper revision number. It's always equal to 1.
* Due to the previous point it's not recommended to specify revision number in `UnityVersion` object while working with **release** builds.

## License
This project is licensed under [the MIT license](LICENSE).