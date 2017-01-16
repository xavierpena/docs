using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WikidotParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // CONFIGURATION:
            var sourceFolder = @"E:\git-xavierpenya\docs\temp";
            var destinationFolder = @"E:\git-xavierpenya\docs\pages";

            try
            {
                Console.WriteLine("Starting...");
                var files = Directory.GetFiles(sourceFolder);
                foreach(var file in files)
                    Process(file, destinationFolder);
                Console.WriteLine("Finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: " + ex.ToString());
            }

            Console.WriteLine("Press any key ...");
            Console.ReadKey();            
        }

        private static void Process(string sourceFilePath, string destinationFolder)
        {
            var fileInfo = new FileInfo(sourceFilePath);
            var title = GetTitle(fileInfo);
            var sourceLines = File.ReadAllLines(sourceFilePath);
            var resultText = GetRepairedText(title, sourceLines);
            var destinationFilePath = Path.Combine(destinationFolder, fileInfo.Name);
            File.WriteAllText(destinationFilePath, resultText);
        }


        private static string GetRepairedText(string title, string[] sourceLines)
        {
            var repairedLines = new List<string>();
            repairedLines.Add($"# {title}\r\n");
            var isCode = false;
            foreach (var line in sourceLines)
            {
                if (line.Contains("+ Comments"))
                    break;

                var repairedLine = line
                    .Replace("+++ ", "#### ")
                    .Replace("++ ", "### ")
                    .Replace("+ ", "## ");

                repairedLine = ReformatLinks(repairedLine);

                if (line.Contains("[[toc]]"))
                    continue;

                if (line.Contains("[[code"))
                {
                    isCode = true;
                    continue;
                }

                if (line.Contains("[[/code"))
                {
                    isCode = false;
                    continue;
                }

                if (isCode)
                    repairedLines.Add($"\t{line}");
                else
                    repairedLines.Add(repairedLine);
            }

            var resultText = string.Join("\r\n", repairedLines);
            return resultText;
        }

        private static string ReformatLinks(string line)
        {
            var sanitizedLine = line
                .Replace("[[[", "$[[[")
                .Replace("]]]", "$");
            //      0       1           2       3       4
            // <- before -> link <-> after <-> link <-> after
            var cells = sanitizedLine.Split('$');
            for(var index = 0; index < cells.Length - 1; index ++ )
            {
                if(cells[index].StartsWith("[[["))
                {
                    var innerCells = cells[index]
                        .Replace("[[[", "")
                        .Split('|');

                    var url = innerCells[0];
                    var description = innerCells[1];

                    cells[index] = $"[{description}]({url})";
                }                
            }

            return string.Join("", cells);
        }
        
        private static string GetTitle(FileInfo fileInfo)
        {
            var titleVector = fileInfo.Name
                .Replace(fileInfo.Extension, "")
                .Split(' ')
                .Skip(1);
            var title = string.Join(" ", titleVector);
            return title;
        }
    }
}
