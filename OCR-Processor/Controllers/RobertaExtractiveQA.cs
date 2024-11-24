using LangChain.Providers.HuggingFace;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OCR_Processor.Controllers
{
	public class RobertaExtractiveQA
	{
		private readonly string _modelName;
		private readonly HuggingFaceProvider _provider;
		private const string ApiUrl = "https://api-inference.huggingface.co/models/";
		public RobertaExtractiveQA(HuggingFaceProvider provider, string modelName = "deepset/roberta-base-squad2")
		{
			_modelName = modelName;
			_provider = provider;

		}

		public async Task WarmUpAsync()
		{
			Console.WriteLine("Warming up the model...");
			// No specific warm-up endpoint for Haystack; optional based on API design.
		}

		public async Task<QuestionAnswer> AnswerQuestionAsync(string question, List<Document> documents)
		{
			try
			{
				var answers = new QuestionAnswer();

				foreach (var document in documents)
				{
					var payload = new
					{
						question = question,
						context = document.Content
					};

					var jsonPayload = JsonSerializer.Serialize(payload);
					var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
					// Prepare the request
					using var httpClient = new HttpClient();
					httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_provider.ApiKey}");

					var response = await httpClient.PostAsync($"{ApiUrl}{_modelName}", content);


					if (response.IsSuccessStatusCode)
					{
						var jsonResponse = await response.Content.ReadAsStringAsync();
						Console.WriteLine($"Res: {response.StatusCode} {jsonResponse}");
						var questionAnswerResponse = JsonSerializer.Deserialize<QuestionAnswer>(jsonResponse);

						answers = questionAnswerResponse;
					}
					else
					{
						Console.WriteLine($"Error: {response.ReasonPhrase}");
					}
				}

				return answers;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in AnswerQuestionAsync: {ex.Message}");
				throw;
			}
		}

		public static List<Document> CreateDocuments(List<string> contents)
		{
			var documents = new List<Document>();
			foreach (var content in contents)
			{
				documents.Add(new Document { Content = content });
			}
			return documents;
		}

		public class Document
		{
			public string Content { get; set; }
		}

		public class QuestionAnswer
		{
			[JsonPropertyName("answer")]
			public string Answer { get; set; }

			[JsonPropertyName("score")]
			public float Score { get; set; }
		}

		public class QuestionAnswerResponse
		{
			public QuestionAnswer Answers { get; set; }
		}
	}
}
