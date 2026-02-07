# GitMaster Documentation

This directory contains the documentation for GitMaster, deployed to GitHub Pages.

## Documentation Files

- **[index.md](index.md)** - Main documentation page with command reference and examples
- **[RELEASE.md](RELEASE.md)** - Guide for creating and managing releases
- **[GITHUB_PAGES.md](GITHUB_PAGES.md)** - Instructions for GitHub Pages setup and configuration
- **[_config.yml](_config.yml)** - Jekyll configuration for GitHub Pages

## Viewing Documentation

### Online
The documentation is automatically deployed to: https://adjanour.github.io/git-master/

### Locally
To preview documentation locally:

```bash
# Install Jekyll
gem install jekyll bundler

# Create Gemfile (if not exists)
cat > Gemfile << EOF
source 'https://rubygems.org'
gem 'github-pages', group: :jekyll_plugins
EOF

# Install dependencies
bundle install

# Serve locally
bundle exec jekyll serve

# Visit http://localhost:4000
```

## Updating Documentation

1. Edit the markdown files in this directory
2. Commit and push changes to main/master branch
3. GitHub Actions will automatically rebuild and deploy

### Adding New Pages

1. Create a new `.md` file in this directory
2. Add front matter (optional):
   ```markdown
   ---
   title: Page Title
   layout: default
   ---
   ```
3. Link to it from `index.md`

## Structure

```
docs/
├── README.md           # This file
├── index.md           # Main documentation page
├── RELEASE.md         # Release process guide
├── GITHUB_PAGES.md    # GitHub Pages setup guide
└── _config.yml        # Jekyll configuration
```

## Deployment

Documentation is deployed via GitHub Actions workflow:
- Workflow: `.github/workflows/docs.yml`
- Triggers: Push to main/master, changes in docs/, manual trigger
- Engine: Jekyll with Cayman theme

## Theme

The documentation uses the Cayman theme. To change the theme, edit `_config.yml`:

```yaml
theme: jekyll-theme-cayman  # Change to another supported theme
```

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for contribution guidelines.
