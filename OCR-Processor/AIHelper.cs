using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCR_Processor
{
	public class AIHelper
	{
		private const string ApiKey = "";
		private const string OpenAiEndpoint = "https://api.openai.com/v1/completions";

		public async Task<string> ProcessText(string inputText)
		{

			string prompt = $"Convert the following information into a structured JSON format:\n\n{inputText}\n\nJSON:";
			string response = await GenerateJsonFromText(prompt);

			Console.WriteLine("Generated JSON:");
			Console.WriteLine(response);
			return response;
		}

		private static async Task<string> GenerateJsonFromText(string prompt)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

				var requestBody = new
				{
					model = "gpt-3.5-turbo", // Use "gpt-3.5-turbo" for faster/cheaper responses
					prompt = prompt,
					max_tokens = 300,
					temperature = 0
				};

				string jsonBody = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

				HttpResponseMessage response = await client.PostAsync(OpenAiEndpoint, content);
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
				}

				string responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

				return result.GetProperty("choices")[0].GetProperty("text").ToString().Trim();
			}
		}
	}
}
