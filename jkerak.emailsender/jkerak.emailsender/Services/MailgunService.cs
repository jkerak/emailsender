using jkerak.emailsender.Utilities;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.MailgunTemplateManager;
using jkerak.emailsender.Model;
using Microsoft.IdentityModel.Tokens;

namespace jkerak.emailsender.Services
{
    public class MailgunService : IMailgunService
    {
        private readonly HttpClient _httpClient;
        private readonly string _fromEmail;
        private readonly bool _enableMailgun;
        private readonly bool _mailgunTestMode;
        private readonly KeyVaultClient _keyVaultClient;


        public MailgunService(HttpClient httpClient, KeyVaultClient keyVaultClient)
        {
            _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
            _enableMailgun = Convert.ToBoolean(Environment.GetEnvironmentVariable("ENABLE_MAILGUN"));
            _mailgunTestMode = Convert.ToBoolean(Environment.GetEnvironmentVariable("MAILGUN_TEST_MODE"));

            _httpClient = httpClient;
            _keyVaultClient = keyVaultClient;
        }


        public async Task SendEmails(List<CommunicationSendRequest> sendRequests)
        {
            if (_enableMailgun)
            {
                var tasks = sendRequests.Select(
                    r => SendEmail(r)).ToList();

                await Task.WhenAll(tasks);
            }
        }

        private async Task SendEmail(CommunicationSendRequest request)
        {
            var emailSendForm = await GetEmailSendForm(request);
            var response = await _httpClient.PostAsync("messages", emailSendForm);

            request.MailgunResponse = await response.Content.ReadAsStringAsync();
        }



        public async Task RefreshTemplates(string templateString, string fileName)
        {
            var response = await _httpClient.GetAsync("templates?limit=100");
            var content = await response.Content.ReadAsStringAsync();
            var storedTemplates = JsonConvert.DeserializeObject<MailGunTemplateResponse>(content).items;

            if (storedTemplates.Any(t => String.Equals(t.name, fileName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await UpdateTemplateVersion(templateString, fileName);
            }
            else
            {
                await UploadNewTemplate(templateString, fileName);
            }
        }


        private async Task UploadNewTemplate(string templateString, string fileName)
        {
            await _httpClient.PostAsync("templates", PostNewTemplateForm(templateString, fileName));
        }

        private async Task UpdateTemplateVersion(string templateString, string fileName)
        {
            // This is a test
            await _httpClient.PutAsync($"templates/{fileName}/versions/{fileName}",
                UpdateTemplateVersionForm(templateString));
        }

        private HttpContent PostNewTemplateForm(string templateString, string fileName)
        {
            var formDictionary = new Dictionary<string, string>
            {
                {"name", fileName},
                {"description", fileName},
                {"template", templateString},
                {"tag", fileName}
            };
            return new FormUrlEncodedContent(formDictionary);
        }

        private HttpContent UpdateTemplateVersionForm(string templateString)
        {
            var formDictionary = new Dictionary<string, string>
            {
                {"template", templateString},
                {"active", "yes"}
            };
            return new FormUrlEncodedContent(formDictionary);
        }

        private async Task<HttpContent> GetEmailSendForm(CommunicationSendRequest sendRequest)
        {
            if (sendRequest.UseCase.CanUnsubscribe)
            {
                var unsubLink = await GetUnsubscribeLink(sendRequest);
                sendRequest.Parameters.Add(
                    new KeyValuePair<string, string>("UnsubscribeLink", unsubLink));
            }

            var formDictionary = new Dictionary<string, string>
            {
                {"from", sendRequest.FromEmail.Length == 0 ? _fromEmail : sendRequest.FromEmail},
                {"to", sendRequest.UserContactInfo.EmailAddress},
                {"subject", sendRequest.UseCase.Parameters.First(p => p.Key == "EmailSubject").Value},
                {"template", sendRequest.UseCase.Parameters.First(p => p.Key == "EmailTemplate").Value},
                {
                    "h:sender", sendRequest.FromEmail.Length == 0 ? _fromEmail : sendRequest.FromEmail
                }, //added to ensure sender address appears correctly in Outlook
                {
                    "h:X-Mailgun-Variables",
                    JsonConvert.SerializeObject(sendRequest.Parameters.ToDictionary(k => k.Key, v => v.Value))
                },
                {"o:testmode", (_mailgunTestMode).ToString()},
                {"o:tag", sendRequest.UseCase.Id}
            };

            if (_enableMailgun)
            {
                return new FormUrlEncodedContent(formDictionary);
            }

            return new StringContent(JsonConvert.SerializeObject(formDictionary), Encoding.UTF8, "application/json");
        }

        private async Task<string> GetUnsubscribeLink(CommunicationSendRequest request)
        {
            var prefix = Environment.GetEnvironmentVariable("EmbeddedSiteUrl");
            var suffix = "#!/contact-preferences";

            var obj = new
            {
                personId = request.PersonId,
                signature = await SignString(string.Concat(request.PersonId.ToString(), request.UseCase.Id)),
                useCase = request.UseCase.Id
            };
            var queryParam =
                Base64UrlEncoder.Encode(JsonConvert.SerializeObject(obj));
            return prefix + queryParam + suffix;
        }

        private async Task<string> SignString(string dataToSign)
        {

            string keyId = Environment.GetEnvironmentVariable("SIGNING_KEY_ID");
            var signature =
                await _keyVaultClient.SignAsync(keyId, JsonWebKeySignatureAlgorithm.RS256, dataToSign.HashString());

            
            var bsig = Base64UrlEncoder.Encode(signature.Result); ;

            return bsig;
        }
    }
}
