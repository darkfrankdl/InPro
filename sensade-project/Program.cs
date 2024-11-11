using sensade_project;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Repository repo = new Repository();
repo.InitializeDatabase(); // sets up the database tables from the repository class if not already
// demo data
//repo.CreateParkingArea(1, "someStreet", "somecity", 1234, 51.1234M, 69.6969M);
//repo.CreateParkingSpace("free", 1, 1);
//repo.CreateParkingSpace("occupied", 2, 1);

app.Run();