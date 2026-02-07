# Release Process

This document describes how to create a new release of GitMaster.

## Prerequisites

- Push access to the main repository
- All changes merged to main/master branch
- All tests passing

## Creating a Release

### 1. Update Version Information

If you have version information in your code, update it first.

### 2. Create and Push a Tag

```bash
# Ensure you're on the main branch
git checkout main
git pull origin main

# Create a new tag (following semantic versioning)
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push the tag to GitHub
git push origin v1.0.0
```

### 3. Automatic Build and Release

Once the tag is pushed:

1. The Release workflow will automatically trigger
2. It will build binaries for:
   - Windows (win-x64)
   - Linux (linux-x64)
   - macOS (osx-x64)
3. A GitHub Release will be created with:
   - Release notes
   - Platform-specific binaries
   - Installation instructions

### 4. Verify the Release

1. Go to https://github.com/Adjanour/git-master/releases
2. Verify the release was created
3. Download and test binaries for each platform
4. Update release notes if needed

## Version Numbering

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version (v2.0.0): Incompatible API changes
- **MINOR** version (v1.1.0): New functionality (backwards compatible)
- **PATCH** version (v1.0.1): Bug fixes (backwards compatible)

## Release Checklist

- [ ] All changes merged to main branch
- [ ] CI builds passing
- [ ] Version updated (if applicable)
- [ ] Tag created and pushed
- [ ] Release workflow completed successfully
- [ ] Binaries downloaded and tested
- [ ] Release notes updated
- [ ] Documentation updated (if needed)

## Troubleshooting

### Release workflow fails

1. Check the Actions tab for error details
2. Verify the tag was created correctly: `git tag -l`
3. Ensure all dependencies are available
4. Check workflow logs for specific errors

### Binary doesn't work

1. Verify the correct runtime identifier was used
2. Test on a clean machine
3. Check for missing dependencies
4. Review build logs

## Post-Release

1. Announce the release (if applicable)
2. Update documentation site
3. Monitor for issues
4. Prepare for next release
