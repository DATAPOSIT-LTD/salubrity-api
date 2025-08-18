// File: Salubrity.Application/DTOs/Email/RenderedTemplate.cs
#nullable enable


namespace Salubrity.Application.DTOs.Email;

public class RenderedTemplate
{
    public required string Html { get; init; }
    public required string Text { get; init; }
    public required string TemplateKey { get; init; }

    public void Deconstruct(out string html, out string text)
    {
        html = Html;
        text = Text;
    }
}
