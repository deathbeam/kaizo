# Todo

### Tasks

#### `run` - new
This task should probably depend on `build` task and then get final built assembly and run it (of course only when final assembly type is `exe`)

#### `clean` - improve
Need to improve this task, because right now it is only very cheap solution which is deleting entire folders and it is working perfectly only for root project

#### `build` - improve
In this task subproject dependencies in `dependencies` table are not working at all. I do not know how to add them as references without generating temporary `.csproj` files.

### Core

#### `tasks` command-line command
This command should print all available tasks. I am not sure how I would do this because current task system based on Lua modules and global functions probably do not allows this.

#### `configuration` property
Allow more customization and more value types (right now you can only define MSBuild properties)
