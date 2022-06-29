
using System.IO;
using System.Threading.Tasks;

namespace RainstormTech.Storm.ImageProxy
{
    public interface IImageResizerService
    {
        Task<bool> ResizeAsync(ResizeImagePayload resizeParams, string size, string output, string mode);
    }
}
