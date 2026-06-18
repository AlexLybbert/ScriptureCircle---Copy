using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Infrastructure.Scriptures;

public sealed class OpenScriptureService(HttpClient httpClient, IOptions<OpenScriptureOptions> options) : IScriptureService
{
    public async Task<ScriptureChapterDto> GetChapterAsync(
        string volumeId,
        string bookId,
        int chapter,
        int? selectedStart,
        int? selectedEnd,
        CancellationToken cancellationToken)
    {
        httpClient.BaseAddress ??= new Uri(options.Value.BaseUrl);
        var response = await httpClient.GetFromJsonAsync<OpenScriptureChapterResponse>(
            $"volume/{Uri.EscapeDataString(volumeId)}/{Uri.EscapeDataString(bookId)}/{chapter}",
            cancellationToken);

        if (response?.Chapter is null)
        {
            throw new InvalidOperationException("Open Scripture API returned an empty chapter.");
        }

        var verses = response.Chapter.Verses
            .Select((verse, index) =>
            {
                var number = index + 1;
                var selected = selectedStart is not null &&
                    number >= selectedStart &&
                    number <= (selectedEnd ?? selectedStart.Value);
                return new VerseDto(number, verse.Text ?? string.Empty, selected);
            })
            .ToList();

        return new ScriptureChapterDto(
            response.Id ?? $"{bookId}{chapter}",
            response.Chapter.BookTitle ?? bookId,
            response.Chapter.Delineation ?? "Chapter",
            response.Chapter.Number,
            response.Chapter.Summary ?? string.Empty,
            response.PrevChapterId ?? string.Empty,
            response.NextChapterId ?? string.Empty,
            verses);
    }

    private sealed class OpenScriptureChapterResponse
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        public string? NextChapterId { get; set; }
        public string? PrevChapterId { get; set; }
        public OpenScriptureChapter? Chapter { get; set; }
    }

    private sealed class OpenScriptureChapter
    {
        public string? BookTitle { get; set; }
        public string? Delineation { get; set; }
        public int Number { get; set; }
        public string? Summary { get; set; }
        public List<OpenScriptureVerse> Verses { get; set; } = [];
    }

    private sealed class OpenScriptureVerse
    {
        public string? Text { get; set; }
    }
}
