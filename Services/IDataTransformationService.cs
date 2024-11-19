using System.Threading.Tasks;

namespace ApiGateway.Services
{
    public interface IDataTransformationService
    {
        Task<string> TransformRequestAsync(string content, string sourceFormat, string targetFormat);
        Task<string> TransformResponseAsync(string content, string sourceFormat, string targetFormat);
    }
}
