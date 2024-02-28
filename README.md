# DynamicLOD_ResetEdition

Based on muumimorko's idea and code in MSFS_AdaptiveLOD, as further developed by Fragtality in DynamicLOD.<br/><br/>

This utility builds upon the functionality provided in DynamicLOD, which aims to improve MSFS performance and smoothness by dynamically changing the TLOD and OLOD based on the current AGL, and provides additonal features such as:<br/>
- Simultaneous PC, FG and VR mode compatibilty including correct FG FPS display and separate FPS targets for each mode,<br/>
- Optional LOD updates in cruise,<br/> 
- Optional predictive incremental steps between LOD changes to improve smoothness,<br/>
- Optional cloud quality decrease with FPS Adaption,<br/>
- Enhanced FPS Adaption control,<br/>
- Automatic pause when MSFS loses focus option, particularly useful if using FG due to varying FPS when MSFS gains or loses focus,<br/>
- Automatic FPS settling timer on MSFS graphics mode and focus changes to allow FPS to stabilise before being acted upon,<br/>
- Auto future MSFS version compatibility, provided MSFS memory changes are like in previous updates,<br/>
- Update prompt if newer utility version found on startup,<br/>
- Custom profile naming, but only through editing of the config file at this time,<br/> 
- Auto restoration of original settings changed by the utility,<br/>
- Streamlined log entries,<br/> 
- Removal of redundant features, and<br/>
- Minor UI changes.<br/><br/>

Which app should I use? DynamicLOD_ResetEdition or MSFS2020_AutoFPS?:
- Essentially both apps are intended to give you better overall performance but with different priorities to achieve it that result in a slightly different experience.  They both allow a lower TLOD down low and on the ground, when your viewing distance reduced anyway so the visual impact is minimal, and a higher TLOD when at higher altitude and not in close proximity to complex scenery or traffic. They also adjust OLOD and Cloud Quality but TLOD is usually the most important determiner of performance at these two extremes.
- Where they differ is that DynamicLOD provides user set tables for LOD changes at specific altitudes, giving the user precise control over when and where these changes take place such that they can optimise them to their particular flight activity they normally do, and can set a specific profile for each one. The price of such precise control is that the user must be intimately familiar with LODs to be able to tune a variety of settings in the app for the best outcome and this can be a bit daunting for more casual and non-technical users.
- Alternatively, AutoFPS seeks to automate these changes as much as possible based on a target FPS and a minimum and maximum LOD range within which to automatically adjust. This results in a much simpler and generally similarly acceptable user experience compared to DynamicLOD. Nonetheless, the automation algorithm does require FPS headroom to function correctly, so can conflict in cases where an FPS cap is being used, such as with Vsync or motion reprojection in VR. Additionally, AutoFPS tends to make constant small changes to TLOD, much more than DynamicLOD does, and this can induce stuttering on older hardware as it struggles to manage even small scenery changes. In these cases, the user would be better off using DynamicLOD in a more manually tuned approach. 

This utility can be installed concurrent with any DynamicLOD variant. You just shouldn't run them at the same time, as they would both be fighting each other with MSFS settings. This app can detect whether itself, a previous DynamicLOD variant or MSFS2020_AutoFPS is running and will quit if it encounters one.</br>

Important:<br/> 
- This utility directly accesses active MSFS memory locations while MSFS is running to read and set OLOD, TLOD and cloud quality settings on the fly. From 0.3.7 version onwards, the utility will first verify that the MSFS memory locations being used are still valid and if not, likely because of an MSFS version change, will attempt to find where they have been relocated. If it does find the new memory locations and they pass validation tests, the utility will update itself automatically and will function as normal. If it can't find or validate MSFS memory locations at any time when starting up, the utility will self-restrict to read only mode to prevent the utility making changes to unknown MSFS memory locations.<br/>
- As such, I believe the app to be robust in its interaction with validated MSFS memory locations and to be responsible in disabling itself if it can't guarantee that. Nonetheless, this utility is offered as is and no responsibility will be taken for unintended negative side effects. Use at your own risk!<br/><br/>

If you are not familiar with what MSFS graphics settings do, specifically TLOD, OLOD and cloud quality, and don't understand the consequences of changing them, it is highly recommended you do not use this utility.
<br/><br/>

This utility is unsigned because I am a hobbyist and the cost of obtaining certification is prohibitive to me. As a result, you may get a warning message of a potentially dangerous app when you download it in a web browser like Chrome. You can either trust this download, based on feedback you can easily find on Avsim and Youtube, and run a virus scan and malware scan before you install just be sure, otherwise choose not to and not have this utility version.<br/><br/>

## Requirements

The Installer will install the following Software:
- .NET 7 Desktop Runtime (x64)
- MobiFlight Event/WASM Module

<br/>

[Download here](https://github.com/ResetXPDR/DynamicLOD_ResetEdition/releases/latest)

(Under Assests, the DynamicLOD_ResetEdition-Installer-vXYZ.exe File)

<br/><br/>

## Installation / Update / Uninstall
Basically: Just run the Installer.<br/>

Some Notes:
- DynamicLOD_ResetEdition has to be stopped before installing.
- If the MobiFlight Module is not installed or outdated, MSFS also has to be stopped.
- If you have duplicate MobiFlight Modules installed, in either your official or community folders, the utility may display 0 value Sim Values and otherwise not function. Remove the duplicate versions, rerun the utility installer and it should now work.
- Do not run the Installer as Admin!
- If you wish to retain your settings for an update version, do NOT uninstall first, as that deletes all app files, including the config file. Just run the installer, select update and your settings will be retained.
- For Auto-Start either your FSUIPC7.ini or EXE.xml (MSFS) is modified. The Installer does not create a Backup.
- The utility may be blocked by Windows Security or your AV-Scanner, try if unblocking and/or setting an Exception helps (for the whole Folder)
- The Installation-Location is fixed to %appdata%\DynamicLOD_ResetEdition (your Users AppData\Roaming Folder) and can't be changed.
  - Binary in %appdata%\DynamicLOD_ResetEdition\bin
  - Logs in %appdata%\DynamicLOD_ResetEdition\log
  - Config: %appdata%\DynamicLOD_ResetEdition\DynamicLOD_ResetEdition.config
- If after installing and running the app your simconnect always stays red, try downloading and installing a Microsoft official version of “Microsoft Visual C++ 2015 - 2022 Redistributable”, which may be missing from your Windows installation.
- If you get an "MSFS compatibility test failed - Read Only mode" message there are three possible causes:
  - You may have changed MSFS settings in your usercfg.opt file beyond what is possible to set in the MSFS settings menu. To rectify, go into MSFS settings at the main menu and reset to default (F12) the graphics settings for both PC and VR mode, then make all changes to MSFS within the MSFS settings menu.
  - There is an issue with permissions and you may need to run the app as Administrator.
  - A new version of MSFS has come out that has a different memory map to what the app expects, which has happened only once since MSFS 2020 was released, and the app can't auto adjust to the new memory location for MSFS settings. If so, I will likely be already aware of it and working on a solution, but if you may be one of the first to encounter it (eg. on an MSFS beta) then please do let me know.
- To uninstall, run the installer and select remove on the first window. This will remove all traces of the app, including the desktop icon, MSFS or FSUIPC autostart entries if you used them, and the entire app folder, including your configuration file.

<br/><br/>

## Usage / Configuration

- General
  - Starting manually: anytime, but preferably before MSFS or in the Main Menu. The utility will stop itself when MSFS closes.  
  - Closing the Window does not close the utiltiy, use the Context Menu of the SysTray Icon.
  - Clicking on the SysTray Icon opens the Window (again).
  - Runnning as Admin NOT required (BUT: It is required to be run under the same User/Elevation as MSFS).
  - Do not change TLOD, OLOD and Cloud Quality MSFS settings manually while in a flight with this app running as it will conflict with what the app is managing and they will not restore to what you set when you exit your flight. If you wish to change the defaults for these MSFS settings, you must do so either without this app running or, if it is, only while you are in the MSFS main menu (ie not in a flight). 
- Connection Status
  - Red values indicate not connected, green is connected.
- Sim Values
  - Will not show valid values unless all three connections are green.
  - Red values mean FPS Adaption is active, orange means LOD stepping is active, black means steady state, n/a means not available right now.
- General
  - You can have (exactly) six different Sets/Profiles for the AGL/LOD Pairs to switch between (manually but dynamically).
  - If you wish to have custom profile names, you will need to manually edit the config file for these items after running the new app version at least once, eg. &lt;add key="profileName1" value="IFR" /&gt;.
  - Cruise LOD Updates, when checked, will continue to update LOD values based on AGL in the cruise phase, which is useful for VFR flights over undulating terrain and has an otherwise negligble impact on high level or IFR flights so it is recommended to enable this.
  - LOD Step Max, when checked, allows the utility to slow the rate of change in LOD per second, with increase and decrease being individually settable, to smooth out LOD table changes. This allows you to have large steps in your LOD tables without experiencing abrupt changes like having it disabled would do, hence it is recommended to turn it on and start out with the default steps of 5.
  - Pause when MSFS loses focus - This will stop MSFS settings being changed while you are focused on another app and not MSFS. It is particularly useful for when using FG as the FG active and inactive frame rate can vary quite considerably and because FG is not always an exact doubling of non-FG FPS.
  - Status Message - On app startup indicates key system messages, such as:
    - Before loading a flight - whether a newer version of the app is available to download and install
    - Loading in to a flight - whether MSFS memory integrity test have failed, and
    - Flight is loaded - showing detected DX version, Graphics Mode (PC, FG, or VR), and app pause or FPS settling time status as applicable. The FPS settling timer runs for 6 seconds to allow FPS to settle between pausing/unpausing and VR/PC/FG mode transitions. This allows the FPS to stabilise before engaging automatic functions and should lead to much smaller TLOD changes when seeking the target FPS on such transitions.
- LOD Level Tables
  - The first Pair with AGL 0 can not be deleted. The AGL can not be changed. Only the xLOD.
  - Additional Pairs can be added at any AGL and xLOD desired. Pairs will always be sorted by AGL.
  - Plus is Add, Minus is Remove, S is Set (Change). Remove and Set require to double-click the Pair first.
  - A Pair is selected (and the configured xLOD applied) when the current AGL is above the configured AGL. If the current AGL goes below the configured AGL, the next lower Pair will be selected.
  - A new Pair is only selected in Accordance to the VS Trend - i.e. a lower Pair won't be selected if you're actually Climbing (only the next higher)
  - Many users are finding it better to reduce, not increase, OLOD values at higher altitudes as you can't clearly see objects from such distances anyway, especially in VR.
- FPS Adaption:
  - Settings in the FPS adaption area only work if you have checked Limit LODs.
  - FPS Adaption will activate when your FPS is below the target FPS you have set, after any Delay start you have set.
  - Reduce TLOD/OLOD is the maximum values it will reduce those settings by from the current LOD pair values, minimum TLOD/OLOD permitting. If you want to use the Decrease Cloud Quality option without reducing LODs, set these both to 0.
  - Minimum TLOD/OLOD is the minimum values it will allow those settings to reduce to.
  - Delay start is how many seconds of FPS below the target FPS have to occur before FPS Adaption will activate, to stop it false triggering with a transient FPS drop. Default is 1 second but 2 seconds is good too.
  - Reduce for is how many seconds of FPS above the target FPS, plus cloud recover FPS if used, have to occur before FPS Adaption will cancel, to stop it false cancelling with a unsustained FPS increases.
  - Decrease Cloud Quality, when checked, will reduce cloud quality by one level while FPS adaption is active. 
  - Cloud Recovery FPS + is how many FPS to add to the target FPS for determining whether to cancel FPS adaption once activated. This provides an FPS buffer to account for the increased FPS achieved by reducing cloud quality to stop FPS adaption constantly toggling on and off.
- **Less is more**:
  - Fewer Increments/Decrements are better of reasonable Step-Size (roughly in the Range of 25-75) or use Step LOD Max to spread LOD changes out over time.
  - Don't overdo it with extreme low or high xLOD Values. A xLOD of 100 is reasonable fine on Ground, 200-ish is reasonable fine in the air. 400 if you have a super computer.
  - Tune your AGL/LOD Pairs to the desired Performance (which is more than just FPS).
  - FPS Adaption is just *one temporary* Adjustment on the current AGL/xLOD Pair to fight some special/rare Situations.
  - Forcing the Sim to (un)load Objects in rapid Succession defeats the Goal to reduce Stutters. It is *not* about FPS.
  - Smooth Transitions lead to smoother experiences.  
<br/><br/>
