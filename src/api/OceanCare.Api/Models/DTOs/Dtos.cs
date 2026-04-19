namespace OceanCare.Api.Models.DTOs;

public record AttributeValueDto(int AttributeId, string AttributeName, string Value);

public record SpeciesDto(
    int Id,
    string ScientificName,
    string CommonName,
    string Description,
    string? ImageUrl,
    int CategoryId,
    string CategoryName,
    IEnumerable<AttributeValueDto> Attributes
);

public record SpeciesSearchResultDto(
    SpeciesDto Species,
    float SimilarityScore
);

public record CategoryDto(int Id, string Name, string Description);

public record MarineAttributeDto(int Id, string Name, string Description, string DataType);

public record AdminLoginRequest(string Username, string Password);

public record AdminLoginResponse(string Token, string Username);

public record CreateSpeciesRequest(
    string ScientificName,
    string CommonName,
    string Description,
    string? ImageUrl,
    int CategoryId,
    IEnumerable<AttributeValueRequest> Attributes
);

public record UpdateSpeciesRequest(
    string ScientificName,
    string CommonName,
    string Description,
    string? ImageUrl,
    int CategoryId,
    IEnumerable<AttributeValueRequest> Attributes
);

public record AttributeValueRequest(int AttributeId, string Value);

public record CreateAttributeRequest(string Name, string Description, string DataType = "string");

public record UpdateAttributeRequest(string Name, string Description, string DataType);

public record CreateCategoryRequest(string Name, string Description);

public record UpdateCategoryRequest(string Name, string Description);
