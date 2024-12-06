using LangChain.Providers.HuggingFace;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace OCR_Processor.Controllers
{
	public class BartZeroShotClassification
	{
		private readonly string _modelName;
		private readonly HttpClient _httpClient;
		private readonly HuggingFaceProvider _provider;
		private const string ApiUrl = "https://api-inference.huggingface.co/models/";

		public BartZeroShotClassification(HuggingFaceProvider provider, string modelName = "facebook/bart-large-mnli")
		{
			_modelName = modelName;
			_httpClient = new HttpClient();
			_provider = provider;

		}

		public async Task<ZeroShotClassificationResult> ClassifyAsync(string sequence, List<string> candidateLabels, bool multiLabel = false)
		{
			try
			{
				var payload = new
				{
					inputs = sequence,
					parameters = new
					{
						candidate_labels = string.Join(",", candidateLabels),
						multi_label = multiLabel
					}
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
                   
                    var result = JsonSerializer.Deserialize<ZeroShotClassificationResult>(jsonResponse);
					//find the index of the highest score and return the corresponding label
					var maxIndex = result.Scores.IndexOf(result.Scores.Max());

					result.Labels = new List<string> { result.Labels[maxIndex] };
                    // log the result
                    Console.WriteLine($"Result: {result.Labels[0]}");
                    return result;
				}
				else
				{
					Console.WriteLine($"Error: {response.ReasonPhrase}");
					throw new Exception(response.ReasonPhrase);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in ClassifyAsync: {ex.Message}");
				throw;
			}
		}

		public class ZeroShotClassificationResult
		{
			[JsonPropertyName("labels")]
			public List<string> Labels { get; set; }
			
			[JsonPropertyName("scores")]
			public List<float> Scores { get; set; }
			
			[JsonPropertyName("sequence")]
			public string Sequence { get; set; }
		}
	}

}
