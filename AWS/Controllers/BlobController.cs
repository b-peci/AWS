using AWS.Blob;
using Microsoft.AspNetCore.Mvc;

namespace AWS.Controllers;
[ApiController]
public class BlobController : Controller
{
    public ApplicationDbContext _context;
    private readonly string _connectionString;
    private readonly string _container;
    private BlobStorage _storage;
    public BlobController(ApplicationDbContext context, IConfiguration _conf)
    {
        _context = context;
        _connectionString = _conf.GetValue<string>("StorageConnectionString");
        _container = _conf.GetValue<string>("ContainerName");
        _storage = new BlobStorage();
    }

    [HttpGet("GetList")]

    public async Task<IActionResult> GetList()
    {
        return  Ok(await _storage.GetAllDocuments(_connectionString, _container));
    }
    [HttpGet("ListFiles")]
    public async Task<List<string>> ListFiles()
    {
        return await _storage.GetAllDocuments(_connectionString, _container);
    }

    [Route("InsertFile")]
    [HttpPost]
    public async Task<bool> InsertFile(IFormFile asset)
    {
        if (asset != null)
        {
            Stream stream = asset.OpenReadStream();
            await _storage.UploadDocument(_connectionString, _container, asset.FileName, stream);
            return true;
        }

        return false;
    }

    [HttpGet("DownloadFile/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        var content = await _storage.GetDocument(_connectionString, _container, fileName);
        return File(content, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
    }

    [Route("DeleteFile/{fileName}")]
    [HttpGet]
    public async Task<bool> DeleteFile(string fileName)
    {
        return await _storage.DeleteDocument(_connectionString, _container, fileName);
    }
    
}