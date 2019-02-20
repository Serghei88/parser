using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebParser.Model;

namespace WebParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SetConsoleOutputToFile("out.txt");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            var ids = new List<int>();
            var checkedProjectTemplatesIds = new List<int>();
            using (var context = new MixbookDbContext(configuration))
            {
                var projectTemplates = context.ProjectTemplate
                    .Where(pt => pt.Project != null && pt.Project.Pages.Count > 0 && pt.IsDeleted == 0)
                    .Include(x => x.Project.Pages).ToList();
                foreach (var projectTemplate in projectTemplates)
                {
                    checkedProjectTemplatesIds.Add(projectTemplate.ID);
                    Console.WriteLine(
                        $"{DateTime.Now} Checking project: {checkedProjectTemplatesIds.Count} id = {projectTemplate.ID} from {projectTemplates.Count} " +
                        $"Total progress = {((float) checkedProjectTemplatesIds.Count / (float) projectTemplates.Count) * 100}%");

                    if (!await CheckProjectTemplateImages(projectTemplate))
                    {
                        ids.Add(projectTemplate.ID);
                    }
                }
            }

            Console.WriteLine(String.Join(", ", ids.Distinct().ToArray()));
        }

        private static async Task<bool> CheckProjectTemplateImages(ProjectTemplate projectTemplate)
        {
            var imageUrls = GenerateUrls(projectTemplate.ID, projectTemplate.Project.Pages.Count);
            var tasks = imageUrls.Select(ImageMagicHelper.CheckImageExistsAndNotBroken);
            var results = await Task.WhenAll(tasks);
            return !results.Contains(false);
        }

        private static IList<string> GenerateUrls(int templateId, int numberOfPages)
        {
            var result = new List<string>();
            for (var i = 0; i < numberOfPages - 1; i++)
            {
                result.Add($"http://cdn.mixbook.com/pages/{templateId}_{i}_l.jpg");
            }

            return result;
        }

        private static async Task<bool> ImageNotGrayed(string imageUrl)
        {
            var output = await
                $"convert {imageUrl} -gravity SouthWest -crop 20%x1%   -format %c  -depth 8  histogram:info:- | sed \'/^$/d\'  | sort -V | head -n 1 | grep fractal | wc -l"
                    .Bash();
            Console.WriteLine($"Checking {imageUrl}, valid = {output.Contains("0") && !output.Contains("error")}");
            return output.Contains("0") && !output.Contains("error");
        }

        private static async Task<bool> ImageNotCorrupted(string imageUrl)
        {
            await $"identify -regard-warnings -verbose {imageUrl} > /dev/null 2>&1".Bash();
            var output = await "echo $?".Bash();
            return output.Contains("0");
        }

        private static string GetTemplateIdFromUrl(string url)
        {
            return url.Substring(url.IndexOf("pages/") + 6, url.IndexOf("_") - (url.IndexOf("pages/") + 6));
        }

        private static void SetConsoleOutputToFile(string fileName)
        {
            var fileStream = new FileStream(fileName, FileMode.Create);
            var streamWriter = new StreamWriter(fileStream) {AutoFlush = true};
            Console.SetOut(streamWriter);
        }
    }
}