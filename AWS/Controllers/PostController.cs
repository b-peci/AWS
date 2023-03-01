using System.Net;
using System.Security.Claims;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Blob;
using AWS.Users;
using AWS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32.SafeHandles;

namespace AWS.Controllers;
[ApiController]
[Authorize]
public class PostController : Controller
{
    public ApplicationDbContext _context;
    AmazonS3Client s3Client;
    private readonly string _connectionString;
    private readonly string _container;
    private BlobStorage _storage;

    public PostController(ApplicationDbContext context, IConfiguration _conf)
    {
        _context = context;
        s3Client = new AmazonS3Client(_conf["AccessKey"], _conf["SecretKey"], RegionEndpoint.EUCentral1);
        _connectionString = _conf.GetValue<string>("StorageConnectionString");
        _container = _conf.GetValue<string>("ContainerName");
        _storage = new BlobStorage();
    }
    
    
    [HttpGet("GetAllPosts")]
    public IActionResult GetAllPosts()
    {
        var posts = _context.Posts.ToList();
        return Json(posts);
    }

    [HttpGet("GetPostById")]
    public async Task<IActionResult> GetPostById(int id)
    {
        var post = _context.Posts.FirstOrDefault(x => x.Id == id);
        var postImage = await GetPostImage(id);
        return Ok(new
        {
            PostDetails = post,
            PostImage =  postImage
        });
    }
    [HttpPost("AddPost")]
    public IActionResult AddPost(PostVM post)
    {
        var newPost = new Post()
        {
            Content = post.Content,
            UserId = int.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")!.Value)
        };
        _context.Posts.Add(newPost);
        _context.SaveChanges();
        return Ok();
    }

    [HttpPut("UpdatePost")]
    public IActionResult UpdatePost(PostVM post)
    {
        if (post.Id == 0) return BadRequest("please provide an id");
        var postToUpdate = _context.Posts.FirstOrDefault(x => x.Id == post.Id);
        if (postToUpdate == null) return BadRequest("Could not find post");
        postToUpdate.Content = post.Content;
        _context.Posts.Update(postToUpdate);
        _context.SaveChanges();
        return Ok("Data updated");
    }

    [HttpDelete("DeletePost")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        string imageKey = string.Empty;
        var post = _context.Posts.FirstOrDefault(x => x.Id == postId);
        imageKey = post.ImageKey;
        if (post == null) return BadRequest("Could not find post with the given id");
        _context.Posts.Remove(post);
        _context.SaveChanges();
        if (imageKey is not null or "") await DeleteImageOfPost(imageKey);
        return Ok("Post deleted");
    }

    [HttpPost("AddImageToPost")]
    public async Task<IActionResult> AddImage(int postId, IFormFile file)
    {
        try
        {
            Guid fileKey = Guid.NewGuid();
            var putRequest = new PutObjectRequest
            {
                BucketName = "bekim-bucket",
                Key = fileKey.ToString(),
                ContentType = file.ContentType // Specify the content type of the file
            };
            var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            putRequest.InputStream = memory;
            var response = await s3Client.PutObjectAsync(putRequest);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var post = _context.Posts.FirstOrDefault(x => x.Id == postId);
                post.ImageKey = fileKey.ToString();
                await _context.SaveChangesAsync();
                Stream stream = file.OpenReadStream();
                await _storage.UploadDocument(_connectionString, _container, post.ImageKey, stream);
            }
        
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    [HttpGet("GetPostImage")]
    public async Task<string> GetPostImage(int postId)
    {
        var post = _context.Posts.FirstOrDefault(x => x.Id == postId);
            var request = new GetObjectRequest
            {
                BucketName = "bekim-bucket",
                Key = post.ImageKey
            };

            using var getObjectResponse = await s3Client.GetObjectAsync(request);
            await using var responseStream = getObjectResponse.ResponseStream;
            var stream = new MemoryStream();
            await responseStream.CopyToAsync(stream);
            List<string> iamges = new List<string>();
            iamges.Add(Convert.ToBase64String(stream.ToArray()));
            iamges.AddRange(await _storage.GetAllDocuments(_connectionString, _container));
            return Convert.ToBase64String(stream.ToArray()) ;
    }

    [HttpDelete]
    private async Task DeleteImageOfPost(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = "bekim-bucket",
            Key = key
        };
        await s3Client.DeleteObjectAsync(deleteRequest);
        await _storage.DeleteDocument(_connectionString, _container, key);
    }
}