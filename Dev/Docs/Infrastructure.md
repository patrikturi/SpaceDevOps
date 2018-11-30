Increment patch version:
```version --next_patch```

Increment minor version:
```version --next_minor```

Build project:
* Set UNITY_PATH environment variable to Unity folder path
* Make sure Unity is closed
* Windows target: `build Windows64`
* Linux target: `build Linux64`
* The build is created in `out\build\` and also zipped in `out\`
* The build version is automatically read from the git tag created by the version script. If the current commit is not tagged, build version will be "draft".
