// File: Salubrity.Application/Interfaces/Notifications/ITemplateRenderer.cs
#nullable enable
#nullable enable
using System.Threading.Tasks;
using Salubrity.Application.DTOs.Email;

namespace Salubrity.Application.Interfaces;

public interface ITemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(string templateKey, object model);
    Task<bool> TemplateExistsAsync(string templateKey);

}