using Tesseract;
using Newtonsoft.Json;
using Spire.Pdf;
using System;
using System.IO;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Enter the file path of the document (image or PDF):");
		string filePath = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(filePath))
		{
			Console.WriteLine("File path cannot be empty. Exiting...");
			return;
		}

		if (!File.Exists(filePath))
		{
			Console.WriteLine("File not found. Please provide a valid path.");
			return;
		}

		string extractedText = string.Empty;

		try
		{
			string imagePath = filePath;

			// If the file is a PDF, convert its first page to an image
			if (Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("PDF detected. Converting the first page to an image...");
				imagePath = ConvertPdfToImage(filePath, "temp.jpg");
				Console.WriteLine("PDF converted to image: " + imagePath);
			}

			// Process the image with Tesseract
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

			// Convert extracted text to JSON
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

	// Converts the first page of a PDF to an image (JPEG) using Spire.PDF
	static string ConvertPdfToImage(string pdfPath, string outputImagePath)
	{
		using (PdfDocument pdfDocument = new PdfDocument())
		{
			// Load the PDF document
			pdfDocument.LoadFromFile(pdfPath);

			// Render the first page to an image
			var image = pdfDocument.SaveAsImage(0, 300, 300); // 300 DPI for quality

			// Save the image as a JPEG
			image.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
		}

		return outputImagePath;
	}
}
