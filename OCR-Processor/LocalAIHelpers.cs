using LLama;
using LLama.Common;
using LLama.Sampling;
using System;
using System.Threading.Tasks;

namespace OCR_Processor
{
	internal class LocalAIHelpers
	{
		private string _modelPath;

		public LocalAIHelpers(string modelPath)
		{
			_modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
		}

		public async Task<string> ProcessModelAsync(string documentText)
		{
			try
			{
				string modelPath = _modelPath;

				var prompt = documentText;

				var parameters = new ModelParams(modelPath)
				{
					GpuLayerCount = 5
				};
				using var model = await LLamaWeights.LoadFromFileAsync(parameters);
				using var context = model.CreateContext(parameters);
				var executor = new InstructExecutor(context);

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Beginning inference...");
				Console.ForegroundColor = ConsoleColor.White;

				var inferenceParams = new InferenceParams
				{
					SamplingPipeline = new DefaultSamplingPipeline
					{
						Temperature = 0.8f
					},
					MaxTokens = 600
				};
				string result = string.Empty;
				prompt = prompt + "The provided data is from a document that has been OCR'd using tessaract and has been converted to text as a string." +
					"I want to use AI to do classification of documents and extract important information from them." +
					". Classify the provided document into a class of documents: Identity Document, Financial Document, or Legal Document." +
					" Do not provide reasons for your classification";
				int counter = 0;
				while (counter < 2)
				{
					await foreach (var text in executor.InferAsync(prompt, inferenceParams))
					{
						Console.Write(text);
						result += text;
					}

					Console.ForegroundColor = ConsoleColor.Green;
					prompt = "Return only a JSON as a string containing information contained in the document such as names, addresses, phone numbers," +
						" dates and any important numbers as fields as well as the class of the document as a field on the JSON: Identity Document, Financial Document," +
						" or Legal Document. Please Capture all of the information on the document in the JSON." +
						"Only return class the document belongs to classify the document based on the information it contains";
					counter++;
					Console.ForegroundColor = ConsoleColor.White;
				}

				return result.ToString().Trim();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error processing model: {ex.Message}");
				throw;
			}
		}
	}
}