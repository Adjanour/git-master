# GitHub Pages Setup Guide

This guide explains how to set up GitHub Pages for the GitMaster documentation.

## Automatic Setup (Already Configured)

The documentation is automatically deployed via GitHub Actions. No manual setup required!

## Manual Setup (If Needed)

If you need to manually configure GitHub Pages:

### 1. Enable GitHub Pages

1. Go to your repository on GitHub
2. Click **Settings**
3. Scroll down to **Pages** (in the left sidebar)
4. Under **Source**, select:
   - **Source**: GitHub Actions

### 2. Verify Workflow Permissions

1. Go to **Settings** → **Actions** → **General**
2. Scroll to **Workflow permissions**
3. Select **Read and write permissions**
4. Check **Allow GitHub Actions to create and approve pull requests**
5. Click **Save**

### 3. Trigger Deployment

The documentation will automatically deploy when:
- Changes are pushed to the `main` or `master` branch
- Changes are made in the `docs/` directory
- The workflow is manually triggered

To manually trigger:
1. Go to **Actions** tab
2. Select **Deploy Documentation** workflow
3. Click **Run workflow**

## Accessing Documentation

Once deployed, your documentation will be available at:
- https://adjanour.github.io/git-master/

## Documentation Structure

```
docs/
├── index.md          # Main documentation page
├── _config.yml       # Jekyll configuration
├── RELEASE.md        # Release process guide
└── GITHUB_PAGES.md   # This file
```

## Customizing the Theme

The documentation uses the Cayman theme. To customize:

1. Edit `docs/_config.yml`
2. Available themes:
   - jekyll-theme-architect
   - jekyll-theme-cayman
   - jekyll-theme-dinky
   - jekyll-theme-hacker
   - jekyll-theme-leap-day
   - jekyll-theme-merlot
   - jekyll-theme-midnight
   - jekyll-theme-minimal
   - jekyll-theme-modernist
   - jekyll-theme-slate
   - jekyll-theme-tactile
   - jekyll-theme-time-machine

3. Update the `theme` value:
   ```yaml
   theme: jekyll-theme-cayman
   ```

## Adding Pages

To add a new page:

1. Create a markdown file in the `docs/` directory
2. Add front matter at the top:
   ```markdown
   ---
   title: Page Title
   ---
   
   # Page Content
   ```

3. Link to it from `index.md`:
   ```markdown
   [Link Text](page-name.md)
   ```

## Local Testing

To test documentation locally:

1. Install Jekyll:
   ```bash
   gem install jekyll bundler
   ```

2. Create a `Gemfile` in the `docs/` directory:
   ```ruby
   source 'https://rubygems.org'
   gem 'github-pages', group: :jekyll_plugins
   ```

3. Install dependencies:
   ```bash
   cd docs
   bundle install
   ```

4. Serve locally:
   ```bash
   bundle exec jekyll serve
   ```

5. Visit http://localhost:4000

## Troubleshooting

### Deployment Fails

1. Check the Actions tab for error logs
2. Verify workflow permissions are set correctly
3. Ensure the docs directory exists and has content
4. Check `_config.yml` syntax

### Page Not Found (404)

1. Wait a few minutes for deployment to complete
2. Check that GitHub Pages is enabled
3. Verify the correct source is selected
4. Clear browser cache

### Styles Not Loading

1. Check `_config.yml` for correct theme name
2. Verify Jekyll can process the markdown
3. Check browser console for errors

## Resources

- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [Jekyll Documentation](https://jekyllrb.com/docs/)
- [Markdown Guide](https://www.markdownguide.org/)
