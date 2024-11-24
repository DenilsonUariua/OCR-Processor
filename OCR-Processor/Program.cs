using Tesseract;
using Spire.Pdf;
using LangChain.Providers.HuggingFace;
using OCR_Processor.Controllers;
using static OCR_Processor.Controllers.RobertaExtractiveQA;

class Program
{
	static async Task Main(string[] args)
	{
		using var client = new HttpClient();
		string apiToken = "hugging_face_api_key_here";
		var provider = new HuggingFaceProvider(apiKey: apiToken, client);

		Console.WriteLine("Enter the file path of the document (image or PDF):");
		string filePath = "file_path_of_the_document";

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

			// Process the image with Tesseract OCR to extract text
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

			// This handle the document classification using the extracted text
			BartZeroShotClassification bartClassifier = new BartZeroShotClassification(provider);
			List<string > categories = new List<string> { "Identity Document", "Financial Document", "Legal Document", "Other" };
			var classificationResult = await bartClassifier.ClassifyAsync(extractedText, categories, true );
			
			// Prints out the classification of the document and its score
			Console.WriteLine($"It is a : {classificationResult.Labels[0]}, Score: {classificationResult.Scores[0]}");

			// This handles the question-answering using the extracted text
			RobertaExtractiveQA qaSystem = new RobertaExtractiveQA(provider);

			await qaSystem.WarmUpAsync();

			var documents = CreateDocuments(new List<string>
		{
			extractedText,

		});
            Console.WriteLine("Enter your question: ");
            string question = Console.ReadLine();

			//function that does the question-answering
			QuestionAnswer answer = await qaSystem.AnswerQuestionAsync(question, documents);

			Console.WriteLine($"Answer: {answer.Answer}, Score: {answer.Score}");

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
