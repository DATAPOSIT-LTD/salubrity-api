// File: Salubrity.Infrastructure/Services/ScribanTemplateRenderer.cs
#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Scriban;
using HtmlAgilityPack;
using Salubrity.Application.Interfaces;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.Configurations;

namespace Salubrity.Infrastructure.Services;

public class ScribanTemplateRenderer : ITemplateRenderer
{
    private readonly EmailOptions _settings;
    private readonly ILogger<ScribanTemplateRenderer> _logger;
    private readonly ConcurrentDictionary<string, (Template Html, Template? Text)> _templateCache;
    private readonly Lazy<Template> _baseTemplate;

    public ScribanTemplateRenderer(
        IOptions<EmailOptions> settings,
        ILogger<ScribanTemplateRenderer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _templateCache = new ConcurrentDictionary<string, (Template, Template?)>();
        _baseTemplate = new Lazy<Template>(LoadBaseTemplate);
    }

    public async Task<RenderedTemplate> RenderAsync(string templateKey, object model)
    {
        try
        {
            var templates = await GetTemplatesAsync(templateKey);

            // Render content template first
            var contentHtml = await templates.Html.RenderAsync(model);

            // Wrap in base template with header/footer
            var fullHtml = await RenderWithBaseTemplateAsync(contentHtml, templateKey, model);

            var text = templates.Text != null
                ? await templates.Text.RenderAsync(model)
                : ConvertHtmlToText(fullHtml);

            return new RenderedTemplate
            {
                Html = fullHtml,
                Text = text,
                TemplateKey = templateKey
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render email template {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<bool> TemplateExistsAsync(string templateKey)
    {
        var htmlPath = GetTemplatePath(templateKey, "html");
        return await Task.FromResult(File.Exists(htmlPath));
    }

    private async Task<string> RenderWithBaseTemplateAsync(string contentHtml, string templateKey, object model)
    {
        var wrappedModel = new
        {
            content = contentHtml,
            template_key = templateKey,
            year = DateTime.UtcNow.Year,
            company = new
            {
                name = "Salubrity Centre",
                website = "https://salubritycentre.com",
                logo_url = "https://salubritycentre.com/wp-content/uploads/2017/07/X-Large-Logo-Salubrity-2.png",
                address = "Nairobi, Kenya",
                phone = "+254-XXX-XXXX",
                email = "info@salubritycentre.com"
            },
            original_model = model
        };

        return await _baseTemplate.Value.RenderAsync(wrappedModel);
    }

    private async Task<(Template Html, Template? Text)> GetTemplatesAsync(string templateKey)
    {
        if (_templateCache.TryGetValue(templateKey, out var cached))
            return cached;

        var htmlPath = GetTemplatePath(templateKey, "html");
        var textPath = GetTemplatePath(templateKey, "txt");

        if (!File.Exists(htmlPath))
        {
            _logger.LogError("Email template not found: {TemplatePath}", htmlPath);
            throw new FileNotFoundException($"Email template not found: {htmlPath}");
        }

        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        var htmlTemplate = Template.Parse(htmlContent);

        if (htmlTemplate.HasErrors)
        {
            var errors = string.Join(", ", htmlTemplate.Messages);
            _logger.LogError("Template parsing errors for {TemplateKey}: {Errors}", templateKey, errors);
            throw new InvalidOperationException($"Template parsing errors: {errors}");
        }

        Template? textTemplate = null;
        if (File.Exists(textPath))
        {
            var textContent = await File.ReadAllTextAsync(textPath);
            textTemplate = Template.Parse(textContent);

            if (textTemplate.HasErrors)
            {
                var errors = string.Join(", ", textTemplate.Messages);
                _logger.LogWarning("Text template parsing errors for {TemplateKey}: {Errors}", templateKey, errors);
            }
        }

        var templates = (htmlTemplate, textTemplate);
        _templateCache.TryAdd(templateKey, templates);
        return templates;
    }

    private string GetTemplatePath(string templateKey, string extension)
    {
        return Path.Combine(_settings.TemplatesPath, $"{templateKey}.{extension}");
    }

    private string ConvertHtmlToText(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script and style elements
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());

            // Convert common HTML elements to text equivalents
            foreach (var node in doc.DocumentNode.Descendants().ToList())
            {
                switch (node.Name.ToLower())
                {
                    case "br":
                        node.ParentNode?.ReplaceChild(HtmlNode.CreateNode("\n"), node);
                        break;
                    case "p":
                    case "div":
                        if (node.NextSibling != null)
                        {
                            node.ParentNode?.InsertAfter(HtmlNode.CreateNode("\n\n"), node);
                        }
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        node.ParentNode?.InsertBefore(HtmlNode.CreateNode("\n"), node);
                        node.ParentNode?.InsertAfter(HtmlNode.CreateNode("\n\n"), node);
                        break;
                    case "li":
                        node.ParentNode?.InsertBefore(HtmlNode.CreateNode("• "), node);
                        node.ParentNode?.InsertAfter(HtmlNode.CreateNode("\n"), node);
                        break;
                    case "a":
                        var href = node.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            node.ParentNode?.ReplaceChild(
                                HtmlNode.CreateNode($"{node.InnerText} ({href})"),
                                node);
                        }
                        break;
                }
            }

            var text = doc.DocumentNode.InnerText;

            // Clean up whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s*\n", "\n\n");

            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert HTML to text using HtmlAgilityPack, using fallback");
            return FallbackHtmlStrip(html);
        }
    }

    private static string FallbackHtmlStrip(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty)
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&#39;", "'")
            .Trim();
    }

    private Template LoadBaseTemplate()
    {
        var baseTemplatePath = Path.Combine(_settings.TemplatesPath, "_base.html");

        string templateContent;
        if (File.Exists(baseTemplatePath))
        {
            templateContent = File.ReadAllText(baseTemplatePath);
        }
        else
        {
            _logger.LogWarning("Base template not found at {Path}, using default", baseTemplatePath);
            templateContent = GetDefaultBaseTemplate();
        }

        var template = Template.Parse(templateContent);
        if (template.HasErrors)
        {
            var errors = string.Join(", ", template.Messages);
            _logger.LogError("Base template parsing errors: {Errors}", errors);
            throw new InvalidOperationException($"Base template parsing errors: {errors}");
        }

        return template;
    }

    private static string GetDefaultBaseTemplate()
    {
        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{{ company.name }}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, Helvetica, sans-serif;"">
    {{ content }}
</body>
</html>";
    }
}