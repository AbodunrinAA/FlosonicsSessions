using System.Reflection.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FlosonicsSession.Helpers;

public class AddHeaderParameters : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();
       
        operation.Parameters.Add(new OpenApiParameter()
        {
            Name = "ETag",
            Schema = new OpenApiSchema()
            {
                Type = "string"
            },
            In = ParameterLocation.Header,
            Required = false,
            Description = "ETag"
        });    
    }
}