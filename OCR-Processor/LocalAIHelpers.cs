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
				prompt = prompt + "Classify the provided document into a class of documents: Identity Document, Financial Document, or Legal Document. Do not provide reasons for your classification";
				int counter = 0;
				while (counter < 2)
				{
					await foreach (var text in executor.InferAsync(prompt, inferenceParams))
					{
						Console.Write(text);
						result += text;
					}

					Console.ForegroundColor = ConsoleColor.Green;
					prompt = "Return only a json as a string containing the document info as well as the class of the document: Identity Document, Financial Document, or Legal Document.";
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