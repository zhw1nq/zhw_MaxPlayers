# Automatic live hot-reloading

During development of your plugin, you may find yourself repeating a workflow 
similar to the following:
1. Make a change to your plugin
2. Run your build task (ex. `dotnet build`)
3. Upload plugin DLLs to your server using an FTP client
4. Alt-tab to the game
5. Test your changes
6. Repeat

Iterating on your plugin this way is painfully slow and is impacting your productivity.
Below, you will find a guide and recommendations on how to setup your dev environment
to watch for file changes and automatically update plugin files on your server as you work.
By following this guide, your new workflow should look like this:
1. Make a change to your plugin
2. Alt-tab to the game
3. Test your changes
4. Repeat

> [!CAUTION]
> Exercise caution when developing your plugin while using automatic live hot-reloading.
> Build time errors are mostly caught by .NET SDK before files are committed 
> but incomplete implementation may lead to issues such as server crashes.
> Avoid using this workflow on a production server meant for players.

## Setup

#### 1. Build plugin on change

##### Using `dotnet` CLI
The `dotnet` CLI, included with the .NET SDK, offers a convenient command for 
automatically watching for source file changes. If you have access to the `dotnet`
CLI, run the following command to start watching your source code.
```shell
dotnet watch build --project src/zhw_MaxPlayers.csproj
```
<sup>By default, `dotnet watch` executes the `dotnet run` command on file changes
so specifying `build` as the first argument is required.</sup>

> Your plugin will now build automatically on file change to the `<OutDir>` as 
> specified in your `zhw_MaxPlayers.csproj` file.

#### 2. Setup automatic uploads

##### Using WinSCP (Windows only)
Once connected to your server:
1. Go to the `Commands` tab at the top of the WinSCP window
and click `Keep Remote Directory up to Date`.
2. Select the plugin build directory containing your DLLs.
3. Select the plugin destination.
(`csgo/addons/counterstrikesharp/plugins/zhw_MaxPlayers`)
4. Click `Start`

> [!IMPORTANT]
> **For WSL users:**
> Applications running on Windows, such as WinSCP, cannot watch your Linux subsystem for file
> changes. Try using [this workaround](#using-winscp-while-developing-in-wsl) or consider
> moving development to Windows.

##### Using `lsyncd` (Linux)
> **TODO:** in-depth guide for setting up lsyncd

Learn more: https://github.com/lsyncd/lsyncd

___

#### Using WinSCP while developing in WSL
Run the following watch command in place of the one mentioned in 
[Step 1](#1-build-plugin-on-change) to build to a directory in your Windows filesystem
```shell
dotnet watch build --project src/zhw_MaxPlayers.csproj --property:OutDir=/mnt/<drive-letter>/some/path/zhw_MaxPlayers`
```
and have [WinSCP in Step 2](#2-setup-automatic-uploads) watch that path instead.

[Learn about Windows filesystem mounts in WSL](https://blogs.windows.com/windowsdeveloper/2016/07/22/fun-with-the-windows-subsystem-for-linux/#Working%20with%20Windows%20files:~:text=Working%20with%20Windows%20files)
