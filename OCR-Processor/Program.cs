using Tesseract;
using LangChain.Providers.HuggingFace;
using static OCR_Processor.Controllers.PDFToImageConverter;
using static OCR_Processor.Controllers.RobertaExtractiveQA;
using OCR_Processor.Controllers;

class Program
{
	static async Task Main(string[] args)
	{
		using var client = new HttpClient();
		string apiToken = "test_token";
		var provider = new HuggingFaceProvider(apiKey: apiToken, client);

		Console.WriteLine("Enter the file path of the document (image or PDF):");
		string filePath = Path.Combine("C:", "Users", "Denilson", "Downloads", "otjiherero stuff" , "dict.pdf");

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

		List<string> extractedTexts = new List<string>();

		try
		{
			// Check if the file is a PDF
			if (Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("PDF detected. Converting all pages to images...");
				List<string> imagePaths =  ConvertPdfToImages(filePath);
				Console.WriteLine($"PDF converted to {imagePaths.Count} images");

				// Process each image with Tesseract OCR
				foreach (var imagePath in imagePaths)
				{
					string pageText = ExtractTextFromImage(imagePath);
					extractedTexts.Add(pageText);

					// Optional: Clean up temporary image files
					File.Delete(imagePath);
				}
			}
			else
			{
				// If it's not a PDF, process the single image
				string pageText = ExtractTextFromImage(filePath);
				extractedTexts.Add(pageText);
			}

			// Combine all extracted texts for classification and Q&A
			string combinedText = string.Join("\n\n", extractedTexts);

			Console.WriteLine($"Extracted text is: {combinedText}");

			// Document Classification
			BartZeroShotClassification bartClassifier = new BartZeroShotClassification(provider);
			List<string> categories = new List<string> { "Identity Document", "Financial Document", "Legal Document", "Other" };
			var classificationResult = await bartClassifier.ClassifyAsync(combinedText, categories, true);

			Console.WriteLine($"Document Classification: {classificationResult.Labels[0]}, Score: {classificationResult.Scores[0]}");

			// Question Answering Setup
			RobertaExtractiveQA qaSystem = new RobertaExtractiveQA(provider);
			var documents = CreateDocuments(new List<string> { combinedText });

			// Interactive Q&A loop
			while (true)
			{
				Console.WriteLine("Enter your question (or 'exit' to quit): ");
				string question = Console.ReadLine();

				if (question.ToLower() == "exit")
					break;

				QuestionAnswer answer = await qaSystem.AnswerQuestionAsync(question, documents);
				Console.WriteLine($"Answer: {answer.Answer}, Score: {answer.Score}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			Console.WriteLine("Ensure the file path is correct and the file is accessible.");
		}
	}

	
	// Extract text from a single image using Tesseract
	static string ExtractTextFromImage(string imagePath)
	{
		using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
		{
			using (var img = Pix.LoadFromFile(imagePath))
			{
				using (var page = engine.Process(img))
				{
					return page.GetText();
				}
			}
		}
	}
}