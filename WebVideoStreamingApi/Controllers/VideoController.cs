using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebVideoStreamingApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VideoController : ControllerBase
	{
		/*private readonly IVideoUseCase _videoUseCase;

		public VideoController(IVideoUseCase videoUseCase)
		{
			_videoUseCase = videoUseCase;
		}*/

		[HttpGet("play/{id}")]
		public async Task<IActionResult> PlayVideo(string id)
		{


			try
			{
				// Suponha que você obtenha o caminho do vídeo com base no id
				var videoPath = "./Files/audio.mp4"; // Substitua pelo caminho real do vídeo
				var videoSize = new FileInfo(videoPath).Length;

				// Parse Range
				var chunkSize = 5 * 1024 * 1024; // 5MB in bytes
				var start = 0;
				var end = Math.Min(start + chunkSize, videoSize - 1);

				var contentLength = end - start + 1;
				var headers = new
				{
					ContentRange = $"bytes {start}-{end}/{videoSize}",
					AcceptRanges = "bytes",
					ContentLength = contentLength,
					ContentType = "video/mp4" // Substitua pelo tipo MIME correto do seu vídeo
				};

				Response.Headers.Add("Content-Range", headers.ContentRange);
				Response.Headers.Add("Accept-Ranges", headers.AcceptRanges);
				Response.Headers.Add("Content-Length", headers.ContentLength.ToString());
				Response.Headers.Add("Content-Type", headers.ContentType);

				Response.StatusCode = 206;

				// Abre o arquivo de vídeo como um stream para leitura
				using (var fileStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					fileStream.Seek(start, SeekOrigin.Begin);

					var buffer = new byte[chunkSize];
					var bytesRead = 0;
					while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0 && Response.Body.CanWrite)
					{
						await Response.Body.WriteAsync(buffer, 0, bytesRead);
						await Response.Body.FlushAsync();
					}
				}

				return new EmptyResult();
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
	}
}
