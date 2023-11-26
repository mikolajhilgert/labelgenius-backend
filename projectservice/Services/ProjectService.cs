using Google.Api.Gax.ResourceNames;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using projectservice.Dto;
using projectservice.Models;
using projectservice.Utils;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

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

        public async Task<(bool, string)> CreateProject(CreateProjectDTO dto)
        {
            try
            {
                ObjectId id = ObjectId.GenerateNewId();
                Dictionary<string, string> projectImages = await _blobStorage.UploadFilesAsync(dto.Images, id.ToString());
                _logger.LogInformation($"{projectImages.Count} new images have been added to '{id}' project.");
                Project project = new()
                {
                    Id = id,
                    Name = dto.ProjectName,
                    Creator = dto.ProjectCreator,
                    Description = dto.ProjectDescription,
                    LabelClasses = dto.LabelClasses,
                    ImageUrls = projectImages
                };
                await _projects.InsertOneAsync(project);

                // Return project id
                return (true, id.ToString());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message)> DeleteProject(string projectId, string userEmail)
        {
            try
            {
                var project = await _projects.Find(x => x.Id == ObjectId.Parse(projectId)).FirstAsync();
                // Check if project exists and if the user is the creator
                if (project != null && project.Creator == userEmail)
                {
                    await _blobStorage.DeleteContainer(projectId);
                    // Delete project
                    await _projects.DeleteOneAsync(a => a.Id == ObjectId.Parse(projectId)); ;
                    return (true, "Project has been successfully deleted");
                }
                else
                {
                    return (false, "Project not found or user is not the creator");
                }

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message)> DeleteUserFromAllProjects(string userEmail)
        {
            try
            {
                var projectsWithUser = await _projects.Find(x => x.LabellingUsers.Contains(userEmail) || x.Creator == userEmail).ToListAsync();

                foreach (var project in projectsWithUser)
                {
                    if (project.Creator == userEmail)
                    {
                        _logger.LogInformation($"Deleting project with id: '{project.Id}'");
                        // If user is a creator then remove
                        await DeleteProject(project.Id.ToString(), userEmail);
                    }
                    else
                    {
                        // Remove labeller
                        var update = Builders<Project>.Update.Pull(p => p.LabellingUsers, userEmail);
                        await _projects.UpdateOneAsync(p => p.Id == project.Id, update);
                    }
                }
                return (true, "User data deleted from all projects succesfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message, ResponseProjectDto Project)> GetProject(string projectId, string userEmail)
        {
            try
            {
                var project = await _projects.Find(x => x.Id == ObjectId.Parse(projectId)).FirstAsync();

                if (project != null && (project.Creator == userEmail || project.LabellingUsers.Contains(userEmail)))
                {
                    ResponseProjectDto projectDto = new()
                    {
                        Id = project.Id.ToString(),
                        ProjectName = project.Name,
                        ProjectCreator = project.Creator,
                        ProjectDescription = project.Description,
                        LabelClasses = project.LabelClasses,
                        IsActive = project.IsActive,
                        CreationDate = project.CreationDate,
                        Images = project.ImageUrls,
                        ImageSasToken = await _blobStorage.GetContainerSASTokenAsync(projectId.ToString()),
                        UserIsOwner = project.Creator == userEmail
                    };

                    return (true, "", projectDto);
                }
                else
                {
                    return (false, "This user does not have access to this project!", new());
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, new());
            }
        }

        public async Task<(bool Result, Dictionary<string, Dictionary<string, string>> Projects)> GetProjects(string userEmail)
        {
            try
            {
                // Fetch projects where the user is a labelling user or the creator
                var projectsWithUser = await _projects.Find(x => x.LabellingUsers.Contains(userEmail) || x.Creator.Equals(userEmail)).ToListAsync();

                // Create a dictionary for each project
                var projectDict = projectsWithUser.ToDictionary(p => p.Id.ToString(), p => new Dictionary<string, string> { { "name", p.Name }, { "description", p.Description }, { "isProjectCreator", (p.Creator == userEmail).ToString() }, { "dateCreated", p.CreationDate.ToString("yyyy-MM-dd") }, { "isActive", p.IsActive.ToString() } });

                return (true, projectDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (false, new());
            }
        }

        public async Task<(bool Result, string Message)> UpdateProject(UpdateProjectDTO dto)
        {
            try
            {
                var projectId = ObjectId.Parse(dto.ProjectId);
                var project = await _projects.Find(x => x.Id == projectId).FirstAsync();
                if (project != null && project.Creator == dto.ProjectCreator)
                {
                    var update = Builders<Project>.Update
                        .Set(p => p.Name, dto.ProjectName)
                        .Set(p => p.Creator, dto.ProjectCreator)
                        .Set(p => p.Description, dto.ProjectDescription)
                        .Set(p => p.LabelClasses, dto.LabelClasses)
                        .Set(p => p.IsActive, dto.IsActive);

                    await _projects.UpdateOneAsync(p => p.Id == projectId, update);
                    return (true, "Project has been successfully updated!");
                }
                else
                {
                    return (false, "Project not found or user is not the creator");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, bool)> UserRoleInProject(string projectId, string userEmail)
        {
            var project = await _projects.Find(x => x.Id == ObjectId.Parse(projectId)).FirstAsync();
            if (project != null && (project.Creator == userEmail || project.LabellingUsers.Contains(userEmail)))
            {
                return (true, project.Creator == userEmail);
            }
            return (false, false);
        }
    }
}

