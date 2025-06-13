
using Microsoft.OpenApi.Models;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Ensure you also add this in the middleware pipeline
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "White Front API",
        Version = "v1",
        Description = "An ASP.NET Core Web API for managing users"
    });



    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseCors();
// Configure the HTTP request pipeline.

// Add Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "White Front API v1"));

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Custom route prefix convention
public class ApiPrefixConvention : IControllerModelConvention
{
    private readonly string _prefix;

    public ApiPrefixConvention(string prefix)
    {
        _prefix = prefix;
    }

    public void Apply(ControllerModel controller)
    {
        if (controller == null)
        {
            return;
        }

        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel == null)
            {
                selector.AttributeRouteModel = new AttributeRouteModel()
                {
                    Template = _prefix + "/[controller]"
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(selector.AttributeRouteModel.Template) &&
                    !selector.AttributeRouteModel.Template.StartsWith(_prefix))
                {
                    selector.AttributeRouteModel.Template =
                        _prefix + "/" + selector.AttributeRouteModel.Template;
                }
            }
        }
    }
}

// Parameter transformer for route tokens
public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string TransformOutbound(object value)
    {
        if (value == null) return null;
        return System.Text.RegularExpressions.Regex.Replace(
            value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
