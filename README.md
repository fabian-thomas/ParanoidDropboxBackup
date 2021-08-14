# ParanoidDropboxBackup
Backup your Dropbox cloud files to any Linux Server

## Prerequisites

- Linux (64 bit)
- [.NET Core](https://dotnet.microsoft.com/download) x64 runtime

## Setup

The application is designed to run as a systemd service, but you should have no problem using it as a normal console application. So just running it without the following setup steps should work totally fine, but then you have to manage running the application at specific times yourself.

Download the latest release files and extract them to `~/ParanoidDropboxBackup` (or another folder of your choice).

Run the app for the first time: `~/ParanoidDropboxBackup/ParanoidDropboxBackup`

The app copies the default config file to your application data folder. Edit it to your needs: `nano ~/.config/ParanoidDropboxBackup/config.json`. You can find more information on the Dropbox API app key at the [Dropbox API section](#dropbox-api) of this readme.

Now run the app another time. If your config file is valid the app prints instructions on how to authorize in the console. Once you have authorized the app to read your Dropbox files it should start the first backup directly. (You can cancel it if you want)

##### Registering the app to systemd:

Copy the two systemd files (.service, .timer) to `/etc/systemd/system` and modify them to your needs. The default files specify that the app should do a backup every day at 4 pm.

Start the service with `systemctl start ParanoidDropboxBackup.service` and check that there are no errors in the log with `journalctl -u ParanoidDropboxBackup`. Optionally: Stop the service with `systemctl stop ParanoidDropboxBackup.service`.

Enable the timer service with `systemctl enable ParanoidDropboxBackup.timer`. Systemd automatically starts the timer again on system startup.

## Configuration

### Dropbox API

The application uses the Dropbox API to backup your files.

#### Permissions

The app uses 3 permissions:

| Permission          | Description                                                                                 | Why the appliction needs it                   |
| ------------------- | ------------------------------------------------------------------------------------------- | --------------------------------------------- |
| account_info.read   | View basic information about your Dropbox account such as your username, email, and country | default permission                            |
| files.content.read  | View content of your Dropbox files and folders                                              | needed to download your Dropbox files         |
| files.metadata.read | View information about your Dropbox files and folders                                       | files.content.read depends on this permission |

#### App Key

You can try using my registered app with the app key: `wj2nybspqjdtnvd`

If this app key does not work for you, you can register your own app following in the Dropbox [App Console](https://www.dropbox.com/developers/apps) and these instructions:

You need to specify the necessary scopes of your the application. You can do that in the "Permissions" tab. Select all permissions in the [Permissions](#permsissions) section of this readme.

Finally copy the app key in the "Settings" tab and insert it into your config file.

### Ignore File

The ignore file works just like [gitignore](https://git-scm.com/docs/gitignore). All files or folders that match the rules in the ignore file are not being downloaded.

## Implementation Notes

- The config file is stored at: `~/.config/ParanoidDropboxBackup/config.json`
- The ignore file is stored at: `~/.config/ParanoidDropboxBackup/ignore`
- The token cache file is stored at: `~/.cache/ParanoidDropboxBackup/token_cache`
- The deleting of the oldest backups operates on the folder names of the backups so it's safe to just rename a backup folder to keep it "forever". Folders with invalid suffixes are being ignored.
