using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataModel;
using IngestionService.Common.Facades.DbFacade;
using IngestionService.Common.Facades.DbFacade.Exceptions;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace CosmosDbDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            CosmosDbFacade dbFacade;
            int itemCount;
            int initValue;
            try
            {
               var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
               

                string endpointUri =
                    $"https://{configuration["COSMOSDB_ACCOUNT_NAME"]}.documents.azure.com:443/";
                string primaryKey = configuration["COSMOSDB_KEY"];
                string databaseName = configuration["COSMOSDB_DB_NAME"];
                string collectionName = configuration["COSMOSDB_COLLECTION_NAME"];

                var documentClient = new DocumentClient(new Uri(endpointUri), primaryKey);

                dbFacade = new CosmosDbFacade(documentClient, databaseName, collectionName);

                itemCount = int.Parse(configuration["ITEM_COUNT"]);
                initValue = int.Parse(configuration["START_NUMBER"]);
            }
            catch (CosmosDbException ex)
            {
                Console.WriteLine($"Error occured during CosmosDb facade creation. {ex.ToString()} ");
                Console.ReadLine();
                return;
            }

            try
            {
                var sampleBaseName = "sampleName";
                var repoOwner = "yuryklyshevich";
                var repoId = "Istest4";
                List<Task> taskCollection = new List<Task>();
                
                for (int i = initValue; i < initValue + itemCount; i++)
                {
                    var sampleName = $"{sampleBaseName}{i}";
                    var sampleId = $"{repoOwner}={repoId}={sampleName}";
                    var item = new Sample(sampleId, repoOwner, repoId, $"manifestName{i}", sampleName,
                        new Author[] {new Author("test2Id", "Ivan Ivanov"), new Author("test1Id", "Yury Klyshevich"),},
                        null,
                        null, $"sampleDescription{i}", new[] {"java"}, $"urlFragment{i}", new[] {"azure"},
                        $"test{i}.sln", new ExtendedZipContent [1], new string[] {"yura.klyshevich1@gmail.com"},
                        new CiConfig[] {new CiConfig()}, null, changed: DateTimeOffset.Now,
                        commitDateTime: DateTimeOffset.Now.AddHours(-3)
                    );
                    var task = dbFacade.WriteAsync(item);
                   taskCollection.Add(task);
                    Console.WriteLine($"Item {i} queued for add.");
                }

                Console.WriteLine($"Waiting to finish processing.");
                Task.WaitAll(taskCollection.ToArray());
                Console.WriteLine($"Processing is finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured during populating data to CosmosDb. {ex.ToString()} ");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Enter a key to finish.....");
            Console.ReadLine();
        }
    }
}
