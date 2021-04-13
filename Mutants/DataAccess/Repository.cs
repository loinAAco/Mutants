using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Mutants.Cache;
using Mutants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.DataAccess
{
    public class Repository : IRepository
    {
        private readonly ICache<Processed> cache;
        private readonly ILogger<Repository> logger;
        private readonly IAmazonDynamoDB client;

        public Repository(ICache<Processed> cache, ILogger<Repository> logger, IAmazonDynamoDB client)
        {
            this.cache = cache;
            this.logger = logger;
            this.client = client;
        }

        public virtual async Task<Processed> DnaWasProcessed(string[] dna)
        {
            var dnaKey = String.Join(String.Empty, dna);
            var processed = cache.Get(dnaKey);
            if (processed == null)
            {
                // Not in cache
                var request = new GetItemRequest {
                    TableName = "historic",
                    ProjectionExpression = "isMutant",
                    ConsistentRead = true,
                    Key = new Dictionary<string, AttributeValue>()
                     {
                         {
                             "dna", new AttributeValue {  S = dnaKey }
                         }
                     }
                };

                var historicData = await client.GetItemAsync(request, default);

                if (historicData != null && historicData.IsItemSet)
                {
                    processed = new Processed(true, historicData.Item["isMutant"].BOOL);
                }
                else
                    processed = new Processed(false, false);
            }

            return processed;
        }

        public virtual async Task<Stats> GetStats()
        {
            Stats stats = null;
            var request = new GetItemRequest
            {
                TableName = "stats",
                Key = new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = "1" } } },
                ProjectionExpression = "total_humans, total_mutants",
                ConsistentRead = true
            };

            var response = await client.GetItemAsync(request, default);
            if (response != null && response.IsItemSet)
            {
                int total_humans = 0;
                int total_mutants = 0;

                if (response.Item.ContainsKey("total_humans"))
                    total_humans = int.Parse(response.Item["total_humans"].N);

                if (response.Item.ContainsKey("total_mutants"))
                    total_mutants = int.Parse(response.Item["total_mutants"].N);

                stats = new Stats(1, total_humans, total_mutants, (total_humans != 0 ? (float)total_mutants / total_humans : 0));
            }

            if (stats == null)
                stats = new Stats(1, 0, 0, 0);

            return stats;
        }

        public virtual async Task<bool> SaveDnaValidation(string[] dna, bool isMutant)
        {
            try
            {
                String dnaKey = string.Join(String.Empty, dna);

                var request = new PutItemRequest
                {
                    TableName = "historic",
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { "dna", new AttributeValue {
                              S = dnaKey
                          }},
                        { "isMutant", new AttributeValue {
                              BOOL = isMutant
                          }}
                    }
                };
                await client.PutItemAsync(request);

                var updateStats = new UpdateItemRequest
                {
                    TableName = "stats",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "id", new AttributeValue { N = "1" } }
                    },
                    AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
                    {
                        {
                            isMutant ? "total_mutants" : "total_humans",
                            new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = "1" } }
                        },
                    },
                };

                await client.UpdateItemAsync(updateStats);

                cache.Set(dnaKey, new Processed(true, isMutant));

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
