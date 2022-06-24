using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda;

public class RandomSalaryGenerator
{
    
    /// <summary>
    /// A simple function that provides a salary for a profile
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public object Run(ILambdaContext context)
    {
        var rand = new Random();
        var salary = rand.Next(25_000, 225_000);

        return new {
            salary = salary
        };
    }
}
