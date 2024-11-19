using Tesseract;
using Newtonsoft.Json;
using System;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Enter the file path of the image:");
		string imagePath = Console.ReadLine(); // Get file path from user input

		if (string.IsNullOrWhiteSpace(imagePath))
		{
			Console.WriteLine("File path cannot be empty. Exiting...");
			return;
		}

		string extractedText = string.Empty;

		try
		{
			using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
			{
				using (var img = Pix.LoadFromFile(imagePath))
				{
					using (var page = engine.Process(img))
					{
						extractedText = page.GetText();
					}
				}
			}

			var jsonObject = new
			{
				OriginalText = extractedText,
				ExtractedLines = extractedText.Split(Environment.NewLine)
			};

			string jsonOutput = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
			Console.WriteLine("\nExtracted Text in JSON Format:\n");
			Console.WriteLine(jsonOutput);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			Console.WriteLine("Ensure the file path is correct and the file is accessible.");
		}
	}
}
