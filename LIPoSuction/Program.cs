using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LinkedInPostScraper
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            string outputPath;

            if (args.Length >= 2 && (args[0] == "-o" || args[0] == "--output"))
            {
                outputPath = args[1];
            }
            else
            {
                outputPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"linkedin_posts_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                );
            }

            try
            {
                await RunScraper(outputPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task RunScraper(string outputPath)
        {
            var options = new ChromeOptions();
            options.AddArguments("--disable-gpu", "--no-sandbox", "--disable-dev-shm-usage");
            var posts = new List<Post>();

            Console.WriteLine($"Posts will be exported to: {outputPath}");

            using var driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(2));

            try
            {
                Console.WriteLine("Please log in to LinkedIn and complete 2FA when the browser opens...");
                driver.Navigate().GoToUrl("https://www.linkedin.com/login");
                wait.Until(d => d.Url.Contains("linkedin.com/feed") || d.Url.Contains("linkedin.com/search"));

                var memberId = GetMemberId(driver);
                Console.WriteLine($"Found member ID: {memberId}");

                var searchUrl = $"https://www.linkedin.com/search/results/content/?fromMember=%5B%22{memberId}%22%5D&keywords=&origin=FACETED_SEARCH&sortBy=%22date_posted%22";
                driver.Navigate().GoToUrl(searchUrl);
                wait.Until(d => d.FindElements(By.CssSelector("div.update-components-text")).Count > 0);

                var lastHeight = 0L;
                var attempts = 0;
                const int MAX_ATTEMPTS = 50;

                while (attempts < MAX_ATTEMPTS)
                {
                    var posts_elements = driver.FindElements(By.CssSelector("div.update-components-text"));
                    var newPostsFound = false;

                    foreach (var element in posts_elements)
                    {
                        try
                        {
                            var postId = element.FindElement(By.XPath("./ancestor::div[contains(@data-urn, 'urn:li:activity')]"))
                                .GetAttribute("data-urn");
                            var textContent = element.Text;
                            var dateElement = element.FindElement(By.XPath("./ancestor::div[contains(@data-urn, 'urn:li:activity')]//span[contains(@class, 'update-components-actor__sub-description')]"));

                            var post = new Post
                            {
                                Id = postId,
                                Content = textContent,
                                PostDate = dateElement.Text,
                                Url = $"https://www.linkedin.com/feed/update/{postId}"
                            };

                            if (!posts.Any(p => p.Id == postId))
                            {
                                posts.Add(post);
                                newPostsFound = true;
                                Console.WriteLine($"Found post: {textContent.Substring(0, Math.Min(50, textContent.Length))}...");
                                await SavePosts(posts, outputPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing post: {ex.Message}");
                        }
                    }

                    if (newPostsFound) attempts = 0;

                    var height = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                        "window.scrollTo(0, document.body.scrollHeight); return document.body.scrollHeight;");

                    if (height == lastHeight)
                    {
                        attempts++;
                        await Task.Delay(500);
                    }
                    else
                    {
                        lastHeight = height;
                        attempts = 0;
                    }

                    Console.WriteLine($"Found {posts.Count} posts so far...");
                }

                await SavePosts(posts, outputPath);
                Console.WriteLine($"\nExport complete! {posts.Count} posts have been saved to:");
                Console.WriteLine(outputPath);
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                driver.Quit();
            }
        }

        private static string GetMemberId(IWebDriver driver)
        {
            var script = @"
                try {
                    var codeElements = document.getElementsByTagName('code');
                    for (var i = 0; i < codeElements.length; i++) {
                        var element = codeElements[i];
                        if (element.textContent.includes('miniProfile') && element.textContent.includes('publicIdentifier')) {
                            var data = JSON.parse(element.textContent);
                            if (data.data && data.data['*miniProfile']) {
                                return data.data['*miniProfile'].replace('urn:li:fs_miniProfile:', '');
                            }
                        }
                    }
                    return null;
                } catch (e) {
                    console.log('Error:', e);
                    return null;
                }
            ";

            var memberId = ((IJavaScriptExecutor)driver).ExecuteScript(script) as string;

            if (string.IsNullOrEmpty(memberId))
            {
                throw new Exception("Could not find member ID. Please ensure you're logged in to LinkedIn.");
            }

            return memberId;
        }

        private static async Task SavePosts(List<Post> posts, string outputPath)
        {
            var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, json);
        }
    }

    public class Post
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string PostDate { get; set; }
        public string Url { get; set; }
    }
}