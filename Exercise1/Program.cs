using Newtonsoft.Json;
using Relewise.Client.DataTypes;

public interface IJob
{
    Task<string> Execute(
        JobArguments arguments,
        Func<string, Task> info,
        Func<string, Task> warn,
        CancellationToken token);
}

public class JobArguments
{
    public JobArguments(
    Guid datasetId,
    string apiKey,
    IReadOnlyDictionary<string, string> jobConfiguration)
    {
        DatasetId = datasetId;
        ApiKey = apiKey;
        JobConfiguration = jobConfiguration;
    }
    public Guid DatasetId { get; }
    public string ApiKey { get; }
    public IReadOnlyDictionary<string, string> JobConfiguration { get; }
}

public class ProductJson // Represents a product data model with required properties for JSON deserialization
{
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string ListPrice { get; set; }
    public required string SalesPrice { get; set; }
}

public class ProductMappingJob : IJob
{
    public async Task<string> Execute(
        JobArguments arguments,
        Func<string, Task> info,
        Func<string, Task> warn,
        CancellationToken token)
    {
        // Retrieves the JSON data from the relewise custom URL using an HttpClient instance.
        string url = "https://cdn.relewise.com/academy/productdata/customjsonfeed";
        using HttpClient client = new HttpClient();

        await info("Starting product mapping...");
        // Downloads the JSON data as a string and stores it in the jsonData variable.
        string jsonData = await client.GetStringAsync(url);

        // Check if the jsonData string is null, empty, or contains only whitespace characters
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            // If so, return an error message indicating that no data was received
            return "No data received.";
        }

        // Deserializes JSON data into an array of ProductJson objects.
        ProductJson[]? products = JsonConvert.DeserializeObject<ProductJson[]>(jsonData);

        // Dereference of a possibly null reference.
        if (products == null || products.Length == 0)
        {
            return "No products found in the JSON data.";
        }

        // First try, it was obsolete, Replaced with a more efficient and concise LINQ-based approach.
        // var mappedProducts = new List<Product>();
        // foreach (var product in products)
        // {
        //     var salesPriceValue = decimal.Parse(product.SalesPrice.Replace("$", "").Trim());
        //     var listPriceValue = decimal.Parse(product.ListPrice.Replace("$", "").Trim());
        //     var relewiseProduct = new Product
        //     {
        //         Id = product.ProductId,
        //         DisplayName = new Multilingual(new Multilingual.Value(new Language("en"), product.ProductName)),
        //         ListPrice = new MultiCurrency("USD", listPriceValue),
        //         SalesPrice = new MultiCurrency("USD", salesPriceValue),
        //     };
        //     mappedProducts.Add(relewiseProduct);
        // }

        // Maps JSON products to Product objects, converting prices and names.
        // Creates a new Product instance for each JSON product, populating its properties.
        var mappedProducts = products.Select(product => new Product(product.ProductId)
        { // ProductId as a Key or it would be [Obsolete]
            DisplayName = new Multilingual(new Multilingual.Value(new Language("en"), product.ProductName)),
            ListPrice = new MultiCurrency("USD", decimal.Parse(product.ListPrice.Replace("$", "").Trim())),
            SalesPrice = new MultiCurrency("USD", decimal.Parse(product.SalesPrice.Replace("$", "").Trim())),
        }).ToList();

        return $"Mapped {mappedProducts.Count} products successfully."; // 71 products 
    }
}


class Program {
    static async Task Main(string[] args)
    {
        // This code executes a ProductMappingJob with the provided arguments and logs messages to the console.
        var job = new ProductMappingJob();
        var arguments = new JobArguments(Guid.NewGuid(), "API_KEY", new Dictionary<string, string>());
        // The log and warn functions are used to handle logging and warning messages, respectively.
        Task log(string msg) => Task.Run(() => Console.WriteLine(msg));
        Task warn(string msg) => log($"Warning: {msg}");
        string result = await job.Execute(arguments, log, warn, CancellationToken.None);
        // The result of the job execution is then printed to the console.
        Console.WriteLine(result);
    }
}


