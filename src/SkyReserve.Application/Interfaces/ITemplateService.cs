namespace SkyReserve.Application.Interfaces
{
    public interface ITemplateService
    {
        Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> parameters);
        Task<string> GetSmsTemplateAsync(string templateName, Dictionary<string, string> parameters);
    }
}
