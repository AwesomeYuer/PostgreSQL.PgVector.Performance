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
              "/pgsql-11w"
            , async () =>
            {
                await new TestContext().PostgreSQL_11w_ProcessAsync();
                return "pgsql-11w";
            }
        );
    
app
    .MapGet
        (
              "/wikipedia/pgvector-25k"
            , async () =>
            {
                await new TestContext().WikipediaPostgreSQL_25k_ProcessAsync();
                return "pgvector-25k";
            }
        );

app
    .MapGet
        (
              "/wikipedia/redisearch-selfhost-25k"
            , async () =>
            {
                await new TestContext().WikipediaSelfHostRediSearch_25k_ProcessAsync();
                return "redisearch-selfhost-25k";
            }
        );

app
    .MapGet
        (
              "/wikipedia/redisearch-azure-25k"
            , async () =>
            {
                await new TestContext().WikipediaAzureRediSearch_25k_ProcessAsync();
                return "redisearch-azure-25k";
            }
        );


app.Run();

