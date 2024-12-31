# LIPoSuction - LinkedIn Post Suction Tool

LIPoSuction is a simple tool that helps you extract and export all your LinkedIn posts into a JSON file. It automates the process of scrolling through your post history and saves everything in an easily accessible format.

Made by Bjørn Furuknap, furuknap@gmail.com, with the caring and loving (not really) assistance of Claude.

"I helped write this quote too, but honestly, the rest of the README is much more interesting." - Claude

## Rationale

LinkedIn Data Export does not include your own posts. I needed those posts to do analysis and train a language model to ensure my posts maintain a consistent voice. However, even as a citizen of a GDPR country, LinkedIn did not give me an easy way to get these posts, and it is impossible for countries without such data protection.

Thus became LIPoSuction a free gift from me to you, to get access to your own data in parts of the world where LinkedIn does not think your data is yours.

## Prerequisites

- Windows operating system
- Google Chrome browser installed
- .NET 7.0 Runtime or higher installed

## Installation

1. Download the latest release from the [Releases page](../../releases)
2. Extract the ZIP file to any folder
3. That's it! No installation needed.

## Usage

### Simple Method (Recommended)

1. Double-click `LIPoSuction.exe`
2. Log in to LinkedIn when the Chrome browser opens
3. Complete any 2FA if required
4. Wait while the tool collects your posts
5. Find your posts in the JSON file created in the same folder as the program

### Advanced Usage (Command Line)

You can specify a custom output location using the command line parameter `-o` followed by the path:

```bash
LIPoSuction.exe -o "C:\MyFolder\linkedin_posts.json"
```

That's all there is to it! If you don't specify an output file, LIPoSuction will create one in its folder using the pattern `linkedin_posts_YYYYMMDD_HHMMSS.json`.

## Output Format

The tool creates a JSON file containing an array of posts. Each post includes:

- Post ID
- Content
- Post Date
- URL to the original post

Example:

```json
[
  {
    "Id": "urn:li:activity:1234567890",
    "Content": "This is a LinkedIn post...",
    "PostDate": "1w",
    "Url": "https://www.linkedin.com/feed/update/urn:li:activity:1234567890"
  }
]
```

## How It Works

LIPoSuction:

1. Opens a Chrome browser window
2. Waits for you to log in to LinkedIn
3. Identifies your member ID
4. Navigates to your posts
5. Automatically scrolls through your post history
6. Saves each post as it's found
7. Continues until no more posts are available

## Common Issues

1. **Chrome fails to start**: Make sure Google Chrome is installed and up to date
2. **Login issues**: The tool requires you to manually log in to ensure security
3. **No posts found**: Verify you're properly logged in to LinkedIn

## Contributing

Contributions are welcome! Feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
