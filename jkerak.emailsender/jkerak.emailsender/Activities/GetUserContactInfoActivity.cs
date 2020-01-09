using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using jkerak.emailsender.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Activities
{
    public class GetUserContactInfoActivity
    {

        [FunctionName("Activities-GetUserContactInfo")]
        public static async Task<List<CommunicationSendRequest>> GetUserContactInfo([ActivityTrigger] IDurableActivityContext context)
        {
            var input = context.GetInput<List<CommunicationSendRequest>>();
            
            
     string sql =
                $"select PERSON_ID AS PersonId, PRIMARY_EMAIL AS EmailAddress, FIRST_NAME AS FirstName," +
                $" LAST_NAME AS LastName " +
                $"FROM PERSON WITH(NOLOCK) " +
                $"WHERE PERSON_ID IN ({string.Join(",",input.Select(i => i.PersonId.ToString()))})";
            
            
            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("HUB_CONNECTION_STRING")))
            {
                var userContactInfo =  (await connection.QueryAsync<UserContactInfo>(sql)).ToList();

                input.ForEach(i =>
                    {
                        i.UserContactInfo = userContactInfo.First(u => u.PersonId == i.PersonId);
                        i.Parameters.Add(new KeyValuePair<string, string>("FirstName", i.UserContactInfo.FirstName));
                        i.Parameters.Add(new KeyValuePair<string, string>("LastName", i.UserContactInfo.LastName));
                    }
                );
                
                return input;
            }
        }
    }
}