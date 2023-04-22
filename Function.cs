using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaTest;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        string methodType = request.RequestContext.Http.Method.ToUpper();
        if (methodType == "POST")
        {
            return await PutCustomer(request);
        }
        else if(methodType=="DELETE")
        {
            return await DeleteCustomer(request);
        }
        else
        {
            return await GetCustomer(request);
        }

    }

    private static async Task<APIGatewayHttpApiV2ProxyResponse> GetCustomer(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("CustomerId", out var customerIdString);

        if (Guid.TryParse(customerIdString, out var customerId))
        {
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var customer = await dynamoDBContext.LoadAsync<Customer>(customerId);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = JsonSerializer.Serialize(customer),
                StatusCode = 200
            };
        }

        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = "Invalid Customer ID",
            StatusCode = 404
        };
    }


    private static async Task<APIGatewayHttpApiV2ProxyResponse> PutCustomer(APIGatewayHttpApiV2ProxyRequest request)
    {
        string strCustomer = request.Body;

        if (!string.IsNullOrEmpty(strCustomer))
        {
            Customer objCustomer = JsonSerializer.Deserialize<Customer>(strCustomer);
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            await dynamoDBContext.SaveAsync<Customer>(objCustomer);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Customer Inserted Successfully",
                StatusCode = 200
            };
        }



        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = "Invalid Customer Details",
            StatusCode = 404
        };
    }


    private static async Task<APIGatewayHttpApiV2ProxyResponse> DeleteCustomer(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("CustomerId", out var customerIdString);

        if (Guid.TryParse(customerIdString, out var customerId))
        {
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            await dynamoDBContext.DeleteAsync<Customer>(customerId);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Customer Deleted Successfully",
                StatusCode = 200
            };
        }

        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = "Invalid Customer ID",
            StatusCode = 404
        };
    }

}

public class Customer
{
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
}
