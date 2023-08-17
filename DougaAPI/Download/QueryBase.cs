using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;

namespace DougaAPI.Download;

public class QueryBase : IRequest<ResultBase>
{
    [Required] 
    [JsonPropertyName("uri")] 
    public required Uri Uri { get; init; }
}
