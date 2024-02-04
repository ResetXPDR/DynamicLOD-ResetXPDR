# DynamicLOD_ResetEdition

Based on muumimorko's idea and code in MSFS_AdaptiveLOD, as further developed by Fragtality in DynamicLOD.<br/><br/>
This utility builds upon the functionality provided in DynamicLOD, which aims to improve MSFS performance and smoothness by dynamically changing the TLOD (and OLOD) based on the current AGL, and provides additonal features such as:<br/>
- Simultaneous PC and VR mode compatibilty,<br/>
- Optional LOD updates in cruise,<br/> 
- Optional incremental steps between LOD changes to improve smoothness,<br/>
- Optional cloud quality decrease with FPS Adaption,<br/>
- Enhanced FPS Adaption control,<br/>
- Auto restoration of original settings changed by the utility,<br/>
- Removal of redundant features, and<br/>
- Minor UI changes.<br/><br/>

If you are not familiar with what MSFS graphics settings do, specifically TLOD, OLOD and cloud quality, and don't understand the consequences of changing them, it is highly recommended you do not use this utility.
<br/><br/>

## Requirements

The Installer will install the following Software:
- .NET 7 Desktop Runtime (x64)
- MobiFlight Event/WASM Module

<br/>

[Download here](https://github.com/ResetXPDR/DynamicLOD_ResetEdition/releases/latest)

(Under Assests, the DynamicLOD_ResetEdition-Installer-vXYZ.exe File)

<br/><br/>

## Installation / Update
Basically: Just run the Installer.<br/>

Some Notes:
- DynamicLOD has to be stopped before installing.
- If the MobiFlight Module is not installed or outdated, MSFS also has to be stopped.
- If you upgrade from Fragtality's Version 0.3.0 or below, delete your old Installation manually (it is no longer needed).
- From Fragtality's Version 0.3.0 onwards, your Configuration is *not* be resetted after Updating.
- Do not copy over a Configuration from a Version below 0.3.0
- Do not run the Installer as Admin!
- For Auto-Start either your FSUIPC7.ini or EXE.xml (MSFS) is modified. The Installer does not create a Backup.
- It may be blocked by Windows Security or your AV-Scanner, try if unblocking and/or setting an Exception helps (for the whole Folder)
- The Installation-Location is fixed to %appdata%\DynamicLOD (your Users AppData\Roaming Folder) and can not be changed.
  - Binary in %appdata%\DynamicLOD_ResetEdition\bin
  - Logs in %appdata%\DynamicLOD_ResetEdition\log
  - Config: %appdata%\DynamicLOD_ResetEdition\DynamicLOD_ResetEdition.config

<br/><br/>

## Usage / Configuration

- Starting manually: before MSFS or in the Main Menu. It will stop itself when MSFS closes. 
- Closing the Window does not close the Program, use the Context Menu of the SysTray Icon.
- Clicking on the SysTray Icon opens the Window (again).
- Runnning as Admin NOT required (BUT: It is required to be run under the same User/Elevation as MSFS).
- You can have (exactly) six different Sets/Profiles for the AGL/LOD Pairs to switch between (manually but dynamically).
- The first Pair with AGL 0 can not be deleted. The AGL can not be changed. Only the xLOD.
- Additional Pairs can be added at any AGL and xLOD desired. Pairs will always be sorted by AGL.
- Plus is Add, Minus is Remove, S is Set (Change). Remove and Set require to double-click the Pair first.
- A Pair is selected (and the configured xLOD applied) when the current AGL is above the configured AGL. If the current AGL goes below the configured AGL, the next lower Pair will be selected.
- A new Pair is only selected in Accordance to the VS Trend - i.e. a lower Pair won't be selected if you're actually Climbing (only the next higher)
- **Less is more**:
  - Fewer Increments/Decrements are better. Of reasonable Step-Size (roughly in the Range of 25-75).
  - Some Time in between Changes is better.
  - Don't overdo it with extreme low or high xLOD Values. A xLOD of 100 is reasonable fine on Ground, 200-ish is reasonable fine in Air.
  - Tune your AGL/LOD Pairs to the desired Performance (which is more than just FPS).
  - FPS Adaption is just *one temporary* Adjustment on the current AGL/xLOD Pair to fight some special/rare Situations.
  - Forcing the Sim to (un)load Objects in rapid Succession defeats the Goal to reduce Stutters. It is *not* about FPS.
  - Smooth Transitions lead to smoother experiences.  

<br/><br/>
