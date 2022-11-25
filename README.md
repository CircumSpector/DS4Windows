<img src="assets/Vapour-128x128.png" align="right" />

# Codename: Vapour Input

[![Build status](https://ci.appveyor.com/api/projects/status/gt6hhm5aqy04ou7u?svg=true)](https://ci.appveyor.com/project/nefarius/ds4windows)
[![Requirements](https://img.shields.io/badge/Requirements-.NET%207.0-blue.svg)](https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md) ![Lines of code](https://img.shields.io/tokei/lines/github/CircumSpector/DS4Windows) ![GitHub issues by-label](https://img.shields.io/github/issues/CircumSpector/DS4Windows/bug) ![GitHub issues by-label](https://img.shields.io/github/issues/CircumSpector/DS4Windows/enhancement)

A reimagination of DS4Windows.

---

☣️ **Highly unstable, work-in-progress, constantly changing, incubating software ahead** ☣️

⚠️ **DO NOT USE UNSUPERVISED** ⚠️

## Disclaimers

- ⚠️ Might crash your system (BSOD)!
- ⚠️ May or may not launch/work/crash!
- ⚠️ Use at your own risk!
- ⚠️ No support provided whatsoever!

## About this fork

👉 [Very sporadically updated development blog](https://github.com/CircumSpector/DS4Windows/discussions/21).

### What this is

Over its lifespan of of *nearly a decade* DS4Windows has seen many contributors, changes, fixes, feature additions and has grown and kept relevant to gamers who'd love more control over their beloved peripherals. Beginning with the PS4 Controller (DualShock 4) it nowadays also supports its successor the PS5 DualSense and even the Nintendo JoyCons. With age and popularity come new challenges. The code has become more powerful, but also more troublesome to maintain and carries a lot of legacy design patterns and restraints from an outdated .NET universe. Here's where we step in.

[CircumSpector](https://github.com/CircumSpector) is a collective of enthusiasts craving to see DS4Windows continued. We attempt to rewrite major sections of the dated code segments to make maintenance and new feature additions fun again. This will take some time and a lot will probably break - intentionally or unintentionally so sooner or later we need a bigger test squad. For now, the issue tracker and discussions remain *collaborators only* to avoid bug reports for things we already know so we can focus on the code and nothing else.

In October 2022 we started a rebranding which includes an intermediate project name change to further distance this work from the current dominant and maintained DS4Windows version(s). An official new app name is yet to be settled on.

<!--
### What this is NOT

As of time of writing we don't strife to be considered the "new maintainers" and dethrone [Ryochan7](https://github.com/Ryochan7/DS4Windows) who's on a well-deserved hiatus from the project for a yet to be known duration (disclaimer: we don't speak on behalf of Ryochan7, we're merely observers as well). Time will tell if Ryochan7 comes back from a vacation and continues working on DS4Windows with help from the members of [CircumSpector](https://github.com/CircumSpector).
-->
## Where are the download links

There are none. Until this message changes, the rework is in constant motion and there is no value for us to provide binaries at this point. Feel free to clone and build yourself if you're brave 😜

## Where can I get more information

[Join our Discord server](https://discord.vigem.org/) 🎉

## Contributing

If you are a **developer** looking to join the team just drop @nefarius a message! ⌨️

If you **want to see this project succeed** give it a GitHub star to show interest! ❤️

## How to build

- Get [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/) (Community Edition is fine)
- Install ".NET desktop development" workload  
  ![setup_Z38OdfI0YX.png](assets/setup_Z38OdfI0YX.png)
- Install latest [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Build the solution in Visual Studio
  - Dependencies are pulled in automatically via NuGet
- To create a production release, use the command line:  
  `dotnet publish /p:PublishProfile=Properties\PublishProfiles\release-win-x64.pubxml`  
  ⚠️ this will fail when triggered via Visual Studio due to a pending issue ⚠️

## Sponsors

[<img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/jetbrains/jetbrains-original.svg" title="JetBrains ReSharper" alt="JetBrains" width="120" height="120"/>](https://www.jetbrains.com/resharper/) [<img src="assets/AiLogoColorRightText.png" title="Advanced Installer" alt="Advanced Instzaller" height="120"/>](https://www.advancedinstaller.com/)

<!--

## 3rd party credits

TODO: don't forget to populate and update those

This application benefits from these awesome projects (appearance in no special order):

- [Adonis UI](https://benruehl.github.io/adonis-ui/)
  - Lightweight UI toolkit for WPF applications offering classic but enhanced windows visuals
- [FastDeepCloner](https://github.com/AlenToma/FastDeepCloner)
  - FastDeepCloner, This is a C# based .NET cross platform library that is used to deep clone objects, whether they are serializable or not. It intends to be much faster than the normal binary serialization method of deep cloning objects
- [WpfExToolkit](https://github.com/dotnetprojects/WpfExtendedToolkit)
  - A fork of [wpftoolkit.codeplex.com](https://wpftoolkit.codeplex.com/) and now [github.com/xceedsoftware/wpftoolkit](https://github.com/xceedsoftware/wpftoolkit)
- [Extended Xml Serializer](https://extendedxmlserializer.github.io/)
  - A configurable and eXtensible Xml serializer for .NET
- [Fody](https://github.com/Fody/Fody)
  - Extensible tool for weaving .net assemblies
- [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
  - Injects INotifyPropertyChanged code into properties at compile time
- [AsyncErrorHandler.Fody](https://github.com/Fody/AsyncErrorHandler)
  - An extension for Fody to integrate error handling into async and TPL code
- [ConfigureAwait.Fody](https://github.com/Fody/ConfigureAwait)
  - Configure async code's ConfigureAwait at a global level
- [Hardcodet NotifyIcon for WPF](https://github.com/hardcodet/wpf-notifyicon)
  - NotifyIcon for .Net Core 3.1 and .Net 5 WPF
- [Jaeger Tracing](https://github.com/jaegertracing/jaeger-client-csharp)
  - C# client (tracer) for Jaeger
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
  - The [OpenTelemetry](https://opentelemetry.io/) .NET Client
- [MdXaml](https://github.com/whistyun/MdXaml)
  - Markdown for WPF - alternate version of Markdown.Xaml
- [Ookii.Dialogs.Wpf](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
  - Awesome dialogs for Windows Desktop applications built with Microsoft .NET (WPF)
- [Serilog](https://serilog.net/)
  - Flexible, structured events — log file convenience
- [Task Scheduler Managed Wrapper](https://github.com/dahall/taskscheduler)
  - Provides a .NET wrapper for the Windows Task Scheduler. It aggregates the multiple versions, provides an editor and allows for localization
- [WPFLocalizeExtension](https://github.com/XAMLMarkupExtensions/WPFLocalizeExtension)
  - LocalizationExtension is a the easy way to localize any type of DependencyProperties or native Properties on DependencyObjects
- [EmbedIO](https://github.com/unosquare/embedio)
  - A tiny, cross-platform, module based web server for .NET
- [Nefarius.Utilities.DeviceManagement](https://github.com/nefarius/Nefarius.Utilities.DeviceManagement)
  - Managed wrappers around SetupAPI and Cfgmgr32
- [P/Invoke](https://github.com/dotnet/pinvoke/)
  - A collection of libraries intended to contain all P/Invoke method signatures for popular operating systems

-->
