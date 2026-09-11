using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WEBAPI_DTOS.DTO;
using WebAPIDemoNew.AuthServices;
using WebAPIDemoNew.Data;
using WebAPIDemoNew.Migrations;
using WebAPIDemoNew.Models;
using WebAPIDemoNew.Services;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("ApiSettings")["Secret"]);

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>();

builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>();

// Add services to the container.
builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => { 
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters { 
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, 
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAutoMapper(options => {
    options.CreateMap<Villa, CreateVillaDTO>().ReverseMap();
    options.CreateMap<Villa, UpdateVillaDTO>().ReverseMap();
    options.CreateMap<Villa, VillaDTO>().ReverseMap();
    options.CreateMap<VillaDTO, UpdateVillaDTO>().ReverseMap();
    options.CreateMap<User, UserDto>().ReverseMap(); 
    options.CreateMap<ApplicationUser, UserDto>().ReverseMap(); 
    options.CreateMap<VillaAmenities, VillaAmenetiesCreateDTO>().ReverseMap(); 
    options.CreateMap<VillaAmenities, VillaAmenitiesUpdateDTO>().ReverseMap(); 
    options.CreateMap<VillaAmenities, VIllaAmenetiesDTO>()
    .ForMember(dest=>dest.VillaName, opt=>opt.MapFrom(src=>src.Villa!=null?src.Villa.Name:null));

	options.CreateMap<VIllaAmenetiesDTO, VillaAmenities>();
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDBContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DeafultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(o=>o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
