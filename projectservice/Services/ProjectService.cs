using MongoDB.Driver;
using projectservice.Dto;
using projectservice.Models;
using projectservice.Utils;

namespace projectservice.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IConfiguration _config;
        private readonly IBlobStorageUtils _blobStorage;
        private readonly ILogger _logger;
        private readonly IMongoCollection<Project> _projects;
        public ProjectService(IConfiguration config, IBlobStorageUtils blobStorage, IMongoClient mongoClient, ILogger<ProjectService> logger)
        {
            var mongoDB = mongoClient.GetDatabase(config.GetSection("MongoDbSettings:DatabaseName").Value);
            _projects = mongoDB.GetCollection<Project>(config.GetSection("MongoDbSettings:ProjectCollectionName").Value);
            _config = config;
            _blobStorage = blobStorage;
            _logger = logger;
        }

        public async Task<(bool, string)> CreateProject(ProjectDto dto)
        {
            try
            {
                List<Uri> projects = new();
                foreach (var (FileName, ImageAsByteArray) in dto.Images)
                {
                    // Upload images to blobstorage and fetch URI's
                    projects.Add(await _blobStorage.UploadFileAsync(ImageAsByteArray, FileName, dto.ProjectName));
                }
                _logger.LogInformation($"{projects.Count} new images have been added to {dto.ProjectName} project.");
                Project project = new()
                {
                    ProjectName = dto.ProjectName,
                    ProjectCreator = dto.ProjectCreator,
                    ProjectDescription = dto.ProjectDescription,
                    LabelClasses = dto.LabelClasses,
                    PictureUrls = projects.Select(uri => uri.ToString()).ToList()
                };
                await _projects.InsertOneAsync(project);
                return (true, "Project has been successfully created!");
            }catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
