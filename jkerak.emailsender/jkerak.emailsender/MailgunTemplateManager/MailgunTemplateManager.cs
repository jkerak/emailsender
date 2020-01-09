using System.IO;
using System.Threading.Tasks;
using jkerak.emailsender.Services;
using Microsoft.Azure.WebJobs;

namespace jkerak.emailsender.MailgunTemplateManager
{
    public class MailgunTemplateManager
    {
        private readonly MailgunService _mailgunService;

        public MailgunTemplateManager(MailgunService mailgunService)
        {
            _mailgunService = mailgunService;
        }

        //[FunctionName("MailgunTemplateManager")]
        public async Task UpdateTemplates([BlobTrigger("templates/{name}", Connection = "AzureWebJobsStorage")]
            Stream templateStream, string name)
        {
            StreamReader reader = new StreamReader(templateStream);
            string text = reader.ReadToEnd();

            await _mailgunService.RefreshTemplates(text, name);
        }
    }
}