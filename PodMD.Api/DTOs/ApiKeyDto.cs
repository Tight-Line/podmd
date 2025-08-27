using PodMD.Api.Models;

namespace PodMD.Api.DTOs;

public record ApiKeyDto(string PlainTextKey, ApiKey ApiKey);