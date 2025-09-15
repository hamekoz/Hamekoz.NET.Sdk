using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hamekoz.Auth.Service.API.Data;

public class HamekozAuthDbContext(DbContextOptions<HamekozAuthDbContext> options) : IdentityDbContext(options)
{
}
