using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using jkerak.emailsender.Services;
using jkerak.emailsender.Utilities;
using jkerak.emailsender.Entities;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace jkerak.emailsender.UserPreferenceManager
{
    public class UserPreferenceManager
    {
        private readonly KeyVaultClient _keyVaultClient;
        private readonly ILoggingService _loggingService;


        public UserPreferenceManager(KeyVaultClient keyVaultClient, ILoggingService loggingService)
        {
            _keyVaultClient = keyVaultClient;
            _loggingService = loggingService;
        }


        [FunctionName(("UnsubscribeAll"))]
        public async Task<HttpResponseMessage> UnsubscribeAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client)
        {

            var body = await req.Content.ReadAsStringAsync();
            var unsubscribeRequest = JsonConvert.DeserializeObject<UnsubscribeRequest>(body);

            if (unsubscribeRequest is null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Not a Valid Request");
            }

            if (!string.IsNullOrEmpty(unsubscribeRequest.Signature))
            {
                var sig = Base64UrlEncoder.DecodeBytes(unsubscribeRequest.Signature);
                var values = string.Concat(unsubscribeRequest.PersonId.ToString(), unsubscribeRequest.UseCase);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var key = (await _keyVaultClient.GetKeyAsync(Environment.GetEnvironmentVariable("SIGNING_KEY_ID"))).Key;
                    var p = new RSAParameters() { Modulus = key.N, Exponent = key.E };
                    rsa.ImportParameters(p);
                    var isVerified = rsa.VerifyHash(values.HashString(), "Sha256", sig);
                    if (!isVerified)
                    {
                        return req.CreateResponse(HttpStatusCode.Forbidden);
                    }
                    
                }
            }

            var id = new EntityId(nameof(CommsTrackingEntity), unsubscribeRequest.PersonId.ToString());

            await client.SignalEntityAsync(id, nameof(ICommsTrackingEntity.UnsubscribeAll));

            //await _loggingService.LogUnsubscribe(unsubscribeRequest.PersonId.ToString(), unsubscribeRequest.UseCase);
            
            return req.CreateResponse(HttpStatusCode.OK);

        }


        [FunctionName(("ResubscribeAll"))]
        public async Task<HttpResponseMessage> ResubscribeAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client)
        {

            var body = await req.Content.ReadAsStringAsync();
            var unsubscribeRequest = JsonConvert.DeserializeObject<UnsubscribeRequest>(body);

            if (unsubscribeRequest?.PersonId is null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Not a Valid Request");
            }


            var id = new EntityId(nameof(CommsTrackingEntity), unsubscribeRequest.PersonId.ToString());

            await client.SignalEntityAsync(id, nameof(ICommsTrackingEntity.ResubscribeAll));

            return req.CreateResponse(HttpStatusCode.OK);

        }


    }
}