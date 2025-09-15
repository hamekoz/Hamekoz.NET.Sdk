var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver");

builder.AddProject<Projects.Hamekoz_Api_Example>("hamekoz-api-example");

var hamekozAuthServiceDb = sqlServer.AddDatabase("hamekoz-auth-service-api-db");

builder.AddProject<Projects.Hamekoz_Auth_Service_Api>("hamekoz-auth-service-api")
    .WithReference(hamekozAuthServiceDb);

builder.Build().Run();
