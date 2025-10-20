using Microsoft.Extensions.Logging;
using SkyReserve.Application.Interfaces;
using System.Text.Json;

namespace SkyReserve.Application.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ILogger<TemplateService> _logger;
        private readonly string _templatesPath;

        public TemplateService(ILogger<TemplateService> logger)
        {
            _logger = logger;
            _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        }

        public async Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> parameters)
        {
            try
            {
                var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Email template not found: {TemplatePath}", templatePath);
                    return GenerateFallbackEmailTemplate(templateName, parameters);
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);

                foreach (var parameter in parameters)
                {
                    templateContent = templateContent.Replace($"{{{{{parameter.Key}}}}}", parameter.Value);
                }

                return templateContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
                return GenerateFallbackEmailTemplate(templateName, parameters);
            }
        }

        public async Task<string> GetSmsTemplateAsync(string templateName, Dictionary<string, string> parameters)
        {
            try
            {
                var templatesFilePath = Path.Combine(_templatesPath, "SmsTemplates.json");

                if (!File.Exists(templatesFilePath))
                {
                    _logger.LogWarning("SMS templates file not found: {FilePath}", templatesFilePath);
                    return GenerateFallbackSmsTemplate(templateName, parameters);
                }

                var jsonContent = await File.ReadAllTextAsync(templatesFilePath);
                var templates = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (templates == null || !templates.TryGetValue(templateName, out var template))
                {
                    _logger.LogWarning("SMS template not found: {TemplateName}", templateName);
                    return GenerateFallbackSmsTemplate(templateName, parameters);
                }

                foreach (var parameter in parameters)
                {
                    template = template.Replace($"{{{{{parameter.Key}}}}}", parameter.Value);
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SMS template: {TemplateName}", templateName);
                return GenerateFallbackSmsTemplate(templateName, parameters);
            }
        }

        private string GenerateFallbackEmailTemplate(string templateName, Dictionary<string, string> parameters)
        {
            return $@"
                <h2>{templateName}</h2>
                <p>This is a fallback template.</p>
                <p>Template parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}: {p.Value}"))}</p>
            ";
        }

        private string GenerateFallbackSmsTemplate(string templateName, Dictionary<string, string> parameters)
        {
            return $"{templateName}: {string.Join(", ", parameters.Select(p => $"{p.Key}: {p.Value}"))}";
        }
    }
}