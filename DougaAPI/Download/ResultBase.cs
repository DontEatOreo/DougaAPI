using Microsoft.AspNetCore.Mvc;

namespace DougaAPI.Download;

public sealed record ResultBase(FileStreamResult StreamResult, params string[] Paths);