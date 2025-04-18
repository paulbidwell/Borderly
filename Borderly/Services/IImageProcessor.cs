using Borderly.Config;

namespace Borderly.Services
{
    public interface IImageProcessor
    {
        void Process(string inputPath, Profile profile);
    }
}