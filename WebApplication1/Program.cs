using VectorDataBases.Performance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

//app.UseHttpsRedirection();

app
    .MapGet
        (
              "/pgsql"
            , async () =>
            {
                await new TestContext().PostgreSQL_11w_ProcessAsync();
                return "ok";
            }
        );
    
app
    .MapGet
        (
              "/wikipedia/pgvector"
            , async () =>
            {
                await new TestContext().WikipediaPostgreSQL_25k_ProcessAsync();
                return "pgvector";
            }
        );

app
    .MapGet
        (
              "/wikipedia/redisearch-selfhost"
            , async () =>
            {
                await new TestContext().WikipediaSelfHostRediSearch_25k_ProcessAsync();
                return "redisearch-selfhost";
            }
        );

app
    .MapGet
        (
              "/wikipedia/redisearch-azure"
            , async () =>
            {
                await new TestContext().WikipediaAzureRediSearch_25k_ProcessAsync();
                return "redisearch-azure";
            }
        );


app.Run();

