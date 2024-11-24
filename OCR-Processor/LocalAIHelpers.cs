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
				prompt = prompt + "Classify the provided document into one of the following categories: Identity Document, "+
					"Financial Document, or Legal Document. Return only the category name as the output, without any explanation or reasoning";
				int counter = 0;
				while (counter < 2)
				{
					await foreach (var text in executor.InferAsync(prompt, inferenceParams))
					{
						Console.Write(text);
						result += text;
					}

					Console.ForegroundColor = ConsoleColor.Green;
					prompt = "Analyze the provided document and extract all relevant important information, "+
						"such as names, contact information, dates, and any other key details. Classify the document" +
						" into one of the following categories: Identity Document, Financial Document, or Legal Document."+
						" Return the output as a JSON object containing the extracted information along with the document class."+ 
						"Do not include any additional explanations or reasoning. ";
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