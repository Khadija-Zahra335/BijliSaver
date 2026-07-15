// Thin HTTP client for the Python OCR microservice.
// Configure the base URL in appsettings.json → "OcrService:BaseUrl".

using BijliSaver.Api.Dtos;

namespace BijliSaver.Api.Services;

public class OcrClient(HttpClient http, ILogger<OcrClient> logger)
{
    public async Task<OcrExtractionResponse> ExtractAsync(
        Stream file, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var filePart = new StreamContent(file);
        filePart.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(filePart, "file", fileName);

        var response = await http.PostAsync("/extract", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OcrExtractionResponse>(cancellationToken: ct)
                     ?? throw new InvalidOperationException("OCR service returned empty body");

        logger.LogInformation("OCR result: status={Status} confidence={Conf} issues={Issues}",
            result.Status, result.EffectiveConfidence, result.Issues.Count);

        return result;
    }

    public async Task<AdviceResponse?> AdviseAsync(object facts, CancellationToken ct = default)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/advise", facts, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AdviceResponse>(cancellationToken: ct);
        }
        catch (Exception e)
        {
            logger.LogWarning("Advice generation failed, falling back: {Msg}", e.Message);
            return null;   // advice is a bonus — never fail the upload because of it
        }
    }
}
