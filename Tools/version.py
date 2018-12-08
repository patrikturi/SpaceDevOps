import argparse
import os.path
import re
import subprocess
import sys


SUPPORTED_PYTHON_MAJOR = 3
SUPPORTED_PYTHON_MINOR = 5
PROJECT_SETTINGS_PATH = 'ProjectSettings/ProjectSettings.asset'


if __name__ == '__main__':
    py_ver = sys.version_info
    assert py_ver.major == SUPPORTED_PYTHON_MAJOR and py_ver.minor >= SUPPORTED_PYTHON_MINOR, \
           'This script needs python {}.{} or newer'.format(SUPPORTED_PYTHON_MAJOR, SUPPORTED_PYTHON_MINOR)

    parser = argparse.ArgumentParser(description='Manipulate project version')
    parser.add_argument('--next_minor', action='store_true', help='Increment minor version')
    parser.add_argument('--next_patch', action='store_true', help='Increment patch version')
    parser.add_argument('--dry_run', action='store_true', help='Do not commit')
    args = parser.parse_args()

    if args.next_minor and args.next_patch:
        raise argparse.ArgumentError('next_minor and next_patch shall not be specified at once')

    assert os.path.exists(PROJECT_SETTINGS_PATH), 'ProjectSettings not found, this script must run from the project root directory'

    # Read previous version
    version_line = None
    with open(PROJECT_SETTINGS_PATH, 'r', newline='') as settings_file:
        settings_file_str = settings_file.read()
        settings_file.seek(0)
        for line in settings_file:
            m = re.match(r'\s*(bundleVersion\s*:\s*((\d+)\.(\d+)\.(\d+)))', line)
            if m:
                version_line = m.group(1)
                cur_version = m.group(2)
                major = int(m.group(3))
                minor = int(m.group(4))
                patch = int(m.group(5))
                break

    assert version_line, 'Project version was not found in {}'.format(PROJECT_SETTINGS_PATH)
    print('Current version: {}'.format(cur_version))

    # Increment version
    mod_version_str = None
    if args.next_minor:
        minor += 1
        patch = 0
        mod_version_str = 'minor'
    elif args.next_patch:
        patch += 1
        mod_version_str = 'patch'

    if not mod_version_str:
        exit(0)

    new_version = '{}.{}.{}'.format(major, minor, patch)
    new_version_line = version_line.replace(cur_version, new_version)
    settings_file_str = settings_file_str.replace(version_line, new_version_line)
    print('New version: {}'.format(new_version))

    # Write new version
    with open(PROJECT_SETTINGS_PATH, 'w', newline='') as settings_file:
        settings_file.write(settings_file_str)

    # Add, commit, tag
    print('Commit')
    git_add_cmd = 'git add {}'.format(PROJECT_SETTINGS_PATH)
    subprocess.check_output(git_add_cmd.split())

    commit_msg = 'Increment {} version to {}'.format(mod_version_str, new_version)
    git_commit_cmd = ['git', 'commit', '-m', commit_msg]
    if args.dry_run:
        git_commit_cmd.append('--dry-run')
    subprocess.check_output(git_commit_cmd)

    git_tag_cmd = 'git tag v{}'.format(new_version)
    if not args.dry_run:
        subprocess.check_output(git_tag_cmd.split())

    done_msg = 'Done' if not args.dry_run else 'Done (dry run)'
    print(done_msg)
