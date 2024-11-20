using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace OCR_Processor
{
	internal class LocalAIHelpers
	{

		private string _modelPath = @"<Your Model Path>"; // change it to your own model path.

		public LocalAIHelpers(string modelPath)
		{
			_modelPath = modelPath;
		}

		public async Task ProcessModel(string prompt)
		{
			//var prompt = File.ReadAllText("Assets/dan.txt").Trim();

			var parameters = new ModelParams(_modelPath)
			{
				ContextSize = 1024,
				//Seed = 1337,
				GpuLayerCount = 5
			};
			using var model = LLamaWeights.LoadFromFile(parameters);
			using var context = model.CreateContext(parameters);
			var executor = new InstructExecutor(context);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Classification starting...");
			Console.ForegroundColor = ConsoleColor.White;

			var inferenceParams = new InferenceParams() { 
				//Temperature = 0.8f, 
				MaxTokens = 600 };
			prompt = prompt + "Classify the document into one of three categories identity document, fax documents and cat documents. Tell me in which category it belong, be concise.";

			while (true)
			{
				await foreach (var text in executor.InferAsync(prompt, inferenceParams))
				{
					Console.Write(text);
				}
				Console.ForegroundColor = ConsoleColor.Green;
				prompt = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
	}
}
