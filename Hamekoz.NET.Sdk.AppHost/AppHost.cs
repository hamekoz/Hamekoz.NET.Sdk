var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Hamekoz_Api_Example>("hamekoz-api-example");

builder.Build().Run();
