using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using projectservice.Dto;
using projectservice.Models;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace projectservice.Services
{
    public class LabelService : ILabelService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly IMongoCollection<ImageLabels> _labels;
        private readonly IProjectService _projectService;

        public LabelService(IConfiguration config, IMongoClient mongoClient, IProjectService projectService, ILogger<ProjectService> logger)
        {
            var mongoDB = mongoClient.GetDatabase(config.GetSection("MongoDbSettings:DatabaseName").Value);
            _labels = mongoDB.GetCollection<ImageLabels>(config.GetSection("MongoDbSettings:ProjectImageLabelsCollectionName").Value);
            _config = config;
            _projectService = projectService;
            _logger = logger;
        }

        public async Task<(bool Result, string Message)> SaveImageLabels(ImageLabelsDTO dto)
        {
            try
            {
                var (IsInProject, IsProjectCreator) = await _projectService.UserRoleInProject(dto.ProjectId, dto.Creator);
                if (IsInProject == false) return (false, "User is not in the project");

                var filter = Builders<ImageLabels>.Filter.And(
                    Builders<ImageLabels>.Filter.Eq("projectId", ObjectId.Parse(dto.ProjectId)),
                    Builders<ImageLabels>.Filter.Eq("imageId", dto.ImageId),
                    Builders<ImageLabels>.Filter.Eq("creator", dto.Creator));

                var existingDocument = await _labels.Find(filter).FirstOrDefaultAsync();

                ImageLabels imageLabels;
                if (existingDocument != null)
                {
                    imageLabels = existingDocument;
                    imageLabels.Labels = dto.Labels.Select(x => ConvertDtoToLabel(x)).ToList();
                }
                else
                {
                    imageLabels = new ImageLabels
                    {
                        ProjectId = ObjectId.Parse(dto.ProjectId),
                        ImageId = dto.ImageId,
                        Creator = dto.Creator,
                        Labels = dto.Labels.Select(x => ConvertDtoToLabel(x)).ToList()
                    };
                }

                var options = new ReplaceOptions { IsUpsert = true };
                await _labels.ReplaceOneAsync(filter, imageLabels, options);

                return (true, "Image labels have been successfully saved");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public async Task<(bool Result, string Message)> DeleteAllUserLabels(string userEmail)
        {
            try
            {
                var filter = Builders<ImageLabels>.Filter.Eq("creator", userEmail);
                await _labels.DeleteManyAsync(filter);
                return (true, "All user labels have been successfully deleted");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message)> DeleteProjectLabels(string projectId, string userEmail)
        {
            try
            {
                var filter = Builders<ImageLabels>.Filter.And(
                    Builders<ImageLabels>.Filter.Eq("projectId", ObjectId.Parse(projectId)),
                    Builders<ImageLabels>.Filter.Eq("creator", userEmail));
                await _labels.DeleteManyAsync(filter);
                return (true, "Project labels have been successfully deleted");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Result, string Message, List<ImageLabelsDTO> Labels)> GetLabelsByProject(string projectId, string userEmail)
        {
            try
            {
                var (IsInProject, IsProjectCreator) = await _projectService.UserRoleInProject(projectId, userEmail);
                if (IsProjectCreator == false) return (false, "User is not the project owner", new());

                var filter = Builders<ImageLabels>.Filter.And(
                    Builders<ImageLabels>.Filter.Eq("projectId", ObjectId.Parse(projectId)),
                    Builders<ImageLabels>.Filter.Eq("creator", userEmail));
                var imageLabels = await _labels.Find(filter).ToListAsync();

                var labelsDto = imageLabels.Select(x => new ImageLabelsDTO()
                {
                    ProjectId = x.ProjectId.ToString(),
                    ImageId = x.ImageId.ToString(),
                    Creator = x.Creator,
                    Labels = x.Labels.Select(x => ConvertLabelToDto(x)).ToList(),
                }).ToList();

                return (true, "Successfully fetched labels from project", labelsDto);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, new());
            }
        }

        public async Task<(bool Result, string Message, ImageLabelsDTO Labels)> GetLabelsByProjectAndImage(string projectId, string imageId, string userEmail)
        {
            try
            {
                var (IsInProject, IsProjectCreator) = await _projectService.UserRoleInProject(projectId, userEmail);
                if (IsInProject == false) return (false, "User is not in the project", new());

                var filter = Builders<ImageLabels>.Filter.And(
                    Builders<ImageLabels>.Filter.Eq("projectId", ObjectId.Parse(projectId)),
                    Builders<ImageLabels>.Filter.Eq("imageId", imageId),
                    Builders<ImageLabels>.Filter.Eq("creator", userEmail));
                var imageLabel = await _labels.Find(filter).SingleOrDefaultAsync();

                if (imageLabel != null)
                {
                    var imageLabelsDTO = new ImageLabelsDTO()
                    {

                        ProjectId = imageLabel.ProjectId.ToString(),
                        ImageId = imageLabel.ImageId.ToString(),
                        Creator = imageLabel.Creator,
                        Labels = imageLabel.Labels.Select(x => ConvertLabelToDto(x)).ToList(),
                    };

                    return (true, "Successfully fetched labels", imageLabelsDTO);
                }
                else
                {
                    return (false, "No labels yet", new());
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, new());
            }
        }

        private static Label ConvertDtoToLabel(LabelDTO dto)
        {
            return new Label
            {
                Id = ObjectId.Parse(dto.Id),
                X = dto.X,
                Y = dto.Y,
                Height = dto.Height,
                Width = dto.Width,
                ClassName = dto.ClassName
            };
        }

        private static LabelDTO ConvertLabelToDto(Label label)
        {
            return new LabelDTO
            {
                Id = label.Id.ToString(),
                X = label.X,
                Y = label.Y,
                Height = label.Height,
                Width = label.Width,
                ClassName = label.ClassName
            };
        }
    }
}
