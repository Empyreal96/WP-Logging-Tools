# Windows Logging Tools for Windows 10 Mobile devices

This tool allows you to enable and view logs and debugging features for a few different aspects of Windows 10 Mobile

## What's New?
*Note this is just an update to update this repo with my offline repo, an "experimental" build has been released for those curious*
- Enable Windows Crash Dumps page (Not fully implimented yet, I believe it needs some extra values to be set)
- Invoke BSOD to test crash dumps (User needs to copy required files to phone storage, see InvokeBSOD-info.txt)
- Device Status page for basic dev info (WIP)

## Types of logging available?
- Windows Boot Logging (What drivers and files are loaded on startup)
- On-screen UEFI Charging log
- Image Update logs (i.e Windows Update)
- Enabling app crash dumps
- Enable Kernel Debugging over USB
- Device Status/Info


## Requirements:
- Interop Unlocked
- CMD access
- Windows 10 build 10240 - 15254

This is currently WIP so UI and features may change before release


### Credit:
- [Bashar Astifan](https://github.com/basharast) with help testing
- NDTKLib creator (gus33000 ?) for the NDTK library
- [Native Access Library](https://forum.xda-developers.com/t/libraries-source-wp8-native-access-project.2393243/) for the Registry lib by GoodDayToDie
- [Fadil Fadz](https://github.com/fadilfadz01) for CMD Injector
- [peewpw](https://github.com/peewpw/Invoke-BSOD) for the Invoke-BSOD code
