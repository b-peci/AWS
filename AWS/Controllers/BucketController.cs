using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWS.Controllers;
[ApiController]
public class BucketController : Controller
{
    private IConfiguration _conf;

    public BucketController(IConfiguration conf)
    {
        _conf = conf;
    }
    [HttpGet("GetBuckets")]
    public async Task<IActionResult> GetBuckets()
    {
        try
        {
            var s3Client = new AmazonS3Client(_conf["AccessKey"], _conf["SecretKey"], RegionEndpoint.EUCentral1);
            var listRequest = new ListBucketsRequest();
            var response = await s3Client.ListBucketsAsync(listRequest);
            List<string> buckets = new();
// Iterate through the list of buckets and print their names
            foreach (var bucket in response.Buckets)
            {
                buckets.Add(bucket.BucketName);
            }

            return Ok(buckets);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}


public class Something
{
    public IFormFile File { get; set; }
    public int Somethingd { get; set; }
}