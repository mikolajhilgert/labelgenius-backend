using Google.Api.Gax.ResourceNames;
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

        public async Task<(bool, string)> CreateProject(ProjectDto dto)
        {
            try
            {
                ObjectId id = ObjectId.GenerateNewId();
                List<Uri> projectImages = new();
                foreach (var (FileName, ImageAsByteArray) in dto.Images)
                {
                    projectImages.Add(await _blobStorage.UploadFileIfNotExistsAsync(ImageAsByteArray, FileName, id.ToString()));
                }
                _logger.LogInformation($"{projectImages.Count} new images have been added to '{id}' project.");
                Project project = new()
                {
                    Id = id,
                    ProjectName = dto.ProjectName,
                    ProjectCreator = dto.ProjectCreator,
                    ProjectDescription = dto.ProjectDescription,
                    LabelClasses = dto.LabelClasses,
                    ImageUrls = projectImages.Select(uri => uri.ToString()).ToList()
                };
                await _projects.InsertOneAsync(project);
                return (true, "Project has been successfully created!");
            }catch (Exception ex)
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
                if (project != null && project.ProjectCreator == userEmail)
                {
                    // Delete associated images from blob storage
                    foreach (var item in project.ImageUrls)
                    {
                        await _blobStorage.DeleteFileAsync(item);
                    }
                }
                // Delete project
                await _projects.DeleteOneAsync(projectId);
                return (true, "Project has been successfully deleted");
            } 
            catch (Exception ex) 
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message)> DeleteUserFromProjects(string userEmail)
        {
            try
            {
                var projectsWithUser = await _projects.Find(x => x.LabellingUsers.Contains(userEmail) || x.ProjectCreator.Contains(userEmail)).ToListAsync();

                foreach (var item in projectsWithUser)
                {
                    if (item.ProjectCreator.Contains(userEmail))
                    {
                        // If user is a creator then remove
                        await DeleteProject(item.Id.ToString(), userEmail);
                    }
                    else
                    {
                        // Remove labeller
                        var update = Builders<Project>.Update.Pull(p => p.LabellingUsers, userEmail);
                        await _projects.UpdateOneAsync(p => p.Id == item.Id, update);
                    }
                }
                return (true, "Project deleted succesfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message)> UpdateProject(string projectId, ProjectDto dto)
        {
            // TODO: Add updating of uploaded images
            try
            {
                var project = await _projects.Find(x => x.Id == ObjectId.Parse(projectId)).FirstAsync();
                if (project != null && project.ProjectCreator == dto.ProjectCreator)
                {
                    var update = Builders<Project>.Update
                        .Set(p => p.ProjectName, dto.ProjectName)
                        .Set(p => p.ProjectCreator, dto.ProjectCreator)
                        .Set(p => p.ProjectDescription, dto.ProjectDescription)
                        .Set(p => p.LabelClasses, dto.LabelClasses)
                        .Set(p => p.IsActive, dto.IsActive);

                    await _projects.UpdateOneAsync(p => p.Id == ObjectId.Parse(projectId), update);
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
    }
}

