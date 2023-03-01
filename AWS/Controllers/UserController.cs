using AWS.Auth;
using AWS.Users;
using Microsoft.AspNetCore.Mvc;

namespace AWS.Controllers;
[ApiController]
public class UserController : Controller
{
    public ApplicationDbContext _context;
    private TokenService _tokenService = new TokenService();

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpPost("AddUser")]
    public IActionResult AddUser(User user)
    {
        var efUser = new User { UserName = user.UserName, Password = user.Password };
        
        _context.Users.Add(efUser);
        _context.SaveChanges();
        return StatusCode(201);
    }
    [HttpPost("Login")]
    public IActionResult Login(User user)
    {
        var logedUser = _context.Users.FirstOrDefault(x => x.UserName == user.UserName && x.Password == user.Password);
        if (logedUser == null) return Unauthorized();
        string token = _tokenService.CreateToken(logedUser);
        return Ok(token);
    }
}