# DirectoryPropSwitch

.NET Core CLI to enable/disable xml key in the Directory.Build.props.
Enble/Disable is done by comment out or not.
You must set these property first for each Directory.Build.props.

## Usage

Enable PathMap for your Directory.Build.props under `c:/git/your_project_path` and recursive.
This will remove comment for the PathMap.

```shell
$ directorypropswitch enable -k PathMap -f c:/git/your_project_path
```

Disable PathMap for your Directory.Build.props under `c:/git/your_project_path` and recursive.
This will add comment for the PathMap.

```shell
$ directorypropswitch disable -k PathMap -f your_project_path
```

Toggle PathMap status for your Directory.Build.props under `c:/git/your_project_path` and recursive.
This will toggle PathMap, if already commented out then remove comment, if already removed comment then commennt out it.

```shell
$ directorypropswitch toggle -k PathMap -f your_project_path
```

