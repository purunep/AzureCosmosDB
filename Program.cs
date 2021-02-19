using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmoSdk
{
    class Program
    {
        static async Task Main(string[] args)
        {

            // await ViewDatabases();
            // await CreateDatabase();
            // await DeleteDatabase();
            // await ViewContainers();
            // await CreateContainer("Services", partitionKey: "/id");
            // await DeleteContainer("Services");
            // await CreateDocuments();
            // await CreateDocumentFromRawJson();
            //  await CreateDocumentFromPOCO();
            // await QueryForDocuments();
            // await QueryAllDocumentsWithStatefulPaging();//Stateful may be very slow if we need to get millions of data
            // await QueryAllDocumentsWithStatelessPaging();

            //when an application runs a query, cosmosdb normally desearlize result stream from database turns into a resource object
            // and then serialize back to json for the applicaion, and cost cpu charge. so can use stream as rawstring and process on appliation side

            //  await QueryAllDocumentsWithStatefulPagingStreamed();
            // await QueryAllDocumentsWithStatelessPagingStreamed();
            // await UpdateDocument("PCre Test4");
            await DeleteDocument("PCre Test4");
            Console.ReadLine();
        }

        private async static Task QueryForDocuments()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var endpoint = config["CosmosEndpoint"];
            var masterKey = config["CosmosMasterKey"];

            using (var client = new CosmosClient(endpoint, masterKey))
            {
                var container = client.GetContainer("LearnDrive", "Instructor");
                var sql = "SELECT * from c";
                var iterator = container.GetItemQueryIterator<dynamic>(sql);

                var page = await iterator.ReadNextAsync();
                foreach (var doc in page)
                {
                    Console.WriteLine($"DocId {doc.id}");
                }
                Console.ReadLine();

            }
        }

        private async static Task ViewDatabases()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var endpoint = config["CosmosEndpoint"];
            var masterKey = config["CosmosMasterKey"];
            using (var client = new CosmosClient(endpoint, masterKey))
            {
                var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
                var databases = await iterator.ReadNextAsync();
                var count = 0;
                foreach (var database in databases)
                {
                    count++;
                    Console.WriteLine($"Database Id: {database.Id}; Modified: {database.LastModified}");
                }
                Console.WriteLine();
                Console.WriteLine($"Total databases: {count}");

            }

        }


        private async static Task CreateDatabase()
        {
            var result = await Shared.Client.CreateDatabaseAsync("DriveInfo");
            var database = result.Resource;
            Console.WriteLine($"Database Id: {database.Id}, Modified:  {database.LastModified}");
        }

        private async static Task DeleteDatabase()
        {
            await Shared.Client.GetDatabase("DriveInfo").DeleteAsync();
            Console.WriteLine($"Database deleted.");
        }


        private async static Task ViewContainers()
        {
            var database = Shared.Client.GetDatabase("LearnDrive");
            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            var containers = await iterator.ReadNextAsync();
            var count = 0;
            foreach (var container in containers)
            {
                count++;
                Console.WriteLine($"Cotainer #{count}");
                await ViewContainer(container);

            }
        }

        private async static Task ViewContainer(ContainerProperties containerProperties)
        {
            Console.WriteLine($"Container ID: {containerProperties.Id}");
            Console.WriteLine($"Last Modified: {containerProperties.LastModified}");
            Console.WriteLine($"PartitionKeyPath: {containerProperties.PartitionKeyPath}");
            var container = Shared.Client.GetContainer("LearnDrive", containerProperties.Id);
            var throughput = await container.ReadThroughputAsync();
            Console.WriteLine($"Throughput: {throughput}");
        }

        private async static Task CreateContainer(string containerId,
            int throughput = 400, string partitionKey = "/partitionKey")
        {
            var containerDef = new ContainerProperties
            {
                Id = containerId,
                PartitionKeyPath = partitionKey
            };
            var database = Shared.Client.GetDatabase("LearnDrive");
            var result = await database.CreateContainerAsync(containerDef, throughput);
            var container = result.Resource;
            Console.WriteLine("Created new container");
            await ViewContainer(container);
        }

        private async static Task DeleteContainer(string containerId)
        {
            var container = Shared.Client.GetContainer("LearnDrive", containerId);
            await container.DeleteContainerAsync();
            Console.WriteLine("Container Deleted.");
        }

        private async static Task CreateDocuments()
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var gid = Guid.NewGuid();
            dynamic documentDynamic = new
            {
                id = gid,
                name = "San Dan",
                address = new
                {
                    postal = "M1L3E9",
                    location = new
                    {
                        city = "Toronto",
                        provience = "ON"
                    },
                    country = "Canada"
                }
            };
            await container.CreateItemAsync(documentDynamic, new PartitionKey($"{ gid }"));
            Console.WriteLine($"Document created: {documentDynamic.id}");
        }

        private async static Task CreateDocumentFromRawJson() {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var gid = Guid.NewGuid();
            var documentJson = $@"
            {{
                ""id"" : ""{gid}"",
                ""name"" : ""Sajsn kum"",
                ""address"" : {{
                
                    ""postal"" : ""M1L3E9"",
                    ""location"" : {{
                    
                        ""city"" : ""Scarborough"",
                        ""provience"" : ""ON""
                    }},
                    ""country"" : ""Canada""
                }}
            }}";

            var document2Object = JsonConvert.DeserializeObject<JObject>(documentJson);
            await container.CreateItemAsync(document2Object, new PartitionKey($"{ gid }"));
            Console.WriteLine($"Document created: {document2Object["id"].Value<string>()}");
        }

        private async static Task CreateDocumentFromPOCO()
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            //creating 1000 documents
            for(int i = 0;i < 1000; i++)
            {
                var gid = Guid.NewGuid().ToString();
                var documentPOCO = new Instructor
                {
                    Id = gid,
                    Name = $"PCre Test{i}",
                    Address = new Address
                    {
                        Postal = "M1L3E9",
                        Location = new Location
                        {
                            City = "Toronto",
                            Provience = "ON"
                        },
                        Country = "Canada"
                    }
                };
                await container.CreateItemAsync(documentPOCO, new PartitionKey($"{ gid }"));
            }
       
            Console.WriteLine($"Document created");
        }

        private async static Task QueryAllDocumentsWithStatefulPaging()
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = "SELECT * from c";
            var itemCount = 0;
            var pageCount = 0;
            var iterator = container.GetItemQueryIterator<Instructor>(sql, requestOptions: new QueryRequestOptions { MaxItemCount = 100 });
            while(iterator.HasMoreResults)
            {
                pageCount++;
                var documents = await iterator.ReadNextAsync();
                foreach(var instructor in documents)
                {
                    Console.WriteLine($"Item {++itemCount} of page: {pageCount}, document: {instructor.Name}");
                }

            }
        }

        private async static Task QueryAllDocumentsWithStatelessPaging()
        {
            var continuationToken = default(string);
            do
            {
                continuationToken = await QueryFetchNextPage(continuationToken);
            } while (continuationToken != null);

            Console.WriteLine("Retrieved all documents");
        }

        private async static Task<string> QueryFetchNextPage(string continuationToken)
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = "SELECT * from c";
            var itemCount = 0;
            var iterator = container.GetItemQueryIterator<Instructor>(sql, continuationToken);
            var documents = await iterator.ReadNextAsync();
            foreach(var instructor in documents)
            {
                Console.WriteLine($"Item: {++itemCount} Name: {instructor.Name}");
            }
            continuationToken = documents.ContinuationToken;
            return continuationToken;
        }

        private async static Task QueryAllDocumentsWithStatefulPagingStreamed()
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = "SELECT * from c";
            var itemCount = 0;
            var pageCount = 0;
            var streamIterator = container.GetItemQueryStreamIterator(sql, requestOptions: new QueryRequestOptions { MaxItemCount = 100 });
           
            while (streamIterator.HasMoreResults)
            {
                pageCount++;
                var results = await streamIterator.ReadNextAsync();
                var stream = results.Content;
                using(var sr = new StreamReader(stream))
                {
                    var json = await sr.ReadToEndAsync();
                    var jobj = JsonConvert.DeserializeObject<JObject>(json);
                    var documents = (JArray)jobj["Documents"];
                    foreach (var item in documents)
                    {
                        var instructor = JsonConvert.DeserializeObject<Instructor>(item.ToString());
                        Console.WriteLine($"Item {++itemCount} of page: {pageCount}, document: {instructor.Name}");
                    }
                }

            }
        }

        private async static Task QueryAllDocumentsWithStatelessPagingStreamed()
        {
            var continuationToken = default(string);
            do
            {
                continuationToken = await QueryFetchNextPageStreamed(continuationToken);
            } while (continuationToken != null);

            Console.WriteLine("Retrieved all documents");
        }

        private async static Task<string> QueryFetchNextPageStreamed(string continuationToken)
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = "SELECT * from c";
            var itemCount = 0;
            var streamIterator = container.GetItemQueryStreamIterator(sql, continuationToken, new QueryRequestOptions { MaxItemCount = 100 });
            var response = await streamIterator.ReadNextAsync();

            var stream = response.Content;
            using(var sr = new StreamReader(stream))
            {
                var json = await sr.ReadToEndAsync();
                var jobj = JsonConvert.DeserializeObject<JObject>(json);
                var jarr = (JArray)jobj["Documents"];
                foreach (var item in jarr)
                {
                    var instructor = JsonConvert.DeserializeObject<Instructor>(item.ToString());
                    Console.WriteLine($"Item: {++itemCount} Name: {instructor.Name}");
                }

            }
          
            continuationToken = response.Headers.ContinuationToken;
            return continuationToken;
        }

        private async static Task UpdateDocument(string name)
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = $"SELECT * from c WHERE c.name = '{name}'";
           
            var documents = (await (container.GetItemQueryIterator<dynamic>(sql)).ReadNextAsync()).ToList();
            foreach(var document in documents)
            {
                document.address.postal = "M2M2M2";
                var result = await container.ReplaceItemAsync<dynamic>(document, (string)document.id);
                var updatedDocument = result.Resource;
                Console.WriteLine($"Updated postal: {updatedDocument.address.postal}");
            }
          
        }

        private async static Task DeleteDocument(string name)
        {
            var container = Shared.Client.GetContainer("LearnDrive", "Instructor");
            var sql = $"SELECT * from c WHERE c.name = '{name}'";
            var iterator = container.GetItemQueryIterator<dynamic>(sql);

            var documents = (await iterator.ReadNextAsync()).ToList();
            foreach (var document in documents)
            {
                string id = document.id;
                string partitionKey = document.id;
                await container.DeleteItemAsync<dynamic>(id, new PartitionKey(partitionKey));
                Console.WriteLine($"Document deleted.");
            }

        }
    }

}
